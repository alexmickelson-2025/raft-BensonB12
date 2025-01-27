using FluentAssertions;
using Logic;

namespace Tests;

public class StateTests
{
  /// <summary>
  /// Testing #3
  /// </summary>
  [Fact]
  public void WhenANewServerIsInitialedItIsAFollower()
  {
    // Given
    ServerNode server = new();

    // When
    ServerNodeState state = server.State;

    // Then
    state.Should().Be(ServerNodeState.FOLLOWER);
  }
}