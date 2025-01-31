using Logic.Models.Args;
using Logic.Models.Server;

namespace UserInterface;

public class SimulationNode : IServerNode
{
  readonly ServerNode _innerServerNode;
  public ServerNode InnerServerNode => _innerServerNode;
  public SimulationNode(ServerNode server)
  {
    _innerServerNode = server;
  }
  public int Id => _innerServerNode.Id;

  public ServerNodeState State => _innerServerNode.State;

  public uint Term => _innerServerNode.Term;

  public void InitializeClusterWithServers(IEnumerable<IServerNode> otherServers)
  {
    _innerServerNode.InitializeClusterWithServers(otherServers);
  }

  public Task RPCFromLeaderAsync(RPCFromLeaderArgs args)
  {
    return _innerServerNode.RPCFromLeaderAsync(args);
  }

  public async Task Pause()
  {
    await _innerServerNode.Pause();
  }

  public Task CountVoteAsync(bool inSupport)
  {
    return _innerServerNode.CountVoteAsync(inSupport);
  }

  public Task RegisterVoteForAsync(int id, uint term)
  {
    return _innerServerNode.RegisterVoteForAsync(id, term);
  }

  public Task RPCFromFollowerAsync(int id, bool rejected)
  {
    return _innerServerNode.RPCFromFollowerAsync(id, rejected);
  }

  public async Task Unpause()
  {
    await _innerServerNode.Unpause();
  }

  public Task AppendLogRPCAsync(string log, int client_id)
  {
    return _innerServerNode.AppendLogRPCAsync(log, client_id);
  }
}