namespace Logic.Exceptions;

public class ClusterDidNotContainServerException : Exception
{
  public ClusterDidNotContainServerException(int? id) : base($"The cluster did not contain the server with the id - {id ?? '?'}") { }
}