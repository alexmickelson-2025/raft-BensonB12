
using Logic.Models.Client;

namespace Logic.Utils;

public static class Constants
{
  public const int INCLUSIVE_MINIMUM_ELECTION_INTERVAL = 1500;
  public const int EXCLUSIVE_MAXIMUM_ELECTION_INTERVAL = 3000;
  public const int HEARTBEAT_PAUSE = 500;
  public const int CLUSTER_WAITS_FOR_RESPONSE_INTERVAL = 750;
}