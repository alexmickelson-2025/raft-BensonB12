namespace Logic;

public class RPCFromLeaderArgs
{
  public uint Term { get; }
  public int ServerId { get; }
  public RPCFromLeaderArgs(int serverId, uint term)
  {
    ServerId = serverId;
    Term = term;
  }
}