
namespace Logic.Models.Server.Logging;

public class LogData
{
  public uint Term { get; }
  public string Log { get; }
  public int Index { get; }

  public LogData(uint term, string log, int index)
  {
    Term = term;
    Log = log;
    Index = index;
  }
}