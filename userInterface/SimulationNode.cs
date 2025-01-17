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

  public ServerNodeState State => ((IServerNode)_innerServerNode).State;

  public int Term => ((IServerNode)_innerServerNode).Term;

  public int ElectionTimerInterval => ((IServerNode)_innerServerNode).ElectionTimerInterval;

  public int? ClusterLeaderId => ((IServerNode)_innerServerNode).ClusterLeaderId;

  public void initializeServerNode()
  {
    ((IServerNode)_innerServerNode).initializeServerNode();
  }

  public void AddServersToServersCluster(IEnumerable<IServerNode> otherServers)
  {
    ((IServerNode)_innerServerNode).AddServersToServersCluster(otherServers);
  }

  public Task ReceiveHeartBeat(HeartbeatArguments arguments)
  {
    return ((IServerNode)_innerServerNode).ReceiveHeartBeat(arguments);
  }

  public void iunElectionForYourself()
  {
    ((IServerNode)_innerServerNode).iunElectionForYourself();
  }

  public void KillServer()
  {
    ((IServerNode)_innerServerNode).KillServer();
  }
}