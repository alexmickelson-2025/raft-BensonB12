class InvalidRPCFromClientArgsException : Exception
{
  public InvalidRPCFromClientArgsException() : base("Cannot have a command from client that is all null") { }
}