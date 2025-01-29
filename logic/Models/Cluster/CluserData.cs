using Logic.Exceptions;
using Logic.Models.Server;
using Logic.Models.Server.Logging;

namespace Logic.Models.Cluster;

public class ClusterData
{
  public List<IServerNode> OtherServersInCluster { get; } = [];
  public int? ClusterLeaderId { get; set; }
  public List<Thread> HeartbeatThreads { get; } = [];
  public ServerData ServerData { get; }
  public LogHandler LogHandler { get; }

  public ClusterData(IEnumerable<IServerNode> otherServers, ServerData serverData)
  {
    // If anyone has the same ID, throw exception
    ServerData = serverData;
    LogHandler = new LogHandler(otherServers.Select(server => server.Id));
    OtherServersInCluster.AddRange(otherServers);
  }

  public IServerNode GetServer(int serverId)
  {
    IServerNode? server = OtherServersInCluster.SingleOrDefault(server => server.Id == serverId);
    // Throw an error if leaderNode is null

    if (server is null)
    {
      throw new ClusterDidNotContainServerException(serverId);
    }

    return server;
  }
}