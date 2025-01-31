namespace Logic.Models.Client;

public interface IClientNode
{
  int Id { get; }
  Task ResponseFromServerAsync(bool committed, int? leaderId = null);
  Task SendLogToClusterAsync(string log);
}