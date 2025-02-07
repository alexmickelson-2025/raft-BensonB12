using FluentAssertions;
using Logic.Models.Args;
using Logic.Models.Client;
using Logic.Models.Server;
using Logic.Utils;
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
    leaderServer.InitializeClusterWithServers([followerServer]);

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
    int clientId = 1;

    IClientNode clientNode = Utils.CreateIClientNodeSubstituteWithId(clientId);
    IServerNode leaderServer = Utils.CreateIServerNodeSubstituteWithId(leaderId);
    ServerNode followerServer = new([leaderServer], clients: [clientNode]);

    // When
    await followerServer.RPCFromLeaderAsync(new RPCFromLeaderArgs(leaderId, 1, 0, 0, 0));
    await followerServer.RPCFromClientAsync(new RPCFromClientArgs(clientId, ""));

    // Then
    await clientNode.Received().ResponseFromServerAsync(false, leaderId);
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
      Thread.Sleep(50);
      // Do I do something here?
    }

    await candidateServer.RPCFromLeaderAsync(new RPCFromLeaderArgs(leaderId, candidateServer.Term + 1, -1, 0, -1));

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
    leaderServer.InitializeClusterWithServers([followerServer]);

    // When
    Utils.WaitForElectionTimerToRunOut();

    // Then
    followerServer.ReceivedWithAnyArgs().RPCFromLeaderAsync(Arg.Any<RPCFromLeaderArgs>());
  }
}