using System.Threading.Tasks;
using FluentAssertions;
using Logic.Models.Server;
using NSubstitute;

namespace Tests.Pause;

public class LeaderTests
{
  /// <summary>
  /// Testing Pausing #1
  /// </summary>
  [Fact]
  public void WhenServerIsLeaderAndPausedOtherNodesDoNotGetHeartbeats()
  {
    // Given
    IServerNode follower = Utils.CreateIServerNodeSubstituteWithId(1);
    ServerNode leaderServer = new();

    Utils.ServersVoteForLeaderWhenAsked([follower], leaderServer);
    leaderServer.InitializeClusterWithServers([follower]);

    // When
    while (leaderServer.State != ServerNodeState.LEADER)
    {
      // Wait
    }

    leaderServer.Pause();
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
    leaderServer.Pause();
    int callsSoFar = follower.ReceivedCalls().Count();
    await leaderServer.Unpause();
    Utils.WaitForHeartbeatTimerToRunOut();

    // Then
    follower.ReceivedCalls().Should().HaveCountGreaterThan(callsSoFar);
  }
}