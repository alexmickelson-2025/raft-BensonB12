using Logic;

namespace UserInterface;

public class SimulationNode : IServerNode
{
  readonly ServerNode _innerServerNode;
  public ServerNode InnerServerNode => _innerServerNode;
  public SimulationNode(ServerNode server)
  {
    _innerServerNode = server;
  }
  public int Id => _innerServerNode.Id;

  public ServerNodeState State => _innerServerNode.State;

  public int Term => _innerServerNode.Term;

  public System.Timers.Timer ElectionTimer => _innerServerNode.ElectionTimer;

  public int? ClusterLeaderId => _innerServerNode.ClusterLeaderId;

  public void AddServersToServersCluster(IEnumerable<IServerNode> otherServers)
  {
    _innerServerNode.AddServersToServersCluster(otherServers);
  }

  public Task ReceiveHeartBeatAsync(HeartbeatArguments arguments)
  {
    return _innerServerNode.ReceiveHeartBeatAsync(arguments);
  }

  public void KillServer()
  {
    _innerServerNode.KillServer();
  }

  public Task AcceptVoteAsync(bool inSupport)
  {
    return _innerServerNode.AcceptVoteAsync(inSupport);
  }

  public Task ThrowBalletForAsync(int id, int term)
  {
    return _innerServerNode.ThrowBalletForAsync(id, term);
  }

  public Task ReceiveAppendEntriesAsync(int id, int term)
  {
    return _innerServerNode.ReceiveAppendEntriesAsync(id, term);
  }

  public Task AppendEntryResponseAsync(int id, bool rejected)
  {
    return _innerServerNode.AppendEntryResponseAsync(id, rejected);
  }
}