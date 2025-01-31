using Logic.Exceptions;
using Logic.Models.Server;
using Logic.Utils;

namespace Logic.Models.Client;

public interface IClientNode
{
  public int Id { get; }
  public Task ResponseFromServerAsync(bool committed, int? leaderId = null);
  public Task SendLogToClusterAsync(string log);
}