namespace logic;

public interface IServerNode
{
  int Id { get; }
  ServerNodeState State { get; }
  int Term { get; }
  int ElectionTimerInterval { get; }
  int? ClusterLeaderId { get; }
  void AddServersToServersCluster(IEnumerable<IServerNode> otherServers);
  Task ReceiveHeartBeatAsync(HeartbeatArguments arguments);
  void KillServer();
  Task AcceptVoteAsync(bool inSupport);
  Task ThrowBalletFor(int id);
}
