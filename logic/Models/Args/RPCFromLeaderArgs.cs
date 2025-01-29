namespace Logic.Models.Args;

public class RPCFromLeaderArgs
{
  public uint Term { get; }
  public int ServerId { get; }
  public string? Log { get; }
  public int? LogIndex { get; }

  public RPCFromLeaderArgs(int serverId, uint term)
  {
    ServerId = serverId;
    Term = term;
  }

  public RPCFromLeaderArgs(int serverId, uint term, string log, int logIndex)
  {
    ServerId = serverId;
    Term = term;
    Log = log;
    LogIndex = logIndex;
  }
}