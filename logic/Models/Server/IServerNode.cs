using Logic.Models.Args;

namespace Logic.Models.Server;

public interface IServerNode
{
  int Id { get; }
  ServerNodeState State { get; }
  uint Term { get; }
  void InitializeClusterWithServers(IEnumerable<IServerNode> otherServers);
  Task RPCFromLeaderAsync(RPCFromLeaderArgs args);
  Task Pause();
  Task Unpause();
  Task CountVoteAsync(bool inSupport);
  Task RegisterVoteForAsync(int id, uint term);
  Task RPCFromFollowerAsync(int id, bool applied);
  Task SetNextIndexToAsync(SetNextIndexToArgs setNextIndexToArgs);
  Task AppendLogRPCAsync(string log);
}
