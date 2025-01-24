namespace Logic;

public class LeaderToFollowerRemoteProcedureCallArguments
{
  public int Term { get; }
  public int ServerNodeId { get; }
  public LeaderToFollowerRemoteProcedureCallArguments(int term, int serverNodeId)
  {
    Term = term;
    ServerNodeId = serverNodeId;
  }
}