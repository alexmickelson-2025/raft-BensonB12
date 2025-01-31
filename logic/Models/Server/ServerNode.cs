using Logic.Exceptions;
using Logic.Models.Args;
using Logic.Models.Client;
using Logic.Models.Cluster;
using Logic.Models.Server.Election;

namespace Logic.Models.Server;

public class ServerNode : IServerNode
{
  ServerData _serverData = new();
  ClusterHandler _clusterHandler = null!;
  public int Id => _serverData.Id;
  public ServerNodeState State => _serverData.State;
  public uint Term => _serverData.Term;
  ElectionHandler _electionHandler = null!;
  IEnumerable<IClientNode> _clients;

  public ServerNode(IEnumerable<IServerNode>? otherServers = null, int? id = null, IEnumerable<IClientNode>? clients = null)
  {
    _clients = clients ?? [];
    _serverData = new ServerData(id);
    InitializeClusterWithServers(otherServers ?? []);
  }

  public void InitializeClusterWithServers(IEnumerable<IServerNode> otherServers)
  {
    if (_clusterHandler is not null && !_clusterHandler.ClusterIsEmpty)
    {
      throw new ClusterAlreadyExistsException();
    }

    _clusterHandler = new ClusterHandler(otherServers, _serverData);
    _electionHandler = new ElectionHandler(_serverData, _clusterHandler);
  }

  public async Task RegisterVoteForAsync(int id, uint term)
  {
    await _clusterHandler.RegisterVoteForAsync(id, term);
  }

  public async Task CountVoteAsync(bool inFavor) // Does not care who sent the vote, the servers are restricted to only vote once per term. Maybe I should take the term then?
  {
    await _clusterHandler.CountVoteAsync(inFavor);
  }

  // TODO: Refactor this method down
  public async Task RPCFromLeaderAsync(RPCFromLeaderArgs args)
  {
    if (_serverData.ServerIsDown())
    {
      return;
    }

    IServerNode leaderServer = _clusterHandler.GetServer(args.LeaderId);

    if (args.Term < _serverData.Term)
    {
      await leaderServer.RPCFromFollowerAsync(_serverData.Id, false);
      return;
    }

    // If the term is the same, we have some corner cases to solve

    _clusterHandler.ClusterLeaderId = args.LeaderId;
    _electionHandler.RestartElectionTimeout();

    ServerNodeState oldState = _serverData.State;
    _serverData.SetState(ServerNodeState.FOLLOWER);
    // Maybe I should validate that the serverNodeId is in my cluster

    if (oldState == ServerNodeState.LEADER)
    {
      // Handle if our terms match
      _clusterHandler.StopAllHeartBeatThreads();
      _electionHandler.RestartElectionTimerWithNewInterval();
    }

    _serverData.Term = args.Term;

    await leaderServer.RPCFromFollowerAsync(_serverData.Id, true);
  }

  public async Task RPCFromFollowerAsync(int id, bool rejected)
  {
    await _clusterHandler.RPCFromFollowerAsync(id, rejected);
  }

  public async Task Pause()
  {
    await _electionHandler.Pause();
  }

  public async Task Unpause()
  {
    await _electionHandler.Unpause();
  }

  public async Task AppendLogRPCAsync(string log, int clientId)
  {
    if (_serverData.ServerIsDown())
    {
      return;
    }

    IClientNode? client = _clients.SingleOrDefault(client => client.Id == clientId);

    if (_serverData.ServerIsTheLeader())
    {
      appendLog(_serverData.Term, log);
      await _clusterHandler.SendRPCFromLeaderToEachFollowerAsync(log);

      if (client is null)
      {
        return;
      }

      await client.ResponseFromServerAsync(true);
    }

    if (client is null)
    {
      return;
    }

    await client.ResponseFromServerAsync(false, _clusterHandler.ClusterLeaderId);
  }

  void appendLog(uint term, string log)
  {
    _serverData.AddToLocalMemory(term, log);
  }
}
