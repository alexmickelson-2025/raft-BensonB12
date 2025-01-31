using Logic.Models.Args;
using Logic.Models.Server;
using NSubstitute;

namespace Tests.AppendingLogs;

public class CommittedLogs
{
  /// <summary>
  /// Testing Logs #16
  /// </summary>
  [Fact]
  public async Task WhenALeaderSendsAHeartbeatWithALogButDoesNotReceiveResponsesFromAMajorityTheEntryIsUncommitted()
  {
    // Given
    IServerNode followerServer = Utils.CreateIServerNodeSubstituteWithId(1);
    ServerNode leaderServer = new();

    Utils.ServersVoteForLeaderWhenAsked([followerServer], leaderServer);
    leaderServer.InitializeClusterWithServers([followerServer]);

    // When
    Utils.WaitForElectionTimerToRunOut();
    await leaderServer.RPCFromClientAsync(new RPCFromClientArgs(0, "log"));
    Utils.WaitForHeartbeatTimerToRunOut();
    followerServer.ClearReceivedCalls();
    Utils.WaitForHeartbeatTimerToRunOut();

    // Then
    await followerServer.Received().RPCFromLeaderAsync(Arg.Is<RPCFromLeaderArgs>(args => args.PreviousLogIndex == null && args.PreviousLogTerm == null));
  }
}