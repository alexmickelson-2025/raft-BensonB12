using Logic.Models.Args;
using Logic.Models.Server;
using Logic.Utils;

namespace Logic.Models.Cluster;

public class OutGoingCallsClusterHandler
{
  ClusterData _clusterData;
  public OutGoingCallsClusterHandler(ClusterData clusterData)
  {
    _clusterData = clusterData;
  }

  public async Task PetitionOtherServersToVoteAsync()
  {
    // Cannot do foreach because of they will update (Threading)
    // TODO: Fix using a lock
    for (int i = 0; i < _clusterData.OtherServersInCluster.Count; i++)
    {
      await _clusterData.OtherServersInCluster[i].RegisterVoteForAsync(_clusterData.ServerData.Id, _clusterData.ServerData.Term);
    }
  }

  public void WaitForEnoughVotesFromOtherServers(bool theElectionIsStillGoing)
  {
    // TODO: re-write the wile loops that are sucking up CPU. I want to do an event listener ...
    while (thereAreNotEnoughVotes() && theElectionIsStillGoing)
    {
      // Wait for calls
    }
  }

  bool thereAreNotEnoughVotes()
  {
    int numberOfNodes = _clusterData.OtherServersInCluster.Count + 1;
    int majority = (numberOfNodes / 2) + 1;

    return _clusterData.ServerData.VotesInFavorForServer < majority && _clusterData.ServerData.VotesNotInFavorForServer < majority;
  }

  public void StartSendingHeartbeatsToEachOtherServer()
  {
    // Watchout for updates to _otherServerNodesInCluster or the servers in them, it will throw an exception
    // Should probably test and then catch and handle cases for that
    // Probably a lock if I where to guess
    foreach (IServerNode server in _clusterData.OtherServersInCluster)
    {
      Thread thread = new(() => runSendHeartbeatsToServerAsync(server));
      thread.Start();
      _clusterData.HeartbeatThreads.Add(thread);
    }
  }

  async void runSendHeartbeatsToServerAsync(IServerNode server)
  {
    while (_clusterData.ServerData.ServerIsTheLeader())
    {
      await sendHeartbeatToServerAsync(server);
      Thread.Sleep(Constants.HEARTBEAT_PAUSE);
    }
  }

  async Task sendHeartbeatToServerAsync(IServerNode server)
  {
    // NOT RIGHT NEED TO FIX TODO
    RPCFromLeaderArgs heartbeatArgs = new(
      leaderId: _clusterData.ServerData.Id,
          term: _clusterData.ServerData.Term,
          previousLogIndex: _clusterData.ServerData.NextIndex,
          previousLogTerm: _clusterData.ServerData.Term,
          leadersLastCommitIndex: _clusterData.ServerData.NextIndex,
          newLogs: []
    );

    await server.RPCFromLeaderAsync(heartbeatArgs);
  }

  public void StopAllHeartBeatThreads()
  {
    foreach (Thread thread in _clusterData.HeartbeatThreads)
    {
      // I am worried this might not work and we will need a cancellation token
      thread.Join();
    }
  }

  public bool HasMajorityInFavorVotes()
  {
    int numberOfNodes = _clusterData.OtherServersInCluster.Count + 1;
    int majority = (numberOfNodes / 2) + 1;

    return _clusterData.ServerData.VotesInFavorForServer >= majority;

    // Do I become a follower? Or do I wait for a heartbeat?
  }

  public async Task SetFollowersNextIndexToAsync(int nextIndex)
  {
    // Watchout for updates to _otherServerNodesInCluster or the servers in them, it will throw an exception
    // Should probably test and then catch and handle cases for that
    // Probably a lock if I where to guess
    foreach (IServerNode server in _clusterData.OtherServersInCluster)
    {
      // NOT RIGHT NEED TO FIX TODO
      await server.RPCFromLeaderAsync(
        new RPCFromLeaderArgs(
          leaderId: _clusterData.ServerData.Id,
          term: _clusterData.ServerData.Term,
          previousLogIndex: _clusterData.ServerData.NextIndex,
          previousLogTerm: _clusterData.ServerData.Term,
          leadersLastCommitIndex: _clusterData.ServerData.NextIndex,
          newLogs: []
        )
      );

      //await server.SetNextIndexToAsync(new SetNextIndexToArgs(_clusterData.ServerData.Id, _clusterData.ServerData.Term, nextIndex));
      // If any respond with a rejection, I need to handle that soon
    }
  }

  public async Task SendRPCFromLeaderToEachFollowerAsync(string log)
  {
    // NOT RIGHT NEED TO FIX TODO
    RPCFromLeaderArgs rpcFromLeaderArgs = new(
          leaderId: _clusterData.ServerData.Id,
          term: _clusterData.ServerData.Term,
          previousLogIndex: _clusterData.LogHandler.PreviousLogIndex,
          previousLogTerm: _clusterData.LogHandler.PreviousLogTerm,
          leadersLastCommitIndex: _clusterData.ServerData.NextIndex,
          newLogs: []
      );

    foreach (IServerNode server in _clusterData.OtherServersInCluster)
    {
      await server.RPCFromLeaderAsync(rpcFromLeaderArgs);
    }
  }
}