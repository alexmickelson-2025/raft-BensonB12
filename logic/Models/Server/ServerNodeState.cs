namespace Logic.Models.Server;

public enum ServerNodeState
{
  FOLLOWER,
  CANDIDATE,
  LEADER,
  DOWN,
}