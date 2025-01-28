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

    leaderServer.Pause();
    int callsSoFar = follower.ReceivedCalls().Count();
    leaderServer.Unpause();
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
    server.Pause();
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
    server.Pause();
    Utils.WaitForElectionTimerToRunOut();

    // Then
    await otherServer.DidNotReceive().ThrowBalletForAsync(Arg.Any<int>(), Arg.Any<uint>());
  }

  /// <summary>
  /// Testing Pausing #4
  /// </summary>
  [Fact]
  public void WhenAFollowerGetsPausedAndUnpausedItStillBecomesACandidate()
  {
    // Given
    ServerNode server = new();

    // When
    server.Pause();
    server.Unpause();
    Utils.WaitForElectionTimerToRunOut();

    // Then
    server.State.Should().Be(ServerNodeState.LEADER);
  }

  /// <summary>
  /// Testing Pausing #5
  /// </summary>
  [Fact]
  public async Task WhenServerIsPausedItDoesNotVote()
  {
    // Given
    int otherServerId = 1;

    ServerNode server = new();
    IServerNode otherServer = Utils.CreateIServerNodeSubstituteWithId(otherServerId);

    // When
    server.Pause();
    await server.ThrowBalletForAsync(otherServerId, server.Term + 1);

    // Then
    await otherServer.DidNotReceiveWithAnyArgs().AcceptVoteAsync(Arg.Any<bool>());
  }
}