using FluentAssertions;
using NSubstitute;
using Logic.Models.Server;
using Logic.Models.Args;
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
    ServerNode leaderServer = new();

    // When & Then
    // leaderServer.LogMessages.Should().BeEmpty();
    Assert.Fail();
  }

  /// <summary>
  /// Testing Logs #4
  /// </summary>
  [Fact]
  public void WhenALeaderWinsAnElectionItInitializesTheNextIndexCorrectly()
  {
    // Given
    ServerNode leaderServer = new();

    // When
    Utils.WaitForElectionTimerToRunOut();

    // Then
    // leaderServer.LogMessages.NextIndex.Should().Be(0);
    Assert.Fail();
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
    await followerServer.Received().SetNextIndexToAsync(Arg.Is<SetNextIndexToArgs>(args => args.NextIndex == 0));
  }

  /// <summary>
  /// Testing Logs #5
  /// </summary>
  [Fact]
  public void LeaderMaintainsNextIndexForEachFollower()
  {
    // Given
    int followerId = 1;

    IServerNode followerServer = Utils.CreateIServerNodeSubstituteWithId(followerId);
    ServerNode leaderServer = new();

    Utils.ServersVoteForLeaderWhenAsked([followerServer], leaderServer);
    leaderServer.InitializeClusterWithServers([followerServer]);

    // When
    Utils.WaitForElectionTimerToRunOut();

    // Then
    // leaderServer.FollowerToNextIndex[followerId].Should().Be(0);
    Assert.Fail();
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
    await leaderServer.AppendLogRPCAsync("log");

    // Then
    await followerServer.Received().RPCFromLeaderAsync(Arg.Is<RPCFromLeaderArgs>(args => args.LogIndex == 1)); // It will be a list of logs eventually
  }
}