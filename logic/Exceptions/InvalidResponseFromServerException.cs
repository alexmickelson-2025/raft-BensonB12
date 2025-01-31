namespace Logic.Exceptions;

class InvalidResponseFromServerException : Exception
{
  public InvalidResponseFromServerException() : base("Client didn't receive a confirm commit or a new leader Id") { }
}