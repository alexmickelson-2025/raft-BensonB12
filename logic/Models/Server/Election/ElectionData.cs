using Logic.Utils;

namespace Logic.Models.Server.Election;

public class ElectionData
{
  public System.Timers.Timer ElectionTimer { get; set; } = Util.NewElectionTimer();
  public bool ElectionCancellationFlag { get; set; } = false;

  public ElectionData(InitiateElection initiateElection)
  {
    NewElectionTimer(initiateElection);
  }

  public void NewElectionTimer(InitiateElection initiateElection, double? interval = null)
  {
    double randomInterval = interval ?? Random.Shared.Next(Constants.INCLUSIVE_MINIMUM_ELECTION_INTERVAL, Constants.EXCLUSIVE_MAXIMUM_ELECTION_INTERVAL);

    ElectionTimer = new(randomInterval)
    {
      AutoReset = false,
      Enabled = true
    };

    addElectionTimeOutProcedureEventToElectionTimer(initiateElection);
  }

  void addElectionTimeOutProcedureEventToElectionTimer(InitiateElection initiateElection)
  {
    ElectionTimer.Elapsed += async (sender, e) => await initiateElection(sender, e);
  }
}