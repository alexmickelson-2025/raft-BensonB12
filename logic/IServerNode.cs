namespace Logic;

public interface IServerNode
{
  int Id { get; }
  ServerNodeState State { get; }
  uint Term { get; }
  System.Timers.Timer ElectionTimer { get; }
  int? ClusterLeaderId { get; }
  void AddServersToCluster(IEnumerable<IServerNode> otherServers);
  Task RPCFromLeaderAsync(RPCFromLeaderArgs args);
  void Pause();
  void Unpause();
  Task CountVoteAsync(bool inSupport);
  Task TryToVoteForAsync(int id, uint term);
  Task RPCResponseAsyncFromFollowerAsync(int id, bool applied);
}
