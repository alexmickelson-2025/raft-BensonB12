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
  IEnumerable<IClientNode> _knownClients;

  public ServerNode(IEnumerable<IServerNode>? otherServers = null, int? id = null, IEnumerable<IClientNode>? clients = null)
  {
    _knownClients = clients ?? [];
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

  public async Task RPCFromCandidateAsync(RPCFromCandidateArgs args)
  {
    await _clusterHandler.RegisterVoteForAsync(args.CandidateId, args.Term);
  }

  async Task countVoteAsync(bool inFavor) // Does not care who sent the vote, the servers are restricted to only vote once per term. Maybe I should take the term then?
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
      await leaderServer.RPCFromFollowerAsync(new RPCFromFollowerArgs(_serverData.Id, _serverData.Term, false));
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

    await leaderServer.RPCFromFollowerAsync(new RPCFromFollowerArgs(_serverData.Id, _serverData.Term, true));
  }

  public async Task RPCFromFollowerAsync(RPCFromFollowerArgs args)
  {
    if (_serverData.ServerIsACandidate())
    {
      await countVoteAsync(args.WasSuccess);
      return;
    }

    await _clusterHandler.RPCFromFollowerAsync(args.FollowerId, args.WasSuccess);
  }

  async Task appendLogRPCAsync(string log, int clientId)
  {
    if (_serverData.ServerIsDown())
    {
      return;
    }

    IClientNode? client = _knownClients.SingleOrDefault(client => client.Id == clientId);

    if (_serverData.ServerIsTheLeader())
    {
      _serverData.AddToLocalMemory(_serverData.Term, log);
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

  public async Task RPCFromClientAsync(RPCFromClientArgs args)
  {

    IClientNode? client = _knownClients.SingleOrDefault(client => client.Id == args.ClientId);

    if (client is null)
    {
      addClientToClients(args.ClientId);
    }

    if (args.ServerShouldBePaused == false)
    {
      await _electionHandler.Unpause();
    }

    if (args.Log is not null)
    {
      await appendLogRPCAsync(args.Log, args.ClientId);
    }

    if (args.ServerShouldBePaused == true)
    {
      await _electionHandler.Pause();
    }
  }

  void addClientToClients(int clientId)
  {
    Console.Write(clientId);
    // IDK how to do this know
  }
}
