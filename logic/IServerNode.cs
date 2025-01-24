namespace Logic;

public interface IServerNode
{
  int Id { get; }
  ServerNodeState State { get; }
  int Term { get; }
  System.Timers.Timer ElectionTimer { get; }
  int? ClusterLeaderId { get; }
  void AddServersToServersCluster(IEnumerable<IServerNode> otherServers);
  Task ReceiveLeaderToFollowerRemoteProcedureCallAsync(LeaderToFollowerRemoteProcedureCallArguments arguments);
  void KillServer();
  Task AcceptVoteAsync(bool inSupport);
  Task ThrowBalletForAsync(int id, int term);
  Task LeaderToFollowerRemoteProcedureCallResponse(int id, bool applied);
}
