using FluentAssertions;
using Logic;
using NSubstitute;

namespace Tests.Pause;

public class CandidateTests
{
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
    await otherServer.DidNotReceive().RegisterVoteForAsync(Arg.Any<int>(), Arg.Any<uint>());
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
  public async Task WhenServerIsPausedItDoesNotAcceptVote()
  {
    // Given
    int candidateId = 1;
    IServerNode candidateServer = Utils.CreateIServerNodeSubstituteWithId(candidateId);
    ServerNode server = new([candidateServer]);

    // When
    server.Pause();
    await server.RegisterVoteForAsync(candidateId, server.Term + 1);
    Thread.Sleep(Utils.GENERAL_BUFFER_TIME);

    // Then
    await candidateServer.DidNotReceiveWithAnyArgs().CountVoteAsync(Arg.Any<bool>());
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
    await server.RegisterVoteForAsync(otherServerId, server.Term + 1);

    // Then
    await otherServer.DidNotReceiveWithAnyArgs().CountVoteAsync(Arg.Any<bool>());
  }
}