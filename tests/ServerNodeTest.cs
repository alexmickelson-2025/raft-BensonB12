using logic;
using FluentAssertions;
using System.Reflection.Metadata;

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

    [Fact]
    public void WhenTheFollowerDoesNotGetAMessageForThreeHundredMillisecondsItStartsAnElection()
    {
        // Given
        ServerNode server = new();

        // When
        Thread.Sleep(300);

        // Then
        server.State.Should().Be(ServerNodeState.CANDIDATE);
    }

    [Fact]
    public void WhenTheElectionTimeIsResetItIsARandomValueBetweenTheElectionTimerConstants()
    {
        // Given / When
        List<ServerNode> servers = [];
        for (int i = 0; i < 10; i++)
        {
            servers.Add(new());
        }

        // Then
        foreach (ServerNode server in servers)
        {
            server.ElectionTimerInterval.Should().BeInRange(Constants.INCLUSIVE_MINIMUM_ELECTION_TIME, Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME - 1);
        }
    }

    [Fact]
    public void WhenTheElectionTimeIsResetItIsARandomValueBetweenOtherServers()
    {
        // Given
        List<ServerNode> servers = [];
        for (int i = 0; i < 10; i++)
        {
            servers.Add(new());
        }

        // When
        IEnumerable<IGrouping<int, ServerNode>> intervalsSet = servers.GroupBy(server => server.ElectionTimerInterval);

        // Then
        intervalsSet.Count().Should().NotBe(1);
    }
}