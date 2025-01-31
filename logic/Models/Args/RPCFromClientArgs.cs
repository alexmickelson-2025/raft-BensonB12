namespace Logic.Models.Args;

public class RPCFromClientArgs
{
  public int ClientId { get; }
  public string? Log { get; }
  public bool? ServerShouldBePaused { get; }

  public RPCFromClientArgs(int clientId, string? log = null, bool? serverShouldBePaused = null)
  {
    if (log is null && serverShouldBePaused is null)
    {
      throw new InvalidRPCFromClientArgsException();
    }

    ClientId = clientId;
    Log = log;
    ServerShouldBePaused = serverShouldBePaused;
  }
}