namespace Logic.Models.Server.Logging;

public class LogInfo
{
  public List<LogData> LocalLogs { get; set; } = [];
  public List<LogData> CommittedLogs { get; set; } = [];
}