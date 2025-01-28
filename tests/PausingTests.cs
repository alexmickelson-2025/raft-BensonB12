using System.Threading.Tasks;
using FluentAssertions;
using Logic;
using NSubstitute;

namespace Tests;

public class PausingTests
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
    leaderServer.AddServersToServersCluster([follower]);

    // When
    while (leaderServer.State != ServerNodeState.LEADER)
    {
      // Wait
    }

    leaderServer.PauseServer();
    int callsSoFar = follower.ReceivedCalls().Count();
    Utils.WaitForHeartbeatTimerToRunOut();

    // Then
    follower.ReceivedCalls().Should().HaveCount(callsSoFar);
  }

  /// <summary>
  /// Testing Pausing #2
  /// </summary>
  [Fact]
  public void WhenServerIsLeaderPausedThenUnpausedItStartsSendingHeartbeatsAgain()
  {
    // Given
    IServerNode follower = Utils.CreateIServerNodeSubstituteWithId(1);
    ServerNode leaderServer = new();

    Utils.ServersVoteForLeaderWhenAsked([follower], leaderServer);
    leaderServer.AddServersToServersCluster([follower]);

    // When
    while (leaderServer.State != ServerNodeState.LEADER)
    {
      // Wait
    }

    leaderServer.PauseServer();
    int callsSoFar = follower.ReceivedCalls().Count();
    leaderServer.UnpauseServer();
    Utils.WaitForHeartbeatTimerToRunOut();

    // Then
    follower.ReceivedCalls().Should().HaveCountGreaterThan(callsSoFar);
  }

  /// <summary>
  /// Testing Pausing #3
  /// </summary>
  [Fact]
  public void WhenServerGetsPausedTheyDoNotBecomeACandidate()
  {
    // Given
    ServerNode server = new();

    // When
    server.PauseServer();
    Utils.WaitForElectionTimerToRunOut();

    // Then
    server.State.Should().NotBe(ServerNodeState.CANDIDATE);
  }

  /// <summary>
  /// Testing Pausing #3
  /// </summary>
  [Fact]
  public async Task WhenServerGetsPausedTheyDoNotSendOutVoteRequests()
  {
    // Given
    IServerNode otherServer = Utils.CreateIServerNodeSubstituteWithId(1);
    ServerNode server = new([otherServer]);

    // When
    server.PauseServer();
    Utils.WaitForElectionTimerToRunOut();

    // Then
    await otherServer.DidNotReceive().ThrowBalletForAsync(Arg.Any<int>(), Arg.Any<uint>());
  }

  [Fact]
  public void WhenAFollowerGetsPausedAndUnpausedItStillBecomesACandidate()
  {
    // Given
    ServerNode server = new();

    // When
    server.PauseServer();
    server.UnpauseServer();
    Utils.WaitForElectionTimerToRunOut();

    // Then
    server.State.Should().Be(ServerNodeState.LEADER);
  }
}