using FluentAssertions;
using Logic;
using NSubstitute;

namespace Tests;

public class HeartbeatTests
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

    Utils.ServersVoteForLeaderWhenAsked([followerServer], leaderServer);
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
  /// Testing #12
  /// </summary>
  [Fact]
  public async Task GivenACandidateServerWhenItReceivesAHeartbeatFromALaterTermItBecomesAFollowerAndLoses()
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
  /// Testing #19
  /// </summary>
  [Fact]
  public void WhenACandidateWinsAnElectionItImmediatelySendsAHeartBeat()
  {
    // Given
    ServerNode leaderServer = new();
    IServerNode followerServer = Utils.CreateIServerNodeSubstituteWithId(1);

    Utils.ServersVoteForLeaderWhenAsked([followerServer], leaderServer);

    leaderServer.AddServersToServersCluster([followerServer]);

    // When
    Utils.WaitForElectionTimerToRunOut();

    // Then
    followerServer.Received().ReceiveLeaderToFollowerRemoteProcedureCallAsync(Arg.Any<LeaderToFollowerRemoteProcedureCallArguments>());
  }
}