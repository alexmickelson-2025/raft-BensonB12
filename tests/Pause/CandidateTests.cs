using FluentAssertions;
using Logic.Models.Args;
using Logic.Models.Server;
using NSubstitute;

namespace Tests.Pause;

public class CandidateTests
{
  /// <summary>
  /// Testing Pausing #3
  /// </summary>
  [Fact]
  public async Task WhenServerGetsPausedTheyDoNotBecomeACandidate()
  {
    // Given
    ServerNode server = new();

    // When
    await server.RPCFromClientAsync(new RPCFromClientArgs(0, serverShouldBePaused: true));
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
    await server.RPCFromClientAsync(new RPCFromClientArgs(0, serverShouldBePaused: true));
    Utils.WaitForElectionTimerToRunOut();

    // Then
    await otherServer.DidNotReceiveWithAnyArgs().RPCFromCandidateAsync(Arg.Any<RPCFromCandidateArgs>());
  }

  /// <summary>
  /// Testing Pausing #4
  /// </summary>
  [Fact]
  public async Task WhenAFollowerGetsPausedAndUnpausedItStillBecomesACandidate()
  {
    // Given
    ServerNode server = new();

    // When
    await server.RPCFromClientAsync(new RPCFromClientArgs(0, serverShouldBePaused: true));
    await server.RPCFromClientAsync(new RPCFromClientArgs(0, serverShouldBePaused: false));
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
    await server.RPCFromClientAsync(new RPCFromClientArgs(0, serverShouldBePaused: true));
    await server.RPCFromCandidateAsync(new RPCFromCandidateArgs(candidateId, server.Term + 1));
    Thread.Sleep(Utils.GENERAL_BUFFER_TIME);

    // Then
    await candidateServer.DidNotReceiveWithAnyArgs().RPCFromFollowerAsync(Arg.Any<RPCFromFollowerArgs>());
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
    await server.RPCFromClientAsync(new RPCFromClientArgs(0, serverShouldBePaused: true));
    await server.RPCFromCandidateAsync(new RPCFromCandidateArgs(otherServer.Id, server.Term + 1));

    // Then
    await otherServer.DidNotReceiveWithAnyArgs().RPCFromFollowerAsync(Arg.Any<RPCFromFollowerArgs>());
  }
}