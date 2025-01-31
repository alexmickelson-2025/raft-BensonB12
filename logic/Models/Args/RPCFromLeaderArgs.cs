using Logic.Models.Server.Logging;

namespace Logic.Models.Args;

public class RPCFromLeaderArgs
{
  public uint Term { get; }
  public int LeaderId { get; }
  public int? PreviousLogIndex { get; }
  public uint? PreviousLogTerm { get; }
  public int? LeadersLastCommitIndex { get; }
  public List<LogData> NewLogs { get; }

  public RPCFromLeaderArgs(int leaderId, uint term, int? previousLogIndex, uint? previousLogTerm, int? leadersLastCommitIndex, List<LogData>? newLogs = null)
  {
    LeaderId = leaderId;
    Term = term;
    PreviousLogIndex = previousLogIndex;
    PreviousLogTerm = previousLogTerm;
    LeadersLastCommitIndex = leadersLastCommitIndex;
    NewLogs = newLogs ?? [];
  }
}