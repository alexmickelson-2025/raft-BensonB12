using System.Timers;

namespace logic;

public class ServerNode : IServerNode
{
  readonly int _id = GenerateKey.UniqueServerNodeId();
  public int Id => _id;
  ServerNodeState _state = ServerNodeState.FOLLOWER;
  public ServerNodeState State => _state;
  int _term = 0;
  public int Term => _term;
  System.Timers.Timer _electionTimeOut = new();
  public int ElectionTimerInterval => (int)_electionTimeOut.Interval;
  readonly List<IServerNode> _otherServerNodesInCluster = [];
  readonly List<Thread> _heartbeatThreads = [];
  int? _clusterLeaderId;
  public int? ClusterLeaderId => _clusterLeaderId;
  bool _electionCancellationFlag = false;

  public ServerNode()
  {
    initializeServerNode();
  }

  public ServerNode(int id)
  {
    _id = id;
    initializeServerNode();
  }

  void initializeServerNode()
  {
    _electionTimeOut = newElectionTimer();
    _electionTimeOut.Elapsed += electionTimedOutProcedure;
  }

  public void AddServersToServersCluster(IEnumerable<IServerNode> otherServers)
  {
    _otherServerNodesInCluster.AddRange(otherServers);
  }

  void electionTimedOutProcedure(object? sender, ElapsedEventArgs e)
  {
    if (_state == ServerNodeState.CANDIDATE)
    {
      _electionCancellationFlag = true;
    }
    else
    {
      _state = ServerNodeState.CANDIDATE;
    }

    _term++;
    _electionTimeOut = newElectionTimer();
    runElectionForYourself();
  }

  System.Timers.Timer newElectionTimer()
  {
    int randomElectionTime = Random.Shared.Next(Constants.INCLUSIVE_MINIMUM_ELECTION_TIME, Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME);

    return new(randomElectionTime)
    {
      AutoReset = false,
      Enabled = true
    };
  }

  void runElectionForYourself()
  {
    //if (hasMajorityVotes())
    {
      Thread.Sleep(300);
      becomeLeader();
    }
  }

  void becomeLeader()
  {
    _state = ServerNodeState.LEADER;
    _clusterLeaderId = _id;

    foreach (ServerNode server in _otherServerNodesInCluster)
    {
      Thread thread = new(() => runSendHeartbeatsToServerNodeAsync(server));
      thread.Start();
      _heartbeatThreads.Add(thread);
    }
  }

  async void runSendHeartbeatsToServerNodeAsync(ServerNode server)
  {
    while (_state == ServerNodeState.LEADER)
    {
      await sendHeartbeatsToServerNodeAsync(server);
      Thread.Sleep(Constants.HEARTBEAT_PAUSE);
    }
  }

  async Task sendHeartbeatsToServerNodeAsync(ServerNode server)
  {
    HeartbeatArguments heartbeatArguments = new()
    {
      Term = _term,
      ServerNodeId = _id
    };

    await server.ReceiveHeartBeat(heartbeatArguments);
  }

  public async Task ReceiveHeartBeat(HeartbeatArguments arguments)
  {
    _electionTimeOut.Stop();
    _electionTimeOut.Start();

    ServerNodeState oldState = _state;
    _state = ServerNodeState.FOLLOWER;
    _clusterLeaderId = arguments.ServerNodeId;

    if (oldState == ServerNodeState.LEADER)
    {
      stopAllHeartBeatThreads();
    }

    _term = arguments.Term;

    await Task.CompletedTask;
  }

  void stopAllHeartBeatThreads()
  {
    foreach (Thread thread in _heartbeatThreads)
    {
      // I am worried this might not work and we will need a cancellation token
      thread.Join();
    }
  }

  async Task<bool> hasMajorityVotes()
  {
    _electionCancellationFlag = false;
    int numberOfNodes = _otherServerNodesInCluster.Count() + 1;
    int numberOfVotesForMyself = 1;
    int numberOfVotesRejected = 0;
    int majority = numberOfNodes / 2;

    while (numberOfVotesForMyself < majority && numberOfVotesRejected < majority && !_electionCancellationFlag)
    {
      // Call The others
    }

    return numberOfVotesForMyself > majority;

    // Do I become a follower? Or do I wait for a heartbeat?
  }

  public void KillServer()
  {
    _state = ServerNodeState.FOLLOWER;
    stopAllHeartBeatThreads();
  }

  void IServerNode.initializeServerNode()
  {
    initializeServerNode();
  }

  public void iunElectionForYourself()
  {
    throw new NotImplementedException();
  }
}
