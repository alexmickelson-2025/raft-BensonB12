namespace Logic.Exceptions;

public class UnpausedARunningServerException : Exception
{
  public UnpausedARunningServerException() : base("Cannot unpause a server when it was not paused") { }
}