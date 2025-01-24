namespace Logic;

public class LeaderToFollowerRemoteProcedureCallArguments
{
  public uint Term { get; }
  public int ServerNodeId { get; }
  public LeaderToFollowerRemoteProcedureCallArguments(int serverNodeId, uint term)
  {
    ServerNodeId = serverNodeId;
    Term = term;
  }
}