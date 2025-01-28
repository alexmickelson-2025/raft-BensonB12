using FluentAssertions;
using NSubstitute;
using Logic;

namespace Tests;

public class VoteTests
{
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

    Utils.ServersVoteForLeaderWhenAsked([followerOne], leaderServer);
    leaderServer.AddServersToCluster([followerOne, followerTwo]);

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
    await server.TryToVoteForAsync(leaderId, server.Term + 1);

    // Then
    await leaderServer.Received().CountVoteAsync(true);
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
    await server.TryToVoteForAsync(candidateOneId, term);
    await server.TryToVoteForAsync(candidateTwoId, term);

    // Then
    await candidateServerTwo.Received().CountVoteAsync(false);
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
    await server.TryToVoteForAsync(candidateOneId, term);
    await server.TryToVoteForAsync(candidateTwoId, term + 1);

    // Then
    await candidateServerTwo.Received().CountVoteAsync(true);
  }
}