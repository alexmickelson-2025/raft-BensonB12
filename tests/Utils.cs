using Logic.Models.Args;
using Logic.Models.Client;
using Logic.Models.Server;
using Logic.Utils;
using NSubstitute;

namespace Tests;

public static class Utils
{
  public const int GENERAL_BUFFER_TIME = 15;

  public static void WaitForElectionTimerToRunOut()
  {
    Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_INTERVAL + GENERAL_BUFFER_TIME);
  }

  public static void WaitForHeartbeatTimerToRunOut()
  {
    Thread.Sleep(Constants.HEARTBEAT_PAUSE + GENERAL_BUFFER_TIME);
  }

  public static IServerNode CreateIServerNodeSubstituteWithId(int id)
  {
    IServerNode server = Substitute.For<IServerNode>();
    server.Id.Returns(id);
    return server;
  }

  public static IClientNode CreateIClientNodeSubstituteWithId(int id)
  {
    IClientNode client = Substitute.For<IClientNode>();
    client.Id.Returns(id);
    return client;
  }

  public static void ServersVoteForLeaderWhenAsked(IEnumerable<IServerNode> followerServers, IServerNode leaderServer)
  {
    foreach (IServerNode followerServer in followerServers)
    {
      followerServer
        .WhenForAnyArgs(server => server.RPCFromCandidateAsync(Arg.Any<RPCFromCandidateArgs>()))
              .Do(async _ =>
              {
                await leaderServer.RPCFromFollowerAsync(new RPCFromFollowerArgs(followerServer.Id, followerServer.Term, true));
              });
    }
  }
}