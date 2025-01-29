using Logic;
using FluentAssertions;
using NSubstitute;
namespace Tests.Logs;

public class FollowerRejectBecauseLogs
{
  /// <summary>
  /// Testing Logs #14
  /// </summary>
  [Fact]
  public void WhenAFollowerReceivesAHeartbeatThatDoesNotMatchItsIndexItRejectsIt()
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