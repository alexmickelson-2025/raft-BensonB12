using FluentAssertions;
using Logic;
using NSubstitute;

namespace Tests;

public class PausingTests
{
  [Fact]
  public void WhenNodeIsLeaderAndPausedOtherNodesDoNotGetHeartbeats()
  {
    // Given
    IServerNode followerOne = Utils.CreateIServerNodeSubstituteWithId(1);
    ServerNode leaderServer = new();

    Utils.ServersVoteForLeaderWhenAsked([followerOne], leaderServer);
    leaderServer.AddServersToServersCluster([followerOne]);

    // When
    while (leaderServer.State != ServerNodeState.LEADER)
    {
      // Wait
    }

    leaderServer.PauseServer();
    int callsSoFar = followerOne.ReceivedCalls().Count();
    Utils.WaitForHeartbeatTimerToRunOut();

    // Then
    followerOne.ReceivedCalls().Should().HaveCount(callsSoFar);
  }
}