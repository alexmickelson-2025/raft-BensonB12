namespace Logic.Models.Args;

public class RPCFromFollowerArgs
{
  public int FollowerId { get; }
  public uint Term { get; }
  public bool WasSuccess { get; }

  public RPCFromFollowerArgs(int followerId, uint term, bool wasSuccess)
  {
    FollowerId = followerId;
    Term = term;
    WasSuccess = wasSuccess;
  }
}