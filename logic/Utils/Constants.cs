
using Logic.Models.Client;

namespace Logic.Utils;

public static class Constants
{
  public const int INCLUSIVE_MINIMUM_ELECTION_INTERVAL = 150;
  public const int EXCLUSIVE_MAXIMUM_ELECTION_INTERVAL = 300;
  public const int HEARTBEAT_PAUSE = 50;
  public const int CLUSTER_WAITS_FOR_RESPONSE_INTERVAL = 75;
}