namespace Logic.Exceptions;

public class FollowerToLeaderException : Exception
{
  public FollowerToLeaderException() : base("Cannot go from a follower to a leader directly") { }
}