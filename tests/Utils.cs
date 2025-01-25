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

  public static IServerNode CreateIServerNodeSubstituteWithId(int id)
  {
    IServerNode server = Substitute.For<IServerNode>();
    server.Id.Returns(id);
    return server;
  }

}