namespace Logic.Models;

public class LogData
{
  public uint Term { get; }
  public string Log { get; }

  public LogData(uint term, string log)
  {
    Term = term;
    Log = log;
  }
}