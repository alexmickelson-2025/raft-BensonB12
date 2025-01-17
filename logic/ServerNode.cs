using System.Timers;

namespace logic;

public class ServerNode : IServerNode
{
  int _votesForMyself = 1;
  int _votesRejected = 0;
  readonly int _id = Utils.GenerateUniqueServerNodeId();
  public int Id => _id;
  ServerNodeState _state = ServerNodeState.FOLLOWER;
  public ServerNodeState State => _state;
  int _term = 0;
  public int Term => _term;
  System.Timers.Timer _electionTimer = Utils.NewElectionTimer();
  public int ElectionTimerInterval => (int)_electionTimer.Interval;
  readonly List<IServerNode> _otherServerNodesInCluster = [];
  readonly List<Thread> _heartbeatThreads = [];
  int? _clusterLeaderId;
  public int? ClusterLeaderId => _clusterLeaderId;
  bool _electionCancellationFlag = false;

  public ServerNode()
  {
    _electionTimer.Elapsed += async (sender, e) => await electionTimedOutProcedureAsync(sender, e);
  }

  public ServerNode(int id)
  {
    _id = id;
    _electionTimer.Elapsed += async (sender, e) => await electionTimedOutProcedureAsync(sender, e);
  }

  public void AddServersToServersCluster(IEnumerable<IServerNode> otherServers)
  {
    _otherServerNodesInCluster.AddRange(otherServers);
  }

  async Task electionTimedOutProcedureAsync(object? sender, ElapsedEventArgs e)
  {
    if (_state == ServerNodeState.CANDIDATE)
    {
      _electionCancellationFlag = true;
    }
    else
    {
      _state = ServerNodeState.CANDIDATE;
    }

    restartElectionFields();

    await runElectionForYourselfAsync();
  }

  void restartElectionFields()
  {
    _term++;
    _votesForMyself = 1;
    _votesRejected = 0;
    _electionTimer = Utils.NewElectionTimer();
  }

  async Task runElectionForYourselfAsync()
  {
    if (await hasMajorityVotesAsync())
    {
      becomeLeader();
    }
  }

  void becomeLeader()
  {
    _state = ServerNodeState.LEADER;
    _clusterLeaderId = _id;

    foreach (IServerNode server in _otherServerNodesInCluster)
    {
      Thread thread = new(() => runSendHeartbeatsToServerNodeAsync(server));
      thread.Start();
      _heartbeatThreads.Add(thread);
    }
  }

  async void runSendHeartbeatsToServerNodeAsync(IServerNode server)
  {
    while (_state == ServerNodeState.LEADER)
    {
      await sendHeartbeatsToServerNodeAsync(server);
      Thread.Sleep(Constants.HEARTBEAT_PAUSE);
    }
  }

  async Task sendHeartbeatsToServerNodeAsync(IServerNode server)
  {
    HeartbeatArguments heartbeatArguments = new()
    {
      Term = _term,
      ServerNodeId = _id
    };

    await server.ReceiveHeartBeatAsync(heartbeatArguments);
  }

  public async Task ReceiveHeartBeatAsync(HeartbeatArguments arguments)
  {
    _electionTimer.Stop();
    _electionTimer.Start();

    ServerNodeState oldState = _state;
    _state = ServerNodeState.FOLLOWER;
    _clusterLeaderId = arguments.ServerNodeId;

    if (oldState == ServerNodeState.LEADER)
    {
      stopAllHeartBeatThreads();
    }

    restartElectionFields();

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

  async Task<bool> hasMajorityVotesAsync()
  {
    _electionCancellationFlag = false;
    int numberOfNodes = _otherServerNodesInCluster.Count + 1;
    int majority = (numberOfNodes / 2) + 1;

    foreach (IServerNode server in _otherServerNodesInCluster)
    {
      await AskForVoteAsync(server.Id);
    }

    while (_votesForMyself < majority && _votesRejected < majority && !_electionCancellationFlag)
    {
      // Wait for calls
    }

    return _votesForMyself > majority;

    // Do I become a follower? Or do I wait for a heartbeat?
  }

  public async Task AskForVoteAsync(int id)
  {
    // If I have not voted this term
    await sendVoteToServerAsync(true, id); // Can only ask for votes once per term
  }

  async Task sendVoteToServerAsync(bool inFavor, int id)
  {
    IServerNode? receivingNode = _otherServerNodesInCluster.SingleOrDefault(n => n.Id == id);

    if (receivingNode is not null)
    {
      await receivingNode.AcceptVoteAsync(inFavor); // Can only send the vote once per term
    }
  }

  public async Task AcceptVoteAsync(bool inFavor) // Does not care who sent the vote 
  {
    await Task.CompletedTask;

    if (inFavor)
    {
      _votesForMyself++;
    }
    else
    {
      _votesRejected++;
    }
  }

  public void KillServer()
  {
    _state = ServerNodeState.DOWN;
    stopAllHeartBeatThreads();
  }
}
