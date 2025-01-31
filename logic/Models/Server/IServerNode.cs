using Logic.Models.Args;

namespace Logic.Models.Server;

public interface IServerNode
{
  int Id { get; }
  ServerNodeState State { get; }
  uint Term { get; }
  void InitializeClusterWithServers(IEnumerable<IServerNode> otherServers);
  Task RPCFromLeaderAsync(RPCFromLeaderArgs args); // Heartbeat, Log / catchup Log
  Task RPCFromClientAsync(RPCFromClientArgs args); // Pause, Unpause, Log
  Task RPCFromCandidateAsync(RPCFromCandidateArgs args);
  Task RPCFromFollowerAsync(RPCFromFollowerArgs args);
}

// RPCFromClient *DONE*
// // Pause
// // Unpause
// // Log

// RPCFromLeader
// // Heartbeat
// // Log * catchup logs


// RPCFromFollower
// Heartbeat response
// Log response
// Count my vote

// RPCFromCandidate
// request vote