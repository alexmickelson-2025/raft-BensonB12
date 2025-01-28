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
    await leaderServer.Received().RPCResponseAsyncFromFollowerAsync(server.Id, true);
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
    ServerNode leaderServer = new([followerServer]);

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
    leaderServer.Logs.Should().BeEquivalentTo([log]);
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
}

// A follower rejects a candidate vote if it has larger committed logs
// non committed stuff is not a reason to deny a vote