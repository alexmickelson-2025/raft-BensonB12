using System.Threading.Tasks;
using FluentAssertions;
using Logic.Models.Args;
using Logic.Models.Server;
using NSubstitute;

namespace Tests.Pause;

public class LeaderTests
{
  /// <summary>
  /// Testing Pausing #1
  /// </summary>
  [Fact]
  public async Task WhenServerIsLeaderAndPausedOtherNodesDoNotGetHeartbeats()
  {
    // Given
    IServerNode follower = Utils.CreateIServerNodeSubstituteWithId(1);
    ServerNode leaderServer = new();

    Utils.ServersVoteForLeaderWhenAsked([follower], leaderServer);
    leaderServer.InitializeClusterWithServers([follower]);

    // When
    while (leaderServer.State != ServerNodeState.LEADER)
    {
      Thread.Sleep(50);
      // Wait
    }

    await leaderServer.RPCFromClientAsync(new RPCFromClientArgs(0, serverShouldBePaused: true));
    Thread.Sleep(Utils.GENERAL_BUFFER_TIME); // I have to wait for the thread to come back. I need to fix that
    int callsSoFar = follower.ReceivedCalls().Count();
    Utils.WaitForHeartbeatTimerToRunOut();

    // Then
    follower.ReceivedCalls().Should().HaveCount(callsSoFar);
  }

  /// <summary>
  /// Testing Pausing #2
  /// </summary>
  [Fact]
  public async Task WhenServerIsLeaderPausedThenUnpausedItStartsSendingHeartbeatsAgain()
  {
    // Given
    IServerNode follower = Utils.CreateIServerNodeSubstituteWithId(1);
    ServerNode leaderServer = new();

    Utils.ServersVoteForLeaderWhenAsked([follower], leaderServer);
    leaderServer.InitializeClusterWithServers([follower]);

    // When
    Utils.WaitForElectionTimerToRunOut();
    await leaderServer.RPCFromClientAsync(new RPCFromClientArgs(0, serverShouldBePaused: true));
    int callsSoFar = follower.ReceivedCalls().Count();
    await leaderServer.RPCFromClientAsync(new RPCFromClientArgs(0, serverShouldBePaused: false));
    Utils.WaitForHeartbeatTimerToRunOut();

    // Then
    follower.ReceivedCalls().Should().HaveCountGreaterThan(callsSoFar);
  }
}