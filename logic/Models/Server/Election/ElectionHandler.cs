using System.Timers;
using Logic.Models.Cluster;
using Logic.Utils;

namespace Logic.Models.Server.Election;

public class ElectionHandler
{
  ElectionData _electionData;
  ServerData _serverData;
  ClusterHandler _clusterHandler;
  public bool TheElectionIsStillGoing => !_electionData.ElectionCancellationFlag;

  public ElectionHandler(ServerData serverData, ClusterHandler clusterHandler)
  {
    _serverData = serverData;
    _clusterHandler = clusterHandler;
    _electionData = new ElectionData(initiateElection);
  }

  public void StopPreviousElection()
  {
    _electionData.ElectionCancellationFlag = true;
  }

  public void LetNewElectionRun()
  {
    _electionData.ElectionCancellationFlag = false;
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
    _electionData.NewElectionTimer(initiateElection);

    await runElection();
  }

  void discardOldVotes()
  {
    _serverData.VotesInFavorForServer = 1;
    _serverData.VotesNotInFavorForServer = 0;
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
    _electionData.ElectionTimer.Close();

    await _clusterHandler.SetFollowersNextIndexToAsync();
    _clusterHandler.StartSendingHeartbeatsToEachOtherServer();
  }

  public async Task Pause()
  {
    _serverData.StateBeforePause = _serverData.State;
    _serverData.SetState(ServerNodeState.DOWN);
    _electionData.ElectionTimer.Close();
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

    _electionData.NewElectionTimer(initiateElection, _electionData.ElectionTimer.Interval);
  }

  public void RestartElectionTimeout()
  {
    _electionData.ElectionTimer.Close();
    _electionData.NewElectionTimer(initiateElection, _electionData.ElectionTimer.Interval);
  }

  public void RestartElectionTimerWithNewInterval()
  {
    _electionData.NewElectionTimer(initiateElection);
  }
}