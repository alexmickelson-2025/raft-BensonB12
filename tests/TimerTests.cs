using System.Timers;
using FluentAssertions;
using Logic.Models.Args;
using Logic.Models.Server;
using Logic.Models.Server.Election;
using Logic.Utils;

namespace Tests;

public class ElectionTimerTests
{
  static Task _dummyElectionHandler(object? obj, ElapsedEventArgs elapsedEventArgs) => Task.CompletedTask;

  /// <summary>
  /// Testing #5
  /// </summary>
  [Fact]
  public void WhenTheElectionTimeIsResetItIsARandomValueBetweenTheElectionTimerConstants()
  {
    // Given / When
    List<ElectionData> electionTimerObjects = [];

    for (int i = 0; i < 10; i++)
    {
      electionTimerObjects.Add(new(_dummyElectionHandler));
    }

    // Then
    foreach (ElectionData timerObj in electionTimerObjects)
    {
      timerObj.ElectionTimer.Interval.Should().BeInRange(Constants.INCLUSIVE_MINIMUM_ELECTION_INTERVAL, Constants.EXCLUSIVE_MAXIMUM_ELECTION_INTERVAL - 1);
    }
  }

  /// <summary>
  /// Testing #5
  /// </summary>
  [Fact]
  public void WhenTheElectionTimeIsResetItIsARandomValueBetweenOtherServers()
  {
    // Given / When
    List<ElectionData> electionTimerObjects = [];

    for (int i = 0; i < 10; i++)
    {
      electionTimerObjects.Add(new(_dummyElectionHandler));
    }

    IEnumerable<IGrouping<double, ElectionData>> intervalsSet = electionTimerObjects.GroupBy(obj => obj.ElectionTimer.Interval);

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
    Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_INTERVAL);

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
    int waitTime = Constants.INCLUSIVE_MINIMUM_ELECTION_INTERVAL - Utils.GENERAL_BUFFER_TIME;
    int leaderId = 1;

    IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);
    ServerNode followerServer = new([leaderServer]);

    if (waitTime < 1)
    {
      waitTime = 1;
    }

    // When
    for (int i = 0; i < (Constants.EXCLUSIVE_MAXIMUM_ELECTION_INTERVAL / waitTime) + 1; i++)
    {
      Thread.Sleep(waitTime);
      await followerServer.RPCFromLeaderAsync(new RPCFromLeaderArgs(leaderId, followerServer.Term + 1, 0, 0, 0));
    }

    // Then
    followerServer.State.Should().Be(ServerNodeState.FOLLOWER);
  }
}