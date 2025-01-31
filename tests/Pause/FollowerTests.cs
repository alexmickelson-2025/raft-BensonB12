using FluentAssertions;
using Logic.Exceptions;
using Logic.Models.Args;
using Logic.Models.Server;
using Logic.Utils;
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
    ServerNode leaderServer = new();
    IServerNode followerServer = Utils.CreateIServerNodeSubstituteWithId(1);

    Utils.ServersVoteForLeaderWhenAsked([followerServer], leaderServer);
    leaderServer.InitializeClusterWithServers([followerServer]);

    // When
    Utils.WaitForElectionTimerToRunOut();
    await leaderServer.Pause();
    await leaderServer.AppendLogRPCAsync("log", 0);

    // Then
    await followerServer.DidNotReceive().RPCFromLeaderAsync(Arg.Is<RPCFromLeaderArgs>(args => args.PreviousLogIndex != -1));
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
    await server.Pause();
    await server.RPCFromLeaderAsync(new RPCFromLeaderArgs(leaderId, server.Term + 1, -1, 1, -1));

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