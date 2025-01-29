using Logic.Exceptions;
using Logic.Models.Server;

namespace Logic.Models.Cluster;

public class ClusterData
{
  public int VotesInFavorForServer { get; set; } = 1;
  public int VotesNotInFavorForServer { get; set; } = 0;
  public List<IServerNode> OtherServersInCluster { get; } = [];
  public int? ClusterLeaderId { get; set; }
  public Dictionary<int, int> FollowerToNextIndex { get; set; } = [];
  public List<Thread> HeartbeatThreads { get; } = [];
  public ServerData ServerData { get; }

  public ClusterData(IEnumerable<IServerNode> otherServers, ServerData serverData)
  {
    // If anyone has the same ID, throw exception
    ServerData = serverData;
    createNextIndexAccountsForeachServer(otherServers);
    OtherServersInCluster.AddRange(otherServers);
  }

  void createNextIndexAccountsForeachServer(IEnumerable<IServerNode> servers)
  {
    foreach (IServerNode server in servers)
    {
      FollowerToNextIndex[server.Id] = 0;
    }
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