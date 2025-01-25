using Logic;
using FluentAssertions;
using NSubstitute;
namespace Tests;

public class ElectionTests
{
    /// <summary>
    /// Testing #1
    /// </summary>
    [Fact]
    public void WhenALeaderIsActiveItSendsAHeartbeatWithinFiftyMilliseconds()
    {
        // Given
        int minimumNumberOfHeartbeats = 3;

        ServerNode leaderServer = new();
        IServerNode followerServer = Utils.CreateIServerNodeSubstituteWithId(1);

        followerServer
            .WhenForAnyArgs(server => server.ThrowBalletForAsync(Arg.Any<int>(), Arg.Any<uint>()))
            .Do(async _ =>
            {
                await leaderServer.AcceptVoteAsync(true);
            });

        leaderServer.AddServersToServersCluster([followerServer]);

        // When
        Utils.WaitForElectionTimerToRunOut();

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
        int leaderId = 1;

        IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);
        ServerNode followerServer = new([leaderServer]);

        // When
        await followerServer.ReceiveLeaderToFollowerRemoteProcedureCallAsync(new LeaderToFollowerRemoteProcedureCallArguments(leaderId, 1));

        // Then
        followerServer.ClusterLeaderId.Should().Be(leaderId);
    }

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

    /// <summary>
    /// Testing #4
    /// </summary>
    [Fact]
    public void WhenTheFollowerDoesNotGetAMessageForThreeHundredMillisecondsItStartsAnElection()
    {
        // Given
        IServerNode otherServer = Substitute.For<IServerNode>();
        ServerNode server = new([otherServer]);

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
        // Given / When
        List<ServerNode> servers = [];

        for (int i = 0; i < 10; i++)
        {
            servers.Add(new());
        }

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

        uint firstTerm = server.Term;

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
        int waitTime = Constants.INCLUSIVE_MINIMUM_ELECTION_TIME - Utils.GENERAL_BUFFER_TIME;
        int leaderId = 1;

        IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);
        ServerNode followerServer = new([leaderServer]);

        if (waitTime < 1)
        {
            waitTime = 1;
        }

        // When
        for (int i = 0; i < (Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME / waitTime) + 1; i++)
        {
            Thread.Sleep(waitTime);
            await followerServer.ReceiveLeaderToFollowerRemoteProcedureCallAsync(new LeaderToFollowerRemoteProcedureCallArguments(leaderId, followerServer.Term + 1));
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
        Utils.WaitForElectionTimerToRunOut();

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
        ServerNode leaderServer = new();
        IServerNode followerOne = Utils.CreateIServerNodeSubstituteWithId(1);
        IServerNode followerTwo = Utils.CreateIServerNodeSubstituteWithId(2);

        followerOne
            .WhenForAnyArgs(server => server.ThrowBalletForAsync(Arg.Any<int>(), Arg.Any<uint>()))
            .Do(async _ =>
            {
                await leaderServer.AcceptVoteAsync(true);
            });

        leaderServer.AddServersToServersCluster([followerOne, followerTwo]);


        // When
        Utils.WaitForElectionTimerToRunOut();

        // Then
        leaderServer.State.Should().Be(ServerNodeState.LEADER);
    }

    /// <summary>
    /// Testing #10
    /// </summary>
    [Fact]
    public async Task FollowerThatHasNotVotedAndIsInEarlierTermSendsYes()
    {
        // Given
        int leaderId = 1;

        IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);

        ServerNode server = new([leaderServer]);

        // When
        await server.ThrowBalletForAsync(leaderId, server.Term + 1);

        // Then
        await leaderServer.Received().AcceptVoteAsync(true);
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
        Utils.WaitForElectionTimerToRunOut();

        // Then
        // If it is the only server, then majority must be one, therefore it must have received at least one vote, from itself
        candidate.State.Should().Be(ServerNodeState.LEADER);
    }

    /// <summary>
    /// Testing #12
    /// </summary>
    [Fact]
    public async Task GivenACandidateServerWhenItReceivesAHeartbeatOrAppendEntriesMessageFromALaterTermItBecomesAFollowerAndLoses()
    {
        // Given
        int leaderId = 1;

        IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);
        ServerNode candidateServer = new([leaderServer]);

        // When
        while (candidateServer.State == ServerNodeState.FOLLOWER)
        {
            // Do I do something here?
        }

        await candidateServer.ReceiveLeaderToFollowerRemoteProcedureCallAsync(new LeaderToFollowerRemoteProcedureCallArguments(leaderId, candidateServer.Term + 1));

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
        int leaderId = 1;

        IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);
        ServerNode candidateServer = new([leaderServer]);

        // When
        while (candidateServer.State == ServerNodeState.FOLLOWER)
        {
            // Do I do something here?
        }

        await candidateServer.ReceiveLeaderToFollowerRemoteProcedureCallAsync(new LeaderToFollowerRemoteProcedureCallArguments(leaderId, candidateServer.Term));

        // Then
        candidateServer.State.Should().Be(ServerNodeState.FOLLOWER);
    }

    /// <summary>
    /// Testing # 14
    /// </summary>
    [Fact]
    public async Task ServerRespondsNoToAnotherVoteInSameTerm()
    {
        // Given
        uint term = 3;
        int candidateOneId = 1;
        int candidateTwoId = 2;

        IServerNode candidateServerOne = Utils.CreateIServerNodeSubstituteWithId(candidateOneId);
        IServerNode candidateServerTwo = Utils.CreateIServerNodeSubstituteWithId(candidateTwoId);
        ServerNode server = new([candidateServerOne, candidateServerTwo]);

        // When
        await server.ThrowBalletForAsync(candidateOneId, term);
        await server.ThrowBalletForAsync(candidateTwoId, term);

        // Then
        await candidateServerTwo.Received().AcceptVoteAsync(false);
    }

    /// <summary>
    /// Testing #15
    /// </summary>
    [Fact]
    public async Task ServerVotesInEarlierTermButIsAskedToVoteInAHigherTermStillSaysYes()
    {
        // Given
        uint term = 3;
        int candidateOneId = 1;
        int candidateTwoId = 2;

        IServerNode candidateServerOne = Utils.CreateIServerNodeSubstituteWithId(candidateOneId);
        IServerNode candidateServerTwo = Utils.CreateIServerNodeSubstituteWithId(candidateTwoId);
        ServerNode server = new([candidateServerOne, candidateServerTwo]);

        // When
        await server.ThrowBalletForAsync(candidateOneId, term);
        await server.ThrowBalletForAsync(candidateTwoId, term + 1);

        // Then
        await candidateServerTwo.Received().AcceptVoteAsync(true);
    }

    /// <summary>
    /// Testing #16
    /// </summary>
    [Fact]
    public void GivenACandidateStartsAnElectionAndThenTheirTimerRunsOutNewElectionHasStarted()
    {
        // Given
        IServerNode otherServer = Substitute.For<IServerNode>();
        ServerNode server = new([otherServer]);

        // When
        Utils.WaitForElectionTimerToRunOut();

        uint firstCandidateTerm = server.Term;

        Utils.WaitForElectionTimerToRunOut();

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

        IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);
        ServerNode server = new([leaderServer]);

        // When
        await server.ReceiveLeaderToFollowerRemoteProcedureCallAsync(new LeaderToFollowerRemoteProcedureCallArguments(leaderId, 1));

        // Then
        await leaderServer.Received().LeaderToFollowerRemoteProcedureCallResponse(server.Id, true);
    }

    /// <summary>
    /// Testing #18
    /// </summary>
    [Fact]
    public async Task GivenCandidateReceivesAppendEntriesCandidateRejects()
    {
        // Given
        int leaderId = 1;

        IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);
        ServerNode server = new([leaderServer]);

        // When
        Utils.WaitForElectionTimerToRunOut();
        await server.ReceiveLeaderToFollowerRemoteProcedureCallAsync(new LeaderToFollowerRemoteProcedureCallArguments(leaderId, 0));

        // Then
        await leaderServer.Received().LeaderToFollowerRemoteProcedureCallResponse(server.Id, false);
    }

    /// <summary>
    /// Testing #19
    /// </summary>
    [Fact]
    public void WhenACandidateWinsAnElectionItImmediatelySendsAHeartBeat()
    {
        // Given
        ServerNode leaderServer = new();

        IServerNode followerServer = Utils.CreateIServerNodeSubstituteWithId(1);
        followerServer
            .When(server => server.ThrowBalletForAsync(Arg.Any<int>(), Arg.Any<uint>()))
            .Do(async _ =>
            {
                await leaderServer.AcceptVoteAsync(true);
            });

        leaderServer.AddServersToServersCluster([followerServer]);

        // When
        Utils.WaitForElectionTimerToRunOut();

        // Then
        followerServer.Received().ReceiveLeaderToFollowerRemoteProcedureCallAsync(Arg.Any<LeaderToFollowerRemoteProcedureCallArguments>());
    }
}