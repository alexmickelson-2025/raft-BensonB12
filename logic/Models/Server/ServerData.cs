using Logic.Exceptions;
using Logic.Models.Server.Logging;
using Logic.Utils;

namespace Logic.Models.Server;

public class ServerData
{
  Logs _logs = [];
  public int VotesInFavorForServer { get; set; } = 1;
  public int VotesNotInFavorForServer { get; set; } = 0;
  public int Id { get; }
  public ServerNodeState State { get; private set; } = ServerNodeState.FOLLOWER;
  public ServerNodeState? StateBeforePause { get; set; }
  public uint Term { get; set; } = 0;
  public int NextIndex => _logs.NextIndex;
  public Dictionary<uint, bool> HasVotedInTerm { get; set; } = new() { { 0, false } };

  public ServerData(int? id = null)
  {
    Id = id ?? Util.GenerateId();
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
    _logs.Add(term, log, NextIndex);
  }

  public void SetNextIndexTo(int nextIndex)
  {
    _logs.SetNextIndexTo(nextIndex);
  }
}