namespace Logic;

public class HeartbeatArguments
{
  public int Term { get; }
  public int ServerNodeId { get; }
  public HeartbeatArguments(int term, int serverNodeId)
  {
    Term = term;
    ServerNodeId = serverNodeId;
  }
}