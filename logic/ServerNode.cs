using System.Timers;

namespace Logic;

public class ServerNode : IServerNode
{
  int _votesForMyself = 1;
  int _votesRejected = 0;
  readonly int _id = Utils.GenerateUniqueServerNodeId();
  public int Id => _id;
  ServerNodeState _state = ServerNodeState.FOLLOWER;
  ServerNodeState? _stateBeforePause;
  public ServerNodeState State => _state;
  uint _term = 0;
  public uint Term => _term;
  readonly Dictionary<uint, bool> _hasVotedInTerm = new() { { 0, false } };
  System.Timers.Timer _electionTimer = Utils.NewElectionTimer();
  public System.Timers.Timer ElectionTimer => _electionTimer;
  List<IServerNode> _otherServerNodesInCluster = [];
  List<Thread> _heartbeatThreads = [];
  int? _clusterLeaderId;
  public int? ClusterLeaderId => _clusterLeaderId;
  bool _electionCancellationFlag = false;

  public ServerNode()
  {
    addElectionTimeOutProcedureEventToElectionTimer();
  }

  public ServerNode(int id)
  {
    _id = id;
    addElectionTimeOutProcedureEventToElectionTimer();
  }

  public ServerNode(IEnumerable<IServerNode> otherServers)
  {
    addElectionTimeOutProcedureEventToElectionTimer();
    AddServersToServersCluster(otherServers);
  }

  void addElectionTimeOutProcedureEventToElectionTimer()
  {
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
    restartElectionTimerWithNewRandomInterval();

    await runElectionForYourselfAsync();
  }

  void restartElectionFields()
  {
    _votesForMyself = 1;
    _votesRejected = 0;
  }

  void restartElectionTimerWithNewRandomInterval()
  {
    _electionTimer = Utils.NewElectionTimer();
    addElectionTimeOutProcedureEventToElectionTimer();
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
    _electionTimer.Stop();

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
      await sendHeartbeatToServerNodeAsync(server);
      Thread.Sleep(Constants.HEARTBEAT_PAUSE);
    }
  }

  async Task sendHeartbeatToServerNodeAsync(IServerNode server)
  {
    LeaderToFollowerRemoteProcedureCallArguments heartbeatArguments = new(_id, _term);

    await server.ReceiveLeaderToFollowerRemoteProcedureCallAsync(heartbeatArguments);
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

  public async Task ThrowBalletForAsync(int id, uint term)
  {
    if (_hasVotedInTerm.TryGetValue(term, out var value) && value)
    {
      await registerVoteToServerAsync(false, id, term);
    }
    else
    {
      await registerVoteToServerAsync(true, id, term);
    }
  }

  async Task petitionServerForSupport(int id)
  {
    IServerNode? receivingNode = _otherServerNodesInCluster.SingleOrDefault(n => n.Id == id);

    if (receivingNode is not null)
    {
      await receivingNode.ThrowBalletForAsync(_id, _term); // Can only ask for a vote once per term
    }
  }

  async Task registerVoteToServerAsync(bool inFavor, int id, uint newTerm)
  {
    IServerNode? receivingNode = _otherServerNodesInCluster.SingleOrDefault(n => n.Id == id);

    if (receivingNode is not null)
    {
      await receivingNode.AcceptVoteAsync(inFavor); // Can only send the vote once per term
      _hasVotedInTerm[newTerm] = true;
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

  public void PauseServer()
  {
    _stateBeforePause = _state;
    _state = ServerNodeState.DOWN;
    _electionTimer.Stop();
    stopAllHeartBeatThreads();
  }

  public void UnpauseServer()
  {
    if (_stateBeforePause is null)
    {
      return;
    }
    _state = (ServerNodeState)_stateBeforePause;
    _stateBeforePause = null;

    if (_state != ServerNodeState.LEADER)
    {
      becomeLeader();
      return;
    }

    ElectionTimer.Start();
  }

  public async Task ReceiveLeaderToFollowerRemoteProcedureCallAsync(LeaderToFollowerRemoteProcedureCallArguments arguments)
  {
    IServerNode? leaderNode = _otherServerNodesInCluster.SingleOrDefault(server => server.Id == arguments.ServerNodeId);
    // Throw an error if leaderNode is null

    if (leaderNode is null)
    {
      // Throw an error?
      return;
    }

    if (arguments.Term < _term)
    {
      await leaderNode.LeaderToFollowerRemoteProcedureCallResponse(_id, false);
    }

    // If the term is the same, we have some corner cases to solve

    _clusterLeaderId = arguments.ServerNodeId;
    _electionTimer.Stop();
    _electionTimer.Start();

    ServerNodeState oldState = _state;
    _state = ServerNodeState.FOLLOWER;
    // Maybe I should validate that the serverNodeId is in my cluster

    if (oldState == ServerNodeState.LEADER)
    {
      // Handle if our terms match
      stopAllHeartBeatThreads();
      restartElectionTimerWithNewRandomInterval();
    }

    _term = arguments.Term;
    restartElectionFields();

    await leaderNode.LeaderToFollowerRemoteProcedureCallResponse(_id, true);
  }

  public async Task LeaderToFollowerRemoteProcedureCallResponse(int id, bool rejected)
  {
    await Task.CompletedTask;
  }
}
