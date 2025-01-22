namespace Logic;

public interface IServerNode
{
  int Id { get; }
  ServerNodeState State { get; }
  int Term { get; }
  System.Timers.Timer ElectionTimer { get; }
  int? ClusterLeaderId { get; }
  void AddServersToServersCluster(IEnumerable<IServerNode> otherServers);
  Task ReceiveHeartBeatAsync(HeartbeatArguments arguments);
  void KillServer();
  Task AcceptVoteAsync(bool inSupport);
  Task ThrowBalletForAsync(int id, int term);
  Task ReceiveAppendEntriesAsync(int id, int term);
  Task AppendEntryResponseAsync(int id, bool applied);
}
