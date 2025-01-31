using System.Threading.Tasks;
using Logic.Models.Args;
using Logic.Models.Client;
using Logic.Models.Server;
using Logic.Utils;
using NSubstitute;
namespace Tests.AppendingLogs;

public class ClientTests
{
  /// <summary>
  /// Testing Logs #1
  /// </summary>
  [Fact]
  public async Task WhenALeaderReceivesAClientCommandTheLeaderSendsTheLogEntryInTheNextAppendEntryToAllNodes()
  {
    // Given
    string log = "log";

    IServerNode followerServer = Utils.CreateIServerNodeSubstituteWithId(1);
    ServerNode leaderServer = new();

    Utils.ServersVoteForLeaderWhenAsked([followerServer], leaderServer);
    leaderServer.InitializeClusterWithServers([followerServer]);

    // When
    Utils.WaitForElectionTimerToRunOut();
    await leaderServer.RPCFromClientAsync(new RPCFromClientArgs(0, log));
    Utils.WaitForHeartbeatTimerToRunOut();

    // Then
    await followerServer.Received().RPCFromLeaderAsync(Arg.Is<RPCFromLeaderArgs>(args => args.PreviousLogIndex == 0));
  }

  /// <summary>
  /// TestingLogs #2
  /// </summary>
  [Fact]
  public async Task WhenALeaderReceivesACommandFromTheClientItIsAppendedToItsLog()
  {
    // Given
    string log = "log";

    ServerNode leaderServer = new();
    IServerNode followerServer = Utils.CreateIServerNodeSubstituteWithId(1);

    Utils.ServersVoteForLeaderWhenAsked([followerServer], leaderServer);
    leaderServer.InitializeClusterWithServers([followerServer]);

    // When
    Utils.WaitForElectionTimerToRunOut();
    await leaderServer.RPCFromClientAsync(new RPCFromClientArgs(0, log)); ;
    Utils.WaitForHeartbeatTimerToRunOut();

    // Then
    await followerServer.Received().RPCFromLeaderAsync(Arg.Is<RPCFromLeaderArgs>(args => args.PreviousLogIndex == 0));
  }

  /// <summary>
  /// Testing Logs #12
  /// </summary>
  [Fact]
  public void WhenALeaderReceivesAMajorityResponsesAfterALogReplicationHeartbeatTheLeaderSendsAConfirmationToClient()
  {
    // Given

    // When

    // Then
  }

  /// <summary>
  /// Testing Logs #18
  /// </summary>
  [Fact]
  public async Task IfALeaderCannotCommitAnEntryIDoesNotSendAResponseToTheClient()
  {
    // Given
    int clientId = 0;

    IServerNode followerOne = Utils.CreateIServerNodeSubstituteWithId(1);
    IServerNode followerTwo = Utils.CreateIServerNodeSubstituteWithId(2);
    IClientNode client = Utils.CreateIClientNodeSubstituteWithId(clientId);
    ServerNode leaderServer = new(clients: [client]);

    Utils.ServersVoteForLeaderWhenAsked([followerOne, followerTwo], leaderServer);
    leaderServer.InitializeClusterWithServers([followerOne, followerTwo]);

    // When
    Utils.WaitForElectionTimerToRunOut();
    await leaderServer.RPCFromClientAsync(new RPCFromClientArgs(clientId, "log"));


    // Then
    await client.DidNotReceiveWithAnyArgs().ResponseFromServerAsync(Arg.Any<bool>(), Arg.Any<int>());
  }
}