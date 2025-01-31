namespace Logic.Utils;

public static class Util
{
  public static int GenerateId()
  {
    return Random.Shared.Next(int.MinValue, int.MaxValue);
  }

  public static System.Timers.Timer NewElectionTimer(double? interval = null)
  {
    double randomInterval = interval ?? Random.Shared.Next(Constants.INCLUSIVE_MINIMUM_ELECTION_INTERVAL, Constants.EXCLUSIVE_MAXIMUM_ELECTION_INTERVAL);

    return new(randomInterval)
    {
      AutoReset = false,
      Enabled = true
    };
  }
}