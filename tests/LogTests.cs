using Logic;
using FluentAssertions;
using NSubstitute;
using System.Threading.Tasks;
namespace Tests;

public class LogTests
{
  /// <summary>
  /// Testing #17
  /// </summary>
  [Fact]
  public async Task WhenAFollowerReceivesLogItSendsAResponse()
  {
    // Given
    int leaderId = 1;

    IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);
    ServerNode server = new([leaderServer]);

    // When
    await server.RPCFromLeaderAsync(new RPCFromLeaderArgs(leaderId, 1));

    // Then
    await leaderServer.Received().RPCFromFollowerAsync(server.Id, true);
  }

  /// <summary>
  /// Testing Logs #1
  /// </summary>
  [Fact]
  public async Task WhenALeaderReceivesAClientCommandTheLeaderSendsTheLogEntryInTheNextAppendEntryToAllNodes()
  {
    // Given
    string log = "log";

    IServerNode followerServer = Utils.CreateIServerNodeSubstituteWithId(1);
    ServerNode leaderServer = new();

    Utils.ServersVoteForLeaderWhenAsked([followerServer], leaderServer);
    leaderServer.AddServersToCluster([followerServer]);

    // When
    Utils.WaitForElectionTimerToRunOut();
    await leaderServer.AppendLogRPCAsync(log);

    // Then
    await followerServer.Received().RPCFromLeaderAsync(Arg.Is<RPCFromLeaderArgs>(args => args.Log == log));
  }

  /// <summary>
  /// TestingLogs #2
  /// </summary>
  [Fact]
  public async Task WhenALeaderReceivesACommandFromTheClientItIsAppendedToItsLog()
  {
    // Given
    string log = "log";

    ServerNode leaderServer = new();

    // When
    Utils.WaitForElectionTimerToRunOut();
    await leaderServer.AppendLogRPCAsync(log);

    // Then
    leaderServer.Logs.Count().Should().Be(1);
    leaderServer.Logs[0].Log.Should().Be(log);
  }

  /// <summary>
  /// Testing Logs #3
  /// </summary>
  [Fact]
  public void WhenAServerIsNewItsLogIsEmpty()
  {
    // Given
    ServerNode leaderServer = new();

    // When & Then
    leaderServer.Logs.Should().BeEmpty();
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
    leaderServer.Logs.NextIndex.Should().Be(0);
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
    leaderServer.AddServersToCluster([followerServer]);

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
    leaderServer.AddServersToCluster([followerServer]);

    // When
    Utils.WaitForElectionTimerToRunOut();

    // Then
    leaderServer.FollowerToNextIndex[followerId].Should().Be(0);
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
    leaderServer.AddServersToCluster([followerServer]);

    // When
    Utils.WaitForElectionTimerToRunOut();
    await leaderServer.AppendLogRPCAsync("log");

    // Then
    await followerServer.Received().RPCFromLeaderAsync(Arg.Is<RPCFromLeaderArgs>(args => args.LogIndex == 1)); // It will be a list of logs eventually
  }

  /// <summary>
  /// Testing Logs #7
  /// </summary>
  [Fact]
  public void WhenAFollowerLearnsThatALogEntryIsCommittedItAppliesTehEntryToItsLocalStateMachine()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #8
  /// </summary>
  [Fact]
  public void WhenTheLeaderHasReceivedAMajorityConfirmationOfALogItCommitsTheLog()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #9
  /// </summary>
  [Fact]
  public void TheLeaderCommitsLogsByIncrementingItsCommittedLogIndex()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #10
  /// </summary>
  [Fact]
  public void GivenAFollowerReceivesAnRPCFromLeaderWithLogsItAddsThoseLogsToPersonalLog()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #11
  /// </summary>
  [Fact]
  public void FollowersRespondToRPCFromLeaderIncludesTheTermAndLogIndex()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #12
  /// </summary>
  [Fact]
  public void WhenALeaderReceivesAMajorityResponsesFromTheClientAfterALogReplicationHeartbeatTheLeaderSendsAConfirmationToClient()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #13
  /// </summary>
  [Fact]
  public void WhenALeaderHasACommittedLogItAppliesItToItsInternalStateMachine()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #14
  /// </summary>
  [Fact]
  public void WhenAFollowerReceivesAValidHeartbeatItIncreasesItsCommitIndexToMatchTheCommitIndexOfTheHeartbeat()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #14
  /// </summary>
  [Fact]
  public void WhenAFollowerReceivesAdHeartbeatThatDoesNotMatchItsIndexItRejectsIt()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #15
  /// </summary>
  [Fact]
  public void WhenSendingAnRPCFromLeaderItIncludesTheIndexAndTermOfTheEntryInItsLogThatIsImmediatelyBeforeTheNewEntries()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #15
  /// </summary>
  [Fact]
  public void FollowerRefusesRPCFromLeaderIfRPCDoesNotHaveALogWithTheSameIndexAndTermThenRefusesTheNewEntries()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #15
  /// </summary>
  [Fact]
  public void FollowerRefusesRPCFromLeaderIfRPCDoesNotHaveTheSameTerm()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #15
  /// </summary>
  [Fact]
  public void FollowerRefusesRPCFromLeaderIfRPCDoesNotHaveANewerTerm()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #15
  /// </summary>
  [Fact]
  public void FollowerHasItsIndexDecreasedByLeaderIfIndexIsGreater()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #15
  /// </summary>
  [Fact]
  public void FollowerDeletesWhatTheyHaveIfIndexIsLessThanOurs()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #15
  /// </summary>
  [Fact]
  public void WhenFollowerRejectsLeaderDecrementsNextIndexForThatFollowerAndTriesAgain()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #16
  /// </summary>
  [Fact]
  public void WhenALeaderSendsAHeartbeatWithALogButDoesNotReceiveResponsesFromAMajorityTheEntryIsUncommitted()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #17
  /// </summary>
  [Fact]
  public void IfLeaderDoesNotReceiveAResponseFromAFollowerTheLeaderContinuesToSendLogEntriesWithThoseLogsInNextHeartbeats()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #18
  /// </summary>
  [Fact]
  public void IfALeaderCannotCommitAnEntryIDoesNotSendAResponseToTheClient()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #19
  /// </summary>
  [Fact]
  public void IfAServerReceivesAnRPCFromLeaderWithLogsThatAreTooFarInTheFutureFromThatServersLocalStateServerRejectsTheRPC()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #20
  /// </summary>
  [Fact]
  public void IfFollowerReceivesRPCFromLeaderWithTermAndIndexThatDoNotMatchFollowerRejectsTheRPCUntilItFindsAMatchingLog()
  {
    // Given

    // When

    // Then
  }
}

// A follower rejects a candidate vote if it has larger committed logs
// non committed stuff is not a reason to deny a vote