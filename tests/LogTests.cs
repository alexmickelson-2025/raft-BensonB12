using Logic;
using FluentAssertions;
using NSubstitute;
namespace Tests;

public class LogTests
{
  /// <summary>
  /// Testing Logs #1
  /// </summary>
  [Fact]
  public void WhenALeaderReceivesAClientCommandTheLeaderSendsTheLogEntryInTheNextAppendEntryToAllNodes()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// TestingLogs #2
  /// </summary>
  [Fact]
  public void WhenALeaderReceivesACommandFromTheClientItIsAppendedToItsLog()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #3
  /// </summary>
  [Fact]
  public void WhenAServerIsNewItsLogIsEmpty()
  {
    // Given

    // When

    // Then
  }
}

// A follower rejects a candidate vote if it has larger committed logs
// non committed stuff is not a reason to deny a vote