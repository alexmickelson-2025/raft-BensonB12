using Logic;
using NSubstitute;

namespace Tests;

public static class Utils
{
  public const int GENERAL_BUFFER_TIME = 15;

  public static void WaitForElectionTimerToRunOut()
  {
    Thread.Sleep(Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME + GENERAL_BUFFER_TIME);
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

  public static void ServersVoteForLeaderWhenAsked(IEnumerable<IServerNode> followerServers, IServerNode leaderServer)
  {
    foreach (IServerNode followerServer in followerServers)
    {
      followerServer
        .WhenForAnyArgs(server => server.TryToVoteForAsync(Arg.Any<int>(), Arg.Any<uint>()))
              .Do(async _ =>
              {
                await leaderServer.CountVoteAsync(true);
              });
    }
  }
}