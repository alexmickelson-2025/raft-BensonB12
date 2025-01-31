using Logic.Exceptions;
using Logic.Models.Server;
using Logic.Utils;

namespace Logic.Models.Client;

public class ClientNode : IClientNode
{
  List<IServerNode> _serverNodes = [];
  IServerNode? _leaderNode;
  bool _committedLog = false;
  int _id;
  public int Id => _id;

  public ClientNode(IEnumerable<IServerNode> serverNodes, int? id = null)
  {
    _id = id ?? Util.GenerateId();
    _serverNodes.AddRange(serverNodes);
  }

  public async Task SendLogToClusterAsync(string log)
  {
    if (_serverNodes.Count < 0)
    {
      throw new ClusterIsEmptyException();
    }

    _committedLog = false;

    if (_leaderNode is not null)
    {
      await _leaderNode.RPCFromClientAsync(new Args.RPCFromClientArgs(_id, log));
    }

    int randomServerIndex = Random.Shared.Next(_serverNodes.Count);
    await _serverNodes[randomServerIndex].RPCFromClientAsync(new Args.RPCFromClientArgs(_id, log));

    Thread.Sleep(Constants.CLUSTER_WAITS_FOR_RESPONSE_INTERVAL);

    // if (!_committedLog)
    // {
    //   await SendLogToClusterAsync(log);
    // }
  }

  public async Task ResponseFromServerAsync(bool committed, int? leaderId = null)
  {
    if (committed)
    {
      _committedLog = true;
      return;
    }

    if (leaderId is not null)
    {
      throw new InvalidResponseFromServerException();
    }

    IServerNode newLeader = _serverNodes.SingleOrDefault(node => node.Id == leaderId) ?? throw new ClusterDidNotContainServerException(leaderId);

    if (newLeader is not null)
    {
      throw new ClusterDidNotContainServerException(leaderId);
    }

    _leaderNode = newLeader;
    await Task.CompletedTask;
  }
}