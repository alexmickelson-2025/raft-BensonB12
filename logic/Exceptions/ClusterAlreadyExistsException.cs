namespace Logic.Exceptions;

public class ClusterAlreadyExistsException : Exception
{
  public ClusterAlreadyExistsException() : base("Cannot make a add servers to clusters a second time") { }
}