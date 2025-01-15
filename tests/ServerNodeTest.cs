using logic;
using FluentAssertions;

namespace tests;

public class ServerNodeTest
{
    [Fact(Skip = "Felt out of algorithm order")]
    public void WhenALeaderIsActiveItSendsAHeartbeatWithinFiftyMilliseconds()
    {
        // Given
        // ServerNode serverNode = new();

        // When
        // ServerNodeState state = serverNode.state;

        // Then
        // state.Should().Be(ServerNodeState.FOLLOWER);
    }

    [Fact(Skip = "Felt out of algorithm order")]
    public void WhenANodeReceivesAnAppendEntriesFromAnotherNodeTheFirstNodeKnowsThatTheOtherNodeIsTheLeader()
    {
        // Given

        // When

        // Then
    }

    /// <summary>
    /// Testing #3
    /// </summary>
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

    /// <summary>
    /// Testing #4
    /// </summary>
    [Fact]
    public void WhenTheFollowerDoesNotGetAMessageForThreeHundredMillisecondsItStartsAnElection()
    {
        // Given
        ServerNode server = new();

        // When
        Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME);

        // Then
        server.State.Should().Be(ServerNodeState.CANDIDATE);
    }

    /// <summary>
    /// Testing #5
    /// </summary>
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

    /// <summary>
    /// Testing #5
    /// </summary>
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

    /// <summary>
    /// Testing #6
    /// </summary>
    [Fact]
    public void WhenElectionBeginsTheTermIsIncrementedByOne()
    {
        // Given
        ServerNode server = new();
        int firstTerm = server.Term;

        // When
        Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME);

        // Then
        server.Term.Should().BeGreaterThan(firstTerm);
    }

    [Fact(Skip = "Felt out of algorithm order")]
    public void WhenAFollowerReceivesAnAppendEntriesMessageItResetsTheElectionTimer()
    {
        // Given

        // When

        // Then
    }

    /// <summary>
    /// Testing #8
    /// </summary>
    [Fact]
    public void GivenAnElectionWhenACandidateGetsAMajorityVotesItBecomesALeader()
    {
        // Given
        ServerNode server = new();

        // When

        // Server becomes a Candidate
        Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME);

        // Candidate asks for votes (itself) and turns into leader
        Thread.Sleep(300);

        // Then
        server.State.Should().Be(ServerNodeState.LEADER);
    }
}