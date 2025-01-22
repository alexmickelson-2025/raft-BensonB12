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
  readonly Dictionary<int, bool> _hasVotedInTerm = new() { { 0, false } };
  System.Timers.Timer _electionTimer = Utils.NewElectionTimer();
  public System.Timers.Timer ElectionTimer => _electionTimer;
  List<IServerNode> _otherServerNodesInCluster = [];
  List<Thread> _heartbeatThreads = [];
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

    _term++;
    _hasVotedInTerm[_term] = false;
    restartElectionFields();

    await runElectionForYourselfAsync();
  }

  void restartElectionFields()
  {
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

    // Watchout for updates to _otherServerNodesInCluster or the servers in them, it will throw an exception
    // Should probably test and then catch and handle cases for that
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
    HeartbeatArguments heartbeatArguments = new(_term, _id);

    await server.ReceiveHeartBeatAsync(heartbeatArguments);
  }

  public async Task ReceiveHeartBeatAsync(HeartbeatArguments arguments)
  {
    _electionTimer.Stop();
    _electionTimer.Start();

    ServerNodeState oldState = _state;
    _state = ServerNodeState.FOLLOWER;
    // Maybe I should validate that the serverNodeId is in my cluster
    _clusterLeaderId = arguments.ServerNodeId;

    if (oldState == ServerNodeState.LEADER)
    {
      stopAllHeartBeatThreads();
    }

    _term = arguments.Term;
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

    // Cannot do foreach because of they will update (Threading)
    for (int i = 0; i < _otherServerNodesInCluster.Count; i++)
    {
      await petitionServerForSupport(_otherServerNodesInCluster[i].Id);
    }

    while (_votesForMyself < majority && _votesRejected < majority && !_electionCancellationFlag)
    {
      // Wait for calls
    }

    return _votesForMyself >= majority;

    // Do I become a follower? Or do I wait for a heartbeat?
  }

  public async Task ThrowBalletFor(int id)
  {
    if (_hasVotedInTerm[_term])
    {
      await registerVoteToServerAsync(true, id);
    }
    else
    {
      await registerVoteToServerAsync(false, id);
    }
  }

  async Task petitionServerForSupport(int id)
  {
    IServerNode? receivingNode = _otherServerNodesInCluster.SingleOrDefault(n => n.Id == id);

    if (receivingNode is not null)
    {
      await receivingNode.ThrowBalletFor(_id); // Can only ask for a vote once per term
    }
  }

  async Task registerVoteToServerAsync(bool inFavor, int id)
  {
    IServerNode? receivingNode = _otherServerNodesInCluster.SingleOrDefault(n => n.Id == id);

    if (receivingNode is not null)
    {
      await receivingNode.AcceptVoteAsync(inFavor); // Can only send the vote once per term
      _hasVotedInTerm[_term] = true;
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
