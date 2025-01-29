namespace Logic.Models.Server.Election;

public class ElectionHandler
{
  ElectionData _electionData = new();
  ServerData _serverData;

  public ElectionHandler(ServerData serverData)
  {
    _serverData = serverData;
  }
}