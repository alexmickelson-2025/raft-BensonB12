using Logic;
using FluentAssertions;
using NSubstitute;
namespace Tests.Logs;

public class ClientTests
{
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
  /// Testing Logs #18
  /// </summary>
  [Fact]
  public void IfALeaderCannotCommitAnEntryIDoesNotSendAResponseToTheClient()
  {
    // Given

    // When

    // Then
  }
}