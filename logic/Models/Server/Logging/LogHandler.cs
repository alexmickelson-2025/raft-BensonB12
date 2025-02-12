namespace Logic.Models.Server.Logging;

public class LogHandler
{
  Logs _logs = new();
  public int NextIndex => _logs.NextIndex;
  public Dictionary<int, int> FollowerToNextIndex { get; set; } = [];
  public int PreviousLogIndex => _logs.NextIndex - 1;
  public uint? PreviousLogTerm => _logs.PreviousLogTerm;

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