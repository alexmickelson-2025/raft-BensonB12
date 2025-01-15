using logic;

namespace userInterface;

public class SimulationNode
{
  readonly ServerNode _innerServerNode;
  public ServerNode InnerServerNode => _innerServerNode;
  public SimulationNode(ServerNode server)
  {
    _innerServerNode = server;
  }
  public int Id => _innerServerNode.Id;
}