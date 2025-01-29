using System.Timers;
using Logic.Exceptions;
using Logic.Models.Args;
using Logic.Models.Cluster;
using Logic.Models.Server.Logging;
using Logic.Utils;

namespace Logic.Models.Server;

public class ServerNode : IServerNode
{
  ServerData _serverData = new();
  ClusterHandler _clusterHandler = null!;
  public int Id => _serverData.Id;
  public ServerNodeState State => _serverData.State;
  public uint Term => _serverData.Term;
  System.Timers.Timer _electionTimer = Util.NewElectionTimer();
  public System.Timers.Timer ElectionTimer => _electionTimer;
  public int? ClusterLeaderId => _clusterHandler.ClusterLeaderId;
  bool _electionCancellationFlag = false;

  public ServerNode()
  {
    initializeServer([]);
  }

  public ServerNode(int id)
  {
    _serverData = new ServerData(id);
    initializeServer([]);
  }

  public ServerNode(IEnumerable<IServerNode> otherServers)
  {
    initializeServer(otherServers);
  }

  public ServerNode(int id, IEnumerable<IServerNode> otherServers)
  {
    _serverData = new ServerData(id);
    initializeServer(otherServers);
  }

  void initializeServer(IEnumerable<IServerNode> otherServers)
  {
    addElectionTimeOutProcedureEventToElectionTimer();
    InitializeClusterWithServers(otherServers);
  }

  void addElectionTimeOutProcedureEventToElectionTimer()
  {
    _electionTimer.Elapsed += async (sender, e) => await initiateElection(sender, e);
  }

  public void InitializeClusterWithServers(IEnumerable<IServerNode> otherServers)
  {
    if (_clusterHandler is not null && !_clusterHandler.ClusterIsEmpty)
    {
      throw new ClusterAlreadyExistsException();
    }

    _clusterHandler = new ClusterHandler(otherServers, _serverData);
  }

  async Task initiateElection(object? _, ElapsedEventArgs __)
  {
    if (_serverData.ServerIsACandidate())
    {
      stopPreviousElection();
    }
    else
    {
      _serverData.SetState(ServerNodeState.CANDIDATE);
    }

    _serverData.Term++;
    _serverData.HasVotedInTerm[_serverData.Term] = true; // TODO: add to to make sure it does not vote for others
    _clusterHandler.DiscardOldVotes();
    restartElectionTimerWithNewInterval();

    await runElection();
  }

  void stopPreviousElection()
  {
    _electionCancellationFlag = true;
  }

  void restartElectionTimerWithNewInterval()
  {
    _electionTimer = Util.NewElectionTimer();
    addElectionTimeOutProcedureEventToElectionTimer();
  }

  async Task runElection()
  {
    await _clusterHandler.PetitionOtherServersToVoteAsync();
    letNewElectionRun();
    _clusterHandler.WaitForEnoughVotesFromOtherServers(theElectionIsStillGoing()); // I might need to pass by reference

    if (_clusterHandler.HasMajorityInFavorVotes())
    {
      await becomeLeader();
    }

    // Do I become a follower here? Do I stay a candidate?
  }

  public async Task RegisterVoteForAsync(int id, uint term)
  {
    await _clusterHandler.RegisterVoteForAsync(id, term);
  }

  public async Task CountVoteAsync(bool inFavor) // Does not care who sent the vote, the servers are restricted to only vote once per term. Maybe I should take the term then?
  {
    await _clusterHandler.CountVoteAsync(inFavor);
  }

  void letNewElectionRun()
  {
    _electionCancellationFlag = false;
  }

  bool theElectionIsStillGoing()
  {
    return !_electionCancellationFlag;
  }

  async Task becomeLeader()
  {
    _serverData.SetState(ServerNodeState.LEADER);
    _clusterHandler.ClusterLeaderId = _serverData.Id;
    _electionTimer.Stop();

    await _clusterHandler.SetFollowersNextIndexToAsync();
    _clusterHandler.StartSendingHeartbeatsToEachOtherServer();
  }

  // TODO: Refactor this method down
  public async Task RPCFromLeaderAsync(RPCFromLeaderArgs args)
  {
    if (_serverData.ServerIsDown())
    {
      return;
    }

    IServerNode leaderServer = _clusterHandler.GetServer(args.ServerId);

    if (args.Term < _serverData.Term)
    {
      await leaderServer.RPCFromFollowerAsync(_serverData.Id, false);
      return;
    }

    // If the term is the same, we have some corner cases to solve

    _clusterHandler.ClusterLeaderId = args.ServerId;
    _electionTimer.Stop();
    _electionTimer.Start();

    ServerNodeState oldState = _serverData.State;
    _serverData.SetState(ServerNodeState.FOLLOWER);
    // Maybe I should validate that the serverNodeId is in my cluster

    if (oldState == ServerNodeState.LEADER)
    {
      // Handle if our terms match
      _clusterHandler.StopAllHeartBeatThreads();
      restartElectionTimerWithNewInterval();
    }

    _serverData.Term = args.Term;

    await leaderServer.RPCFromFollowerAsync(_serverData.Id, true);
  }

  public async Task RPCFromFollowerAsync(int id, bool rejected)
  {
    await _clusterHandler.RPCFromFollowerAsync(id, rejected);
  }

  public void Pause()
  {
    _serverData.StateBeforePause = _serverData.State;
    _serverData.SetState(ServerNodeState.DOWN);
    _electionTimer.Stop();
    _clusterHandler.StopAllHeartBeatThreads();
  }

  public async Task Unpause()
  {
    _serverData.SetState(_serverData.StateBeforePause);
    _serverData.StateBeforePause = null;

    if (_serverData.ServerIsTheLeader())
    {
      await becomeLeader();
      return;
    }

    ElectionTimer.Start();
  }

  public async Task AppendLogRPCAsync(string log)
  {
    if (_serverData.ServerIsDown())
    {
      return;
    }

    // TODO: Make sure I am the leader
    appendLog(_serverData.Term, log);

    await _clusterHandler.SendRPCFromLeaderToEachFollowerAsync(log);
  }

  void appendLog(uint term, string log)
  {
    _serverData.AddToLocalMemory(term, log);
  }

  public async Task SetNextIndexToAsync(SetNextIndexToArgs args)
  {
    await Task.CompletedTask;
    _serverData.SetNextIndexTo(args.NextIndex);
  }
}
