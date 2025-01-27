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

  public uint Term => _innerServerNode.Term;

  public System.Timers.Timer ElectionTimer => _innerServerNode.ElectionTimer;

  public int? ClusterLeaderId => _innerServerNode.ClusterLeaderId;

  public void AddServersToServersCluster(IEnumerable<IServerNode> otherServers)
  {
    _innerServerNode.AddServersToServersCluster(otherServers);
  }

  public Task ReceiveLeaderToFollowerRemoteProcedureCallAsync(LeaderToFollowerRemoteProcedureCallArguments arguments)
  {
    return _innerServerNode.ReceiveLeaderToFollowerRemoteProcedureCallAsync(arguments);
  }

  public void PauseServer()
  {
    _innerServerNode.PauseServer();
  }

  public Task AcceptVoteAsync(bool inSupport)
  {
    return _innerServerNode.AcceptVoteAsync(inSupport);
  }

  public Task ThrowBalletForAsync(int id, uint term)
  {
    return _innerServerNode.ThrowBalletForAsync(id, term);
  }

  public Task LeaderToFollowerRemoteProcedureCallResponse(int id, bool rejected)
  {
    return _innerServerNode.LeaderToFollowerRemoteProcedureCallResponse(id, rejected);
  }
}