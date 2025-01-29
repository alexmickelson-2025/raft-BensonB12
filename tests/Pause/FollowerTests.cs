using System.Threading.Tasks;
using FluentAssertions;
using Logic.Exceptions;
using Logic.Models.Args;
using Logic.Models.Server;
using NSubstitute;

namespace Tests.Pause;

public class FollowerTests
{
  /// <summary>
  /// Testing Pausing #5
  /// </summary>
  [Fact]
  public async Task WhenServerIsPausedItDoesNotLog()
  {
    // Given
    ServerNode server = new();

    // When
    server.Pause();
    await server.AppendLogRPCAsync("log");

    // Then
    server.LogMessages.Should().BeEmpty();
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
    await leaderServer.DidNotReceiveWithAnyArgs().RPCFromFollowerAsync(Arg.Any<int>(), Arg.Any<bool>());
  }

  /// <summary>
  /// Testing Pausing #6
  /// </summary>
  [Fact]
  public async Task WhenServerIsUnpausedWithoutBeingPausedItThrowsError()
  {
    // Given
    ServerNode server = new();

    // When & Then
    await FluentActions.Invoking(() => server.Unpause())
        .Should()
        .ThrowAsync<UnpausedARunningServerException>();
  }
}