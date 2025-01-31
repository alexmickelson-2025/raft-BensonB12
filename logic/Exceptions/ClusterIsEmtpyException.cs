namespace Logic.Exceptions;

class ClusterIsEmptyException : Exception
{
  public ClusterIsEmptyException() : base("The cluster is empty") { }
}