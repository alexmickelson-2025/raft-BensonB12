using FluentAssertions;
using NSubstitute;
using Logic.Models.Server;
using Logic.Models.Args;
using Logic.Models.Server.Logging;
namespace Tests.AppendingLogs;

public class LogTests
{
  /// <summary>
  /// Testing Logs #3
  /// </summary>
  [Fact]
  public void WhenAServerIsNewItsLogIsEmpty()
  {
    // Given
    LogHandler logHandler = new([]);

    // When & Then
    logHandler.Messages.Should().BeEmpty();
  }

  /// <summary>
  /// Testing Logs #4
  /// </summary>
  [Fact]
  public void WhenALeaderWinsAnElectionItInitializesTheNextIndexCorrectly()
  {
    // Given
    LogHandler logHandler = new([0, 1, 2]);

    // When & Then
    logHandler.NextIndex.Should().Be(0);
  }

  /// <summary>
  /// Testing Logs #4
  /// </summary>
  [Fact]
  public async Task WhenALeaderWinsAnElectionItInitializesTheNextIndexForEachFollowerToIndexJustAfterTheLastLog()
  {
    // Given
    IServerNode followerServer = Utils.CreateIServerNodeSubstituteWithId(1);
    ServerNode leaderServer = new();

    Utils.ServersVoteForLeaderWhenAsked([followerServer], leaderServer);
    leaderServer.InitializeClusterWithServers([followerServer]);

    // When
    Utils.WaitForElectionTimerToRunOut();

    // Then
    await followerServer.Received().RPCFromLeaderAsync(Arg.Is<RPCFromLeaderArgs>(args => args.PreviousLogIndex == 0));
  }

  /// <summary>
  /// Testing Logs #5
  /// </summary>
  [Fact]
  public void LeaderMaintainsNextIndexForEachFollower()
  {
    // Given
    LogHandler logHandler = new([0, 1, 2]);

    // When & Then
    logHandler.FollowerToNextIndex.Keys.Count.Should().Be(3);
  }

  /// <summary>
  /// Testing Logs #6
  /// </summary>
  [Fact]
  public async Task HighestCommittedIndexFromTheLeaderIsIncludedInRPCFromLeader()
  {
    // Given
    IServerNode followerServer = Utils.CreateIServerNodeSubstituteWithId(1);
    ServerNode leaderServer = new();

    Utils.ServersVoteForLeaderWhenAsked([followerServer], leaderServer);
    leaderServer.InitializeClusterWithServers([followerServer]);

    // When
    Utils.WaitForElectionTimerToRunOut();
    await leaderServer.AppendLogRPCAsync("log", 0);

    // Then
    await followerServer.Received().RPCFromLeaderAsync(Arg.Is<RPCFromLeaderArgs>(args => args.PreviousLogTerm == 1)); // It will be a list of logs eventually
  }
}