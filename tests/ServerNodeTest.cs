using Logic;
using FluentAssertions;
using NSubstitute;
using System.Threading.Tasks;
namespace Tests;

public class ServerNodeTest
{
    const int _generalBufferTime = 15;

    /// <summary>
    /// Testing #1
    /// </summary>
    [Fact]
    public void WhenALeaderIsActiveItSendsAHeartbeatWithinFiftyMilliseconds()
    {
        // Given
        int minimumNumberOfHeartbeats = 3;
        ServerNode leaderNode = new();
        IServerNode followerServer = Substitute.For<IServerNode>();
        followerServer.Id.Returns(1);
        followerServer
            .WhenForAnyArgs(server => server.ThrowBalletForAsync(Arg.Any<int>(), Arg.Any<int>()))
            .Do(async _ =>
            {
                await leaderNode.AcceptVoteAsync(true);
            });
        leaderNode.AddServersToServersCluster([followerServer]);

        // When
        // LeaderNode Becomes Leader and has time to send heartbeat
        Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME + _generalBufferTime);

        for (int i = 0; i < minimumNumberOfHeartbeats; i++)
        {
            Thread.Sleep(Constants.HEARTBEAT_PAUSE);
        }

        // Then
        followerServer.ReceivedCalls().Should().HaveCountGreaterThanOrEqualTo(minimumNumberOfHeartbeats + 1);
    }

    /// <summary>
    /// Testing #2
    /// </summary>
    [Fact]
    public async Task WhenANodeReceivesAnAppendEntriesOrHeartBeatFromAnotherNodeTheFirstNodeKnowsThatTheOtherNodeIsTheLeader()
    {
        // Given
        ServerNode followerServer = new();
        int leaderId = 1;

        // When
        await followerServer.ReceiveHeartBeatAsync(new HeartbeatArguments(1, leaderId));

        // Then
        followerServer.ClusterLeaderId.Should().Be(leaderId);
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
        IServerNode otherServer = Substitute.For<IServerNode>();
        server.AddServersToServersCluster([otherServer]);

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
            server.ElectionTimer.Interval.Should().BeInRange(Constants.INCLUSIVE_MINIMUM_ELECTION_TIME, Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME - 1);
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
        IEnumerable<IGrouping<double, ServerNode>> intervalsSet = servers.GroupBy(server => server.ElectionTimer.Interval);

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

    /// <summary>
    /// Testing #7
    /// </summary>
    [Fact]
    public async Task WhenAFollowerReceivesAnAppendEntriesMessageOrHeartbeatItResetsTheElectionTimer()
    {
        // Given
        ServerNode followerServer = new();
        int waitTime = Constants.INCLUSIVE_MINIMUM_ELECTION_TIME - _generalBufferTime;

        if (waitTime < 1)
        {
            waitTime = 1;
        }

        // When
        for (int i = 0; i < (Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME / waitTime) + 1; i++)
        {
            Thread.Sleep(waitTime);
            await followerServer.ReceiveHeartBeatAsync(new HeartbeatArguments(followerServer.Term + 1, 1));
        }

        // Then
        followerServer.State.Should().Be(ServerNodeState.FOLLOWER);
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

        // Server becomes a Candidate and then becomes leader in buffer time
        Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME + _generalBufferTime);

        // Then
        server.State.Should().Be(ServerNodeState.LEADER);
    }

    /// <summary>
    /// Tests #9
    /// </summary>
    [Fact]
    public void CandidateReceivesMajorityVotesWhileWaitingForUnresponsiveNodeStillBecomesLeader()
    {
        // Given
        ServerNode leaderNode = new();
        IServerNode followerOne = Substitute.For<IServerNode>();
        IServerNode followerTwo = Substitute.For<IServerNode>();
        followerOne.Id.Returns(1);
        followerTwo.Id.Returns(2);
        followerOne
            .WhenForAnyArgs(server => server.ThrowBalletForAsync(Arg.Any<int>(), Arg.Any<int>()))
            .Do(async _ =>
            {
                await leaderNode.AcceptVoteAsync(true);
            });

        leaderNode.AddServersToServersCluster([followerOne, followerTwo]);


        // When
        Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME + _generalBufferTime);

        // Then
        leaderNode.State.Should().Be(ServerNodeState.LEADER);
    }

    /// <summary>
    /// Testing #10
    /// </summary>
    [Fact]
    public async Task FollowerThatHasNotVotedAndIsInEarlierTermSendsYes()
    {
        // Given
        int leaderId = 1;
        ServerNode server = new();
        IServerNode leaderNode = Substitute.For<IServerNode>();
        leaderNode.Id.Returns(leaderId);
        server.AddServersToServersCluster([leaderNode]);

        // When
        await server.ThrowBalletForAsync(leaderId, server.Term + 1);

        // Then
        await leaderNode.Received().AcceptVoteAsync(true);
    }

    /// <summary>
    /// Testing #11
    /// </summary>
    [Fact]
    public void CandidateVotesForItself()
    {
        // Given
        ServerNode candidate = new();

        // When
        Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME + _generalBufferTime);

        // Then
        // If it is the only node, then majority must be one, therefore it must have received at least one vote, from itself
        candidate.State.Should().Be(ServerNodeState.LEADER);
    }

    /// <summary>
    /// Testing #12
    /// </summary>
    [Fact]
    public async Task GivenACandidateServerWhenItReceivesAHeartbeatOrAppendEntriesMessageFromALaterTermItBecomesAFollowerAndLoses()
    {
        // Given
        ServerNode candidateServer = new();

        // When
        while (candidateServer.State == ServerNodeState.FOLLOWER)
        {
            // Do I do something here?
        }

        await candidateServer.ReceiveHeartBeatAsync(new HeartbeatArguments(1, candidateServer.Term + 1));

        // Then
        candidateServer.State.Should().Be(ServerNodeState.FOLLOWER);
    }

    /// <summary>
    /// Testing #13
    /// </summary>
    [Fact]
    public async Task GivenACandidateServerWhenItReceivesAHeartbeatOrAppendEntriesMessageFromTheSameTermItBecomesAFollowerAndLoses()
    {
        // Given
        ServerNode candidateServer = new();

        // When
        while (candidateServer.State == ServerNodeState.FOLLOWER)
        {
            // Do I do something here?
        }

        await candidateServer.ReceiveHeartBeatAsync(new HeartbeatArguments(1, candidateServer.Term));

        // Then
        candidateServer.State.Should().Be(ServerNodeState.FOLLOWER);
    }

    /// <summary>
    /// Testing # 14
    /// </summary>
    [Fact]
    public async Task ServerNodeRespondsNoToAnotherVoteInSameTerm()
    {
        // Given
        int candidateOneId = 1;
        int candidateTwoId = 2;
        int term = 3;
        ServerNode server = new();
        IServerNode candidateNodeOne = Substitute.For<IServerNode>();
        IServerNode candidateNodeTwo = Substitute.For<IServerNode>();
        candidateNodeOne.Id.Returns(candidateOneId);
        candidateNodeTwo.Id.Returns(candidateTwoId);
        server.AddServersToServersCluster([candidateNodeOne, candidateNodeTwo]);

        // When
        await server.ThrowBalletForAsync(candidateOneId, term);
        await server.ThrowBalletForAsync(candidateTwoId, term);

        // Then
        await candidateNodeTwo.Received().AcceptVoteAsync(false);
    }

    /// <summary>
    /// Testing #15
    /// </summary>
    [Fact]
    public async Task ServerVotesInEarlierTermButIsAskedToVoteInAHigherTermStillSaysYes()
    {
        // Given
        int candidateOneId = 1;
        int candidateTwoId = 2;
        int term = 3;
        ServerNode server = new();
        IServerNode candidateNodeOne = Substitute.For<IServerNode>();
        IServerNode candidateNodeTwo = Substitute.For<IServerNode>();
        candidateNodeOne.Id.Returns(candidateOneId);
        candidateNodeTwo.Id.Returns(candidateTwoId);
        server.AddServersToServersCluster([candidateNodeOne, candidateNodeTwo]);

        // When
        await server.ThrowBalletForAsync(candidateOneId, term);
        await server.ThrowBalletForAsync(candidateTwoId, term + 1);

        // Then
        await candidateNodeTwo.Received().AcceptVoteAsync(true);
    }

    /// <summary>
    /// Testing #16
    /// </summary>
    [Fact]
    public void GivenACandidateStartsAnElectionAndThenTheirTimerRunsOutNewElectionHasStarted()
    {
        // Given
        ServerNode server = new();
        IServerNode serverMock = Substitute.For<IServerNode>();
        server.AddServersToServersCluster([serverMock]);

        // When
        Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME);
        int firstCandidateTerm = server.Term;
        Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME);

        // Then
        server.State.Should().Be(ServerNodeState.CANDIDATE);
        server.Term.Should().BeGreaterThan(firstCandidateTerm);
    }

    /// <summary>
    /// Testing #17
    /// </summary>
    [Fact]
    public async Task WhenAFollowerReceivesAppendEntriesItSendsAResponse()
    {
        // Given
        int leaderId = 1;
        ServerNode server = new();
        IServerNode leaderNode = Substitute.For<IServerNode>();
        leaderNode.Id.Returns(leaderId);
        server.AddServersToServersCluster([leaderNode]);

        // When
        await server.ReceiveAppendEntriesAsync(leaderId, 1);

        // Then
        await leaderNode.Received().AppendEntryResponseAsync(server.Id, true);
    }

    /// <summary>
    /// Testing #18
    /// </summary>
    [Fact]
    public async Task GivenCandidateReceivesAppendEntriesCandidateRejects()
    {
        // Given
        int leaderId = 1;
        ServerNode server = new();
        IServerNode leaderNode = Substitute.For<IServerNode>();
        leaderNode.Id.Returns(leaderId);
        server.AddServersToServersCluster([leaderNode]);

        // When
        Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME + _generalBufferTime);
        await server.ReceiveAppendEntriesAsync(leaderId, 0);

        // Then
        await leaderNode.Received().AppendEntryResponseAsync(server.Id, false);
    }

    /// <summary>
    /// Testing #19
    /// </summary>
    [Fact]
    public void WhenACandidateWinsAnElectionItImmediatelySendsAHeartBeat()
    {
        // Given
        ServerNode leaderNode = new();
        IServerNode followerServer = Substitute.For<IServerNode>();
        followerServer.Id.Returns(1);
        followerServer
            .When(server => server.ThrowBalletForAsync(Arg.Any<int>(), Arg.Any<int>()))
            .Do(async _ =>
            {
                await leaderNode.AcceptVoteAsync(true);
            });
        leaderNode.AddServersToServersCluster([followerServer]);

        // When
        // LeaderNode Becomes Leader and has time to send heartbeat
        Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME + _generalBufferTime);

        // Then
        followerServer.Received().ReceiveHeartBeatAsync(Arg.Any<HeartbeatArguments>());
    }

    /// <summary>
    /// Test for 5.3 log replication assignment
    /// </summary>
    // [Fact]
    // public void AfterALeaderHasGottenALogCommandAllServersAreUpAfterEnoughTimeForTheServersToReceiveItTheyAllHaveTheSameLog()
    // {
    //     // Given
    //     IServerNode mockServerOne = Substitute.For<IServerNode>();
    //     IServerNode mockServerTwo = Substitute.For<IServerNode>();
    //     ServerNode leaderNode = new();
    //     leaderNode.AddServersToServersCluster([mockServerOne, mockServerTwo]);

    //     // When
    //     leaderNode.AcceptLogAsync("one");
    //     Thread.Sleep(300);

    //     // Then
    //     leaderNode.Logs.Should().Be(["one"]);
    //     mockServerOne.Received().AppendEtries("one");
    //     mockServerTwo.Received().AppendEtries("one");
    // }
}