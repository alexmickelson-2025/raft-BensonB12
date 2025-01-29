namespace Logic.Models.Server.Logging;

public class LogHandler
{
  Logs _logs = [];
  public int NextIndex => _logs.NextIndex;
  public IEnumerable<string> Messages => _logs.Select(log => log.Log);
  public Dictionary<int, int> FollowerToNextIndex { get; set; } = [];

  public LogHandler(IEnumerable<int> otherServerIds)
  {
    createNextIndexAccountsForeachServer(otherServerIds);
  }

  void createNextIndexAccountsForeachServer(IEnumerable<int> serverIds)
  {
    foreach (int serverId in serverIds)
    {
      FollowerToNextIndex[serverId] = 0;
    }
  }

  public void SetNextIndexTo(int nextIndex)
  {
    _logs.SetNextIndexTo(nextIndex);
  }
}