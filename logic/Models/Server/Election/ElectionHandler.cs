using System.Timers;
using Logic.Models.Cluster;
using Logic.Utils;

namespace Logic.Models.Server.Election;

public class ElectionHandler
{
  ElectionData _electionData = new();
  ServerData _serverData;
  ClusterHandler _clusterHandler;
  public bool TheElectionIsStillGoing => !_electionData.ElectionCancellationFlag;

  public ElectionHandler(ServerData serverData, ClusterHandler clusterHandler)
  {
    _serverData = serverData;
    _clusterHandler = clusterHandler;
  }

  public void StopPreviousElection()
  {
    _electionData.ElectionCancellationFlag = true;
  }

  public void LetNewElectionRun()
  {
    _electionData.ElectionCancellationFlag = false;
  }

  public void AddElectionTimeOutProcedureEventToElectionTimer()
  {
    _electionData.ElectionTimer.Elapsed += async (sender, e) => await initiateElection(sender, e);
  }

  async Task initiateElection(object? _, ElapsedEventArgs __)
  {
    if (_serverData.ServerIsACandidate())
    {
      StopPreviousElection();
    }
    else
    {
      _serverData.SetState(ServerNodeState.CANDIDATE);
    }

    _serverData.Term++;
    _serverData.HasVotedInTerm[_serverData.Term] = true; // TODO: add to to make sure it does not vote for others
    discardOldVotes();
    RestartElectionTimerWithNewInterval();

    await runElection();
  }

  void discardOldVotes()
  {
    _serverData.VotesInFavorForServer = 1;
    _serverData.VotesNotInFavorForServer = 0;
  }

  public void RestartElectionTimerWithNewInterval()
  {
    _electionData.ElectionTimer = Util.NewElectionTimer();
    AddElectionTimeOutProcedureEventToElectionTimer();
  }

  async Task runElection()
  {
    await _clusterHandler.PetitionOtherServersToVoteAsync();
    LetNewElectionRun();
    _clusterHandler.WaitForEnoughVotesFromOtherServers(TheElectionIsStillGoing); // I might need to pass by reference

    if (_clusterHandler.HasMajorityInFavorVotes())
    {
      await becomeLeader();
    }

    // Do I become a follower here? Do I stay a candidate?
  }

  async Task becomeLeader()
  {
    _serverData.SetState(ServerNodeState.LEADER);
    _clusterHandler.ClusterLeaderId = _serverData.Id;
    _electionData.ElectionTimer.Stop();

    await _clusterHandler.SetFollowersNextIndexToAsync();
    _clusterHandler.StartSendingHeartbeatsToEachOtherServer();
  }

  public async Task Pause()
  {
    _serverData.StateBeforePause = _serverData.State;
    _serverData.SetState(ServerNodeState.DOWN);
    _electionData.ElectionTimer.Stop();
    _clusterHandler.StopAllHeartBeatThreads();
    await Task.CompletedTask;
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

    _electionData.ElectionTimer.Start();
  }

  public void RestartElectionTimeout()
  {
    _electionData.ElectionTimer.Stop();
    _electionData.ElectionTimer.Start();
  }
}