﻿using System.Timers;

namespace logic;

public class ServerNode
{
  ServerNodeState _state;
  public ServerNodeState State => _state;
  int _term = 0;
  public int Term => _term;
  System.Timers.Timer _electionTimeOut;
  public int ElectionTimerInterval => (int)_electionTimeOut.Interval;

  public ServerNode()
  {
    _state = ServerNodeState.FOLLOWER;
    _electionTimeOut = newElectionTimer();
    _electionTimeOut.Elapsed += electionTimedOutProcedure;
  }

  void electionTimedOutProcedure(object? sender, ElapsedEventArgs e)
  {
    _state = ServerNodeState.CANDIDATE;
    _term++;
    _electionTimeOut = newElectionTimer();
  }

  System.Timers.Timer newElectionTimer()
  {
    int randomElectionTime = Random.Shared.Next(Constants.INCLUSIVE_MINIMUM_ELECTION_TIME, Constants.EXCLUSIVE_MAXIMUM_ELECTION_TIME);

    System.Timers.Timer tempTimer = new(randomElectionTime)
    {
      AutoReset = false
    };

    tempTimer.Start();

    return tempTimer;
  }
}
