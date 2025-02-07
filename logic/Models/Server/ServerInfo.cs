using Logic.Models.Server.Logging;

public class ServerInfo
{
  public int Id { get; set; }
  public uint Term { get; set; }
  public int ServerStateId { get; set; }
  public LogInfo LogInfo { get; set; } = new();
}

// app.MapGet("/nodeData", () =>
// {
//   return new NodeData(
//     Id: node.Id,
//     Status: node.Status,
//     ElectionTimeout: node.ElectionTimeout,
//     Term: node.CurrentTerm,
//     CurrentTermLeader: node.CurrentTermLeader,
//     CommittedEntryIndex: node.CommittedEntryIndex,
//     Log: node.Log,
//     State: node.State,
//     NodeIntervalScalar: RaftNode.NodeIntervalScalar
//   );
// });