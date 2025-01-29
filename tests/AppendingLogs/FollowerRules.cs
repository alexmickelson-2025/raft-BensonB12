using NSubstitute;
using Logic.Models.Server;
using Logic.Models.Args;
namespace Tests.AppendingLogs;

public class FollowerRules
{
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
  /// Testing Logs #7
  /// </summary>
  [Fact]
  public void WhenAFollowerLearnsThatALogEntryIsCommittedItAppliesTheEntryToItsLocalStateMachine()
  {
    // Given

    // When

    // Then
  }

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
}

// A follower rejects a candidate vote if it has larger committed logs
// non committed stuff is not a reason to deny a vote