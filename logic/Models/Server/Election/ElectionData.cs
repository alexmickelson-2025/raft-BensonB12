using Logic.Utils;

namespace Logic.Models.Server.Election;

public class ElectionData
{
  public System.Timers.Timer ElectionTimer { get; set; } = Util.NewElectionTimer();
  public bool ElectionCancellationFlag { get; set; } = false;
}