using System.Collections;

namespace Logic.Models.Server.Logging;

public class Logs : IEnumerable<LogData>
{
  List<LogData> _logs = [];
  public int NextIndex => _logs.Count;
  public uint PreviousLogTerm => _logs.Last().Term;
  public void Add(uint term, string log, int index)
  {
    _logs.Add(new LogData(term, log, index));
  }

  public LogData this[int index]
  {
    get => _logs[index];
    // set { }
  }

  public IEnumerator<LogData> GetEnumerator()
  {
    return _logs.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  public bool SetNextIndexTo(int nextIndex)
  {
    if (_logs.Count != nextIndex)
    {
      return false;
      // throw new Exception();
    }

    return true;
  }

  public int LatestCommittedLogIndex()
  {
    return _logs.Count;
  }

  public int LastAppliedLogIndex()
  {
    return _logs.Count;
  }

  public bool CanExceptLog()
  {
    return true;
  }
}

