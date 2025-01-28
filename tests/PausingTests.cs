using System.Threading.Tasks;
using FluentAssertions;
using Logic;
using Logic.Exceptions;
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
    leaderServer.AddServersToCluster([follower]);

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
    leaderServer.AddServersToCluster([follower]);

    // When
    Utils.WaitForElectionTimerToRunOut();
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
    await otherServer.DidNotReceive().TryToVoteForAsync(Arg.Any<int>(), Arg.Any<uint>());
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

    IServerNode otherServer = Utils.CreateIServerNodeSubstituteWithId(otherServerId);
    ServerNode server = new([otherServer]);

    // When
    server.Pause();
    await server.TryToVoteForAsync(otherServerId, server.Term + 1);

    // Then
    await otherServer.DidNotReceiveWithAnyArgs().CountVoteAsync(Arg.Any<bool>());
  }

  /// <summary>
  /// Testing Pausing #5 #TODO
  /// </summary>
  [Fact]
  public void WhenServerIsPausedItDoesNotLog()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Pausing #5
  /// </summary>
  [Fact]
  public async Task WhenServerIsPausedItDoesNotRespondToLog()
  {
    // Given
    int leaderId = 1;

    IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);
    ServerNode server = new([leaderServer]);

    // When
    server.Pause();
    await server.RPCFromLeaderAsync(new RPCFromLeaderArgs(leaderId, server.Term + 1));

    // Then
    await leaderServer.DidNotReceiveWithAnyArgs().RPCResponseAsyncFromFollowerAsync(Arg.Any<int>(), Arg.Any<bool>());
  }

  /// <summary>
  /// Testing Pausing #5
  /// </summary>
  [Fact]
  public async Task WhenServerIsPausedItDoesNotAcceptVote()
  {
    // Given
    int candidateId = 1;
    IServerNode candidateServer = Utils.CreateIServerNodeSubstituteWithId(candidateId);
    ServerNode server = new([candidateServer]);

    // When
    server.Pause();
    await server.TryToVoteForAsync(candidateId, server.Term + 1);
    Thread.Sleep(Utils.GENERAL_BUFFER_TIME);

    // Then
    await candidateServer.DidNotReceiveWithAnyArgs().CountVoteAsync(Arg.Any<bool>());
  }

  /// <summary>
  /// Testing Pausing #6
  /// </summary>
  [Fact]
  public void WhenServerIsUnpausedWithoutBeingPausedItThrowsError()
  {
    // Given
    ServerNode server = new();

    // When & Then
    FluentActions.Invoking(() => server.Unpause())
        .Should()
        .Throw<UnpausedARunningServerException>();
  }
}