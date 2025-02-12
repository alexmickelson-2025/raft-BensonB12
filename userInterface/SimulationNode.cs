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

  public Task RPCFromCandidateAsync(RPCFromCandidateArgs args)
  {
    return _innerServerNode.RPCFromCandidateAsync(args);
  }

  public Task RPCFromFollowerAsync(RPCFromFollowerArgs args)
  {
    return _innerServerNode.RPCFromFollowerAsync(args);
  }

  public Task RPCFromClientAsync(RPCFromClientArgs args)
  {
    return _innerServerNode.RPCFromClientAsync(args);
  }
}