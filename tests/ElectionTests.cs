using Logic;
using FluentAssertions;
using NSubstitute;
namespace Tests;

public class ElectionTests
{
    /// <summary>
    /// Testing #4
    /// </summary>
    [Fact]
    public void WhenTheFollowerDoesNotGetAHeartbeatOrLogForTheirElectionTimeIntervalItStartsAnElection()
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
    /// Testing #13
    /// </summary>
    [Fact]
    public async Task GivenACandidateServerWhenItReceivesAHeartbeatFromTheSameTermItBecomesAFollowerAndLoses()
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
    /// Testing #18
    /// </summary>
    [Fact]
    public async Task GivenCandidateReceivesLogCandidateRejects()
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
}