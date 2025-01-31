using Logic.Exceptions;
using Logic.Models.Server;
using Logic.Utils;

namespace Logic.Models.Client;

public class ClientNode : IClientNode
{
  IEnumerable<IServerNode> _serverNodes;
  IServerNode? _leaderNode;
  bool _committedLog = false;
  int _id;
  public int Id => _id;

  public ClientNode(IEnumerable<IServerNode> serverNodes, int? id = null)
  {
    _id = id ?? Util.GenerateId();
    _serverNodes = serverNodes;
  }
  public async Task SendLogToClusterAsync(string log)
  {
    _committedLog = false;

    if (_leaderNode is not null)
    {
      await _leaderNode.AppendLogRPCAsync(log, _id);
    }
    else
    {
      await _serverNodes.First().AppendLogRPCAsync(log, _id);
    }

    Thread.Sleep(Constants.HEARTBEAT_PAUSE);

    if (!_committedLog)
    {
      await SendLogToClusterAsync(log);
    }
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
    await Task.CompletedTask;
  }
}