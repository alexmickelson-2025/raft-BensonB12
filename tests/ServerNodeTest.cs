namespace tests;

public class ServerNodeTest
{
    [Fact]
    public void CreatedServerNodeIsFollowerState()
    {
        // Given
        ServerNode serverNode = new();

        // When
        ServerNodeState state = serverNode.state;

        // Then
        state.Should().Be(ServerNodeState.FOLLOWER);
    }
}