using logic;
using FluentAssertions;

namespace tests;

public class ServerNodeTest
{
    [Fact]
    public void WhenALeaderIsActiveItSendsAHeartbeatWithinFiftyMilliseconds()
    {
        // Given
        // ServerNode serverNode = new();

        // When
        // ServerNodeState state = serverNode.state;

        // Then
        // state.Should().Be(ServerNodeState.FOLLOWER);
    }

    [Fact]
    public void WhenANodeReceivesAnAppendEntriesFromAnotherNodeTheFirstNodeKnowsThatTheOtherNodeIsTheLeader()
    {
        // Given
    
        // When
    
        // Then
    }

    [Fact]
    public void WhenANewNodeIsInitialedItIsAFollower()
    {
        // Given
        ServerNode server = new();

        // When
        ServerNodeState state = server.State;

        // Then
        state.Should().Be(ServerNodeState.FOLLOWER);
    }
}