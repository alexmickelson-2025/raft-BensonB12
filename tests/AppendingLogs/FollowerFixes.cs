using Logic;
using FluentAssertions;
using NSubstitute;
namespace Tests.AppendingLogs;

public class FollowerFixes
{
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
  /// Testing Logs #17
  /// </summary>
  [Fact]
  public void IfLeaderDoesNotReceiveAResponseFromAFollowerTheLeaderContinuesToSendLogEntriesWithThoseLogsInNextHeartbeats()
  {
    // Given

    // When

    // Then
  }
}