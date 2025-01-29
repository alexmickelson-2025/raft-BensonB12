using Logic.Exceptions;
using Logic.Models.Server.Logging;
using Logic.Utils;

namespace Logic.Models.Server;

public class ServerData
{
  Logs _logs = [];
  public int Id { get; } = Util.GenerateUniqueServerNodeId();
  public ServerNodeState State { get; private set; } = ServerNodeState.FOLLOWER;
  public ServerNodeState? StateBeforePause { get; set; }
  public uint Term { get; set; } = 0;
  public Dictionary<uint, bool> HasVotedInTerm { get; set; } = new() { { 0, false } };

  public ServerData() { }

  public ServerData(int id)
  {
    Id = id;
  }

  public void SetState(ServerNodeState? newState)
  {
    if (State == ServerNodeState.FOLLOWER && newState == ServerNodeState.LEADER)
    {
      throw new FollowerToLeaderException();
    }

    State = newState ?? throw new UnpausedARunningServerException();
  }

  public bool ServerIsTheLeader()
  {
    return State == ServerNodeState.LEADER;
  }

  public bool ServerIsDown()
  {
    return State == ServerNodeState.DOWN;
  }

  public bool ServerIsACandidate()
  {
    return State == ServerNodeState.CANDIDATE;
  }

  public bool CannotVoteInTerm(uint term)
  {
    return HasVotedInTerm.TryGetValue(term, out var value) && value;
  }

  public void AddToLocalMemory(uint term, string log)
  {
    _logs.Add(term, log);
  }

  public void SetNextIndexTo(int nextIndex)
  {
    _logs.SetNextIndexTo(nextIndex);
  }
}