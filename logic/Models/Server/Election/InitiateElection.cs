using System.Timers;

namespace Logic.Models.Server.Election;

public delegate Task InitiateElection(object? obj, ElapsedEventArgs elapsedEventArgs);