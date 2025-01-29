using FluentAssertions;
using Logic.Models.Args;
using Logic.Models.Server;
using Logic.Utils;

namespace Tests;

public class ElectionTimerTests
{
  /// <summary>
  /// Testing #5
  /// </summary>
  [Fact]
  public void WhenTheElectionTimeIsResetItIsARandomValueBetweenTheElectionTimerConstants()
  {
    // Given / When
    List<ServerNode> servers = [];

    for (int i = 0; i < 10; i++)
    {
      servers.Add(new());
    }

    // Then
    foreach (ServerNode server in servers)
    {
      server.ElectionTimer.Interval.Should().BeInRange(Constants.INCLUSIVE_MINIMUM_ELECTION_TIME, Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME - 1);
    }
  }

  /// <summary>
  /// Testing #5
  /// </summary>
  [Fact]
  public void WhenTheElectionTimeIsResetItIsARandomValueBetweenOtherServers()
  {
    // Given / When
    List<ServerNode> servers = [];

    for (int i = 0; i < 10; i++)
    {
      servers.Add(new());
    }

    IEnumerable<IGrouping<double, ServerNode>> intervalsSet = servers.GroupBy(server => server.ElectionTimer.Interval);

    // Then
    intervalsSet.Count().Should().NotBe(1);
  }

  /// <summary>
  /// Testing #6
  /// </summary>
  [Fact]
  public void WhenElectionBeginsTheTermIsIncrementedByOne()
  {
    // Given
    ServerNode server = new();

    uint firstTerm = server.Term;

    // When
    Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME);

    // Then
    server.Term.Should().BeGreaterThan(firstTerm);
  }

  /// <summary>
  /// Testing #7
  /// </summary>
  [Fact]
  public async Task WhenAFollowerReceivesAnAppendEntriesMessageOrHeartbeatItResetsTheElectionTimer()
  {
    // Given
    int waitTime = Constants.INCLUSIVE_MINIMUM_ELECTION_TIME - Utils.GENERAL_BUFFER_TIME;
    int leaderId = 1;

    IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);
    ServerNode followerServer = new([leaderServer]);

    if (waitTime < 1)
    {
      waitTime = 1;
    }

    // When
    for (int i = 0; i < (Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME / waitTime) + 1; i++)
    {
      Thread.Sleep(waitTime);
      await followerServer.RPCFromLeaderAsync(new RPCFromLeaderArgs(leaderId, followerServer.Term + 1));
    }

    // Then
    followerServer.State.Should().Be(ServerNodeState.FOLLOWER);
  }
}