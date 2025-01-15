namespace logic;

public class ServerNode
{
  ServerNodeState _state;
  public ServerNodeState State => _state;

  public ServerNode()
  {
    _state = ServerNodeState.FOLLOWER;
  }
}
