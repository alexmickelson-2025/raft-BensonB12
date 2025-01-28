using System.Timers;
using Logic.Exceptions;

namespace Logic;

// TODO: Refactor this class down
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
  List<IServerNode> _otherServersInCluster = [];
  List<Thread> _heartbeatThreads = [];
  int? _clusterLeaderId;
  public int? ClusterLeaderId => _clusterLeaderId;
  bool _electionCancellationFlag = false;
  Logs _logs = [];
  public Logs Logs => _logs;
  Dictionary<int, int> _followerToNextIndex = [];
  public Dictionary<int, int> FollowerToNextIndex => _followerToNextIndex; // Throw an error if I am not the leader

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
    AddServersToCluster(otherServers);
  }

  void addElectionTimeOutProcedureEventToElectionTimer()
  {
    _electionTimer.Elapsed += async (sender, e) => await initiateElection(sender, e);
  }

  public void AddServersToCluster(IEnumerable<IServerNode> otherServers)
  {
    foreach (IServerNode server in otherServers)
    {
      _followerToNextIndex[server.Id] = 0;
    }

    _otherServersInCluster.AddRange(otherServers);
  }

  async Task initiateElection(object? _, ElapsedEventArgs __)
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
    _hasVotedInTerm[_term] = true; // TODO: add to to make sure it does not vote for others
    discardOldVotes();
    restartElectionTimerWithNewInterval();

    await runElection();
  }

  void discardOldVotes()
  {
    _votesForMyself = 1;
    _votesRejected = 0;
  }

  void restartElectionTimerWithNewInterval()
  {
    _electionTimer = Utils.NewElectionTimer();
    addElectionTimeOutProcedureEventToElectionTimer();
  }

  async Task runElection()
  {
    await petitionOtherServersForVote();
    waitForEnoughVotesFromOtherServers();

    if (hasMajorityInFavorVotes())
    {
      becomeLeader();
    }

    // Do I become a follower here? Do I stay a candidate?
  }

  async Task petitionOtherServersForVote()
  {
    // Cannot do foreach because of they will update (Threading)
    // TODO: Fix using a lock
    for (int i = 0; i < _otherServersInCluster.Count; i++)
    {
      await _otherServersInCluster[i].RegisterVoteForAsync(_id, _term);
    }
  }

  public async Task RegisterVoteForAsync(int id, uint term)
  {
    if (_state == ServerNodeState.DOWN)
    {
      return;
    }

    if (_hasVotedInTerm.TryGetValue(term, out var value) && value)
    {
      await registerVoteToCandidateAsync(false, id, term);
    }
    else
    {
      await registerVoteToCandidateAsync(true, id, term);
    }
  }

  async Task registerVoteToCandidateAsync(bool inFavor, int id, uint newTerm)
  {
    IServerNode? candidate = _otherServersInCluster.SingleOrDefault(n => n.Id == id);

    if (candidate is null) // TODO: Test this
    {
      throw new ClusterDidNotContainServerException(id);
    }

    await candidate.CountVoteAsync(inFavor);
    _hasVotedInTerm[newTerm] = true;
  }

  public async Task CountVoteAsync(bool inFavor) // Does not care who sent the vote, the servers are restricted to only vote once per term. Maybe I should take the term then?
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

  void waitForEnoughVotesFromOtherServers()
  {
    _electionCancellationFlag = false;
    int numberOfNodes = _otherServersInCluster.Count + 1;
    int majority = (numberOfNodes / 2) + 1;

    // TODO: re-write the wile loops that are sucking up CPU. I want to do an event listener ...
    while (_votesForMyself < majority && _votesRejected < majority && !_electionCancellationFlag)
    {
      // Wait for calls
    }
  }

  bool hasMajorityInFavorVotes()
  {
    int numberOfNodes = _otherServersInCluster.Count + 1;
    int majority = (numberOfNodes / 2) + 1;

    return _votesForMyself >= majority;

    // Do I become a follower? Or do I wait for a heartbeat?
  }

  void becomeLeader()
  {
    _state = ServerNodeState.LEADER;
    _clusterLeaderId = _id;
    _electionTimer.Stop();

    // Watchout for updates to _otherServerNodesInCluster or the servers in them, it will throw an exception
    // Should probably test and then catch and handle cases for that
    // Probably a lock if I where to guess
    foreach (IServerNode server in _otherServersInCluster)
    {
      server.SetNextIndexToAsync(new SetNextIndexToArgs(_id, _term, _logs.NextIndex));
      // If any respond with a rejection, I need to handle that soon
      Thread thread = new(() => runSendHeartbeatsToServerAsync(server));
      thread.Start();
      _heartbeatThreads.Add(thread);
    }
  }

  async void runSendHeartbeatsToServerAsync(IServerNode server)
  {
    while (_state == ServerNodeState.LEADER)
    {
      await sendHeartbeatToServerAsync(server);
      Thread.Sleep(Constants.HEARTBEAT_PAUSE);
    }
  }

  async Task sendHeartbeatToServerAsync(IServerNode server)
  {
    RPCFromLeaderArgs heartbeatArgs = new(_id, _term);

    await server.RPCFromLeaderAsync(heartbeatArgs);
  }

  // TODO: Refactor this method down
  public async Task RPCFromLeaderAsync(RPCFromLeaderArgs args)
  {
    if (_state == ServerNodeState.DOWN)
    {
      return;
    }

    IServerNode? leaderNode = _otherServersInCluster.SingleOrDefault(server => server.Id == args.ServerId);
    // Throw an error if leaderNode is null

    if (leaderNode is null)
    {
      throw new ClusterDidNotContainServerException(args.ServerId);
    }

    if (args.Term < _term)
    {
      await leaderNode.RPCFromFollowerAsync(_id, false);
      return;
    }

    // If the term is the same, we have some corner cases to solve

    _clusterLeaderId = args.ServerId;
    _electionTimer.Stop();
    _electionTimer.Start();

    ServerNodeState oldState = _state;
    _state = ServerNodeState.FOLLOWER;
    // Maybe I should validate that the serverNodeId is in my cluster

    if (oldState == ServerNodeState.LEADER)
    {
      // Handle if our terms match
      stopAllHeartBeatThreads();
      restartElectionTimerWithNewInterval();
    }

    _term = args.Term;

    await leaderNode.RPCFromFollowerAsync(_id, true);
  }

  public async Task RPCFromFollowerAsync(int id, bool rejected)
  {
    await Task.CompletedTask;
  }

  public void Pause()
  {
    _stateBeforePause = _state;
    _state = ServerNodeState.DOWN;
    _electionTimer.Stop();
    stopAllHeartBeatThreads();
  }

  public void Unpause()
  {
    if (_stateBeforePause is null)
    {
      throw new UnpausedARunningServerException();
    }

    _state = (ServerNodeState)_stateBeforePause;
    _stateBeforePause = null;

    if (_state == ServerNodeState.LEADER)
    {
      becomeLeader();
      return;
    }

    ElectionTimer.Start();
  }

  void stopAllHeartBeatThreads()
  {
    foreach (Thread thread in _heartbeatThreads)
    {
      // I am worried this might not work and we will need a cancellation token
      thread.Join();
    }
  }

  public async Task AppendLogRPCAsync(string log)
  {
    // TODO: Make sure I am the leader
    appendLog(_term, log);
    RPCFromLeaderArgs appendLogArgs = new(_id, _term, log, _logs.NextIndex);

    foreach (IServerNode server in _otherServersInCluster)
    {
      await server.RPCFromLeaderAsync(appendLogArgs);
    }
  }

  void appendLog(uint term, string log)
  {
    _logs.Add(term, log);
  }

  public async Task SetNextIndexToAsync(SetNextIndexToArgs args)
  {
    await Task.CompletedTask;
    _logs.SetNextIndexTo(args.NextIndex);
  }
}
