namespace Tests.AppendingLogs;

public class LeaderRules
{
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
}