namespace logic
{
  public interface IServerNode
  {
    int Id { get; }
    ServerNodeState State { get; }
    int Term { get; }
    int ElectionTimerInterval { get; }
    int? ClusterLeaderId { get; }

    void initializeServerNode();

    void AddServersToServersCluster(IEnumerable<IServerNode> otherServers);

    Task ReceiveHeartBeat(HeartbeatArguments arguments);

    void iunElectionForYourself();

    // void becomeLeader();

    void KillServer();

    // Task<bool> hasMajorityVotes();
  }
}
