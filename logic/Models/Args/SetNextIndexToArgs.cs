namespace Logic.Models.Args;

public class SetNextIndexToArgs
{
  public int Id { get; }
  public uint Term { get; }
  public int NextIndex { get; }

  public SetNextIndexToArgs(int serverId, uint currentTerm, int nextIndex)
  {
    Id = serverId;
    Term = currentTerm;
    NextIndex = nextIndex;
  }
}