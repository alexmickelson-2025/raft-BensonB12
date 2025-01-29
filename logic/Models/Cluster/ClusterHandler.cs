using Logic.Models.Server;

namespace Logic.Models.Cluster;

public class ClusterHandler
{
  ClusterData _clusterData;
  OutGoingCallsClusterHandler _outGoingCallsClusterHandler;
  IncomingCallsClusterHandler _inComingCallsClusterHandler;
  public int? ClusterLeaderId { get => _clusterData.ClusterLeaderId; set => _clusterData.ClusterLeaderId = value; }

  public ClusterHandler(IEnumerable<IServerNode> otherServers, ServerData serverData)
  {
    _clusterData = new ClusterData(otherServers, serverData);
    _outGoingCallsClusterHandler = new OutGoingCallsClusterHandler(_clusterData);
    _inComingCallsClusterHandler = new IncomingCallsClusterHandler(_clusterData);
  }

  public bool ClusterIsEmpty => _clusterData.OtherServersInCluster.Count < 1;

  public async Task PetitionOtherServersToVoteAsync()
  {
    await _outGoingCallsClusterHandler.PetitionOtherServersToVoteAsync();
  }

  public void WaitForEnoughVotesFromOtherServers(bool theElectionIsStillGoing)
  {
    _outGoingCallsClusterHandler.WaitForEnoughVotesFromOtherServers(theElectionIsStillGoing);
  }

  public void StartSendingHeartbeatsToEachOtherServer()
  {
    _outGoingCallsClusterHandler.StartSendingHeartbeatsToEachOtherServer();
  }

  public async Task RegisterVoteForAsync(int id, uint term)
  {
    await _inComingCallsClusterHandler.RegisterVoteForAsync(id, term);
  }

  public async Task CountVoteAsync(bool inFavor)
  {
    await _inComingCallsClusterHandler.CountVoteAsync(inFavor);
  }

  public bool HasMajorityInFavorVotes()
  {
    return _outGoingCallsClusterHandler.HasMajorityInFavorVotes();
  }

  public void StopAllHeartBeatThreads()
  {
    _outGoingCallsClusterHandler.StopAllHeartBeatThreads();
  }

  public async Task SetFollowersNextIndexToAsync()
  {
    await _outGoingCallsClusterHandler.SetFollowersNextIndexToAsync(_clusterData.LogHandler.NextIndex);
  }

  public IServerNode GetServer(int serverId)
  {
    return _clusterData.GetServer(serverId);
  }

  // TODO: Make a RPC Handler
  public async Task RPCFromFollowerAsync(int id, bool rejected)
  {
    await _inComingCallsClusterHandler.RPCFromFollowerAsync(id, rejected);
  }

  public async Task SendRPCFromLeaderToEachFollowerAsync(string log)
  {
    await _outGoingCallsClusterHandler.SendRPCFromLeaderToEachFollowerAsync(log);
  }
}