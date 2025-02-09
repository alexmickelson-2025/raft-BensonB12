using Logic;
using FluentAssertions;
using NSubstitute;
using Logic.Models.Server;
using Logic.Models.Args;
using Logic.Models.Server.Logging;
using System.Threading.Tasks;
namespace Tests.AppendingLogs;

public class FollowerFixes
{
  /// <summary>
  /// Testing Logs #15
  /// </summary>
  [Fact]
  public async Task FollowerHasItsIndexDecreasedByLeaderIfIndexIsGreater()
  {
    // Given
    int leaderId = 0;
    uint term = 2;

    IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);
    ServerNode follower = new(otherServers: [leaderServer]);

    // When
    await follower.RPCFromLeaderAsync(new RPCFromLeaderArgs(leaderId, term, null, null, null, newLogs: [new LogData(term, "log", 0)]));
    Thread.Sleep(Utils.GENERAL_BUFFER_TIME);
    await follower.RPCFromLeaderAsync(new RPCFromLeaderArgs(leaderId, term, null, null, null, newLogs: [new LogData(term, "log", 0)]));

    // Then
    Assert.Fail();
  }

  /// <summary>
  /// Testing Logs #15
  /// </summary>
  [Fact]
  public async Task FollowerDeletesWhatTheyHaveIfIndexIsLessThanOurs()
  {
    // Given
    int leaderId = 0;
    uint term = 2;

    IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);
    ServerNode follower = new(otherServers: [leaderServer]);

    // When
    await follower.RPCFromLeaderAsync(new RPCFromLeaderArgs(leaderId, term, null, null, null, newLogs: [new LogData(term, "log", 0)]));
    Thread.Sleep(Utils.GENERAL_BUFFER_TIME);
    await follower.RPCFromLeaderAsync(new RPCFromLeaderArgs(leaderId, term, 0, 0, 0, newLogs: [new LogData(term, "log", 0)]));
    Thread.Sleep(Utils.GENERAL_BUFFER_TIME);
    await follower.RPCFromLeaderAsync(new RPCFromLeaderArgs(leaderId, term, null, null, null, newLogs: [new LogData(term, "log", 0)]));
    
    // Then
    Assert.Fail();
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
    Assert.Fail();
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
    Assert.Fail();
  }
}