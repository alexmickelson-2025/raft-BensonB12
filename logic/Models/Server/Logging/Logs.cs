using System.Collections;

namespace Logic.Models.Server.Logging;

public class Logs
{
  public List<LogData> Committed { get; } = [];
  public List<LogData> Local { get; } = [];
  public int NextIndex => Committed.Count;
  public uint? PreviousLogTerm => Committed.LastOrDefault()?.Term;
  public void Add(uint term, string log, int index)
  {
    Committed.Add(new LogData(term, log, index));
  }

  // public LogData this[int index]
  // {
  //   get => Committed[index];
  //   // set { }
  // }

  public bool SetNextIndexTo(int nextIndex)
  {
    if (Committed.Count != nextIndex)
    {
      return false;
      // throw new Exception();
    }

    return true;
  }

  public int LatestCommittedLogIndex()
  {
    return Committed.Count;
  }

  public int LastAppliedLogIndex()
  {
    return Committed.Count;
  }

  public bool CanExceptLog()
  {
    return true;
  }
}

