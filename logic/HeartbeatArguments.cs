namespace logic;

public class HeartbeatArguments
{
  public int Term;
  public int ServerNodeId;
  public HeartbeatArguments(int term, int serverNodeId)
  {
    Term = term;
    ServerNodeId = serverNodeId;
  }
}