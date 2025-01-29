namespace Logic.Utils;

public static class Util
{
  public static int GenerateUniqueServerNodeId()
  {
    return Random.Shared.Next(int.MinValue, int.MaxValue);
  }

  public static System.Timers.Timer NewElectionTimer()
  {
    int randomElectionTime = Random.Shared.Next(Constants.INCLUSIVE_MINIMUM_ELECTION_TIME, Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME);

    return new(randomElectionTime)
    {
      AutoReset = false,
      Enabled = true
    };
  }
}