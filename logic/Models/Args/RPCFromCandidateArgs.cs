namespace Logic.Models.Args;

public class RPCFromCandidateArgs
{
  public int CandidateId { get; }
  public uint Term { get; }
  public int? CandidateLatestCommittedLogIndex { get; }
  public uint? CandidateLatestCommittedLogTerm { get; }

  public RPCFromCandidateArgs(int candidateId, uint term, int? candidateLatestCommittedLogIndex = null, uint? candidateLatestCommittedLogTerm = null)
  {
    CandidateId = candidateId;
    Term = term;
    CandidateLatestCommittedLogIndex = candidateLatestCommittedLogIndex;
    CandidateLatestCommittedLogTerm = candidateLatestCommittedLogTerm;
  }
}