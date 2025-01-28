namespace Logic;

public class RPCFromLeaderArgs
{
  public uint Term { get; }
  public int ServerId { get; }
  public string? Log { get; }

  public RPCFromLeaderArgs(int serverId, uint term)
  {
    ServerId = serverId;
    Term = term;
  }

  public RPCFromLeaderArgs(int serverId, uint term, string log)
  {
    ServerId = serverId;
    Term = term;
    Log = log;
  }
}