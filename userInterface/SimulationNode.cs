using logic;

namespace userInterface;

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

  public int Term => _innerServerNode.Term;

  public int ElectionTimerInterval => _innerServerNode.ElectionTimerInterval;

  public int? ClusterLeaderId => _innerServerNode.ClusterLeaderId;

  public void AddServersToServersCluster(IEnumerable<IServerNode> otherServers)
  {
    _innerServerNode.AddServersToServersCluster(otherServers);
  }

  public Task ReceiveHeartBeatAsync(HeartbeatArguments arguments)
  {
    return _innerServerNode.ReceiveHeartBeatAsync(arguments);
  }

  public void KillServer()
  {
    _innerServerNode.KillServer();
  }

  public Task AcceptVoteAsync(bool inSupport)
  {
    return _innerServerNode.AcceptVoteAsync(inSupport);
  }
}