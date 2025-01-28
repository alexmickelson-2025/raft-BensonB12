using Logic;

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

  public System.Timers.Timer ElectionTimer => _innerServerNode.ElectionTimer;

  public int? ClusterLeaderId => _innerServerNode.ClusterLeaderId;

  public void AddServersToCluster(IEnumerable<IServerNode> otherServers)
  {
    _innerServerNode.AddServersToCluster(otherServers);
  }

  public Task RPCFromLeaderAsync(RPCFromLeaderArgs args)
  {
    return _innerServerNode.RPCFromLeaderAsync(args);
  }

  public void Pause()
  {
    _innerServerNode.Pause();
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

  public void Unpause()
  {
    _innerServerNode.Unpause();
  }

  public Task SetNextIndexToAsync(SetNextIndexToArgs args)
  {
    return _innerServerNode.SetNextIndexToAsync(args);
  }

  public Task AppendLogRPCAsync(string log)
  {
    return _innerServerNode.AppendLogRPCAsync(log);
  }
}