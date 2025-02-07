using Logic.Models.Args;
using Logic.Models.Server;

namespace Website;


public class HttpServer : IServerNode
{
  public int Id { get; }
  public string Url { get; }
  private HttpClient client = new();

  public HttpServer(int id, string url)
  {
    Id = id;
    Url = url;
  }

  public void InitializeClusterWithServers(IEnumerable<IServerNode> otherServers)
  {
    throw new NotImplementedException();
  }

  public async Task RPCFromCandidateAsync(RPCFromCandidateArgs args)
  {
    try
    {
      await client.PostAsJsonAsync(Url + "/from/candidate", args);
    }
    catch (HttpRequestException)
    {
      Console.WriteLine($"node {Url} is down");
    }
  }

  public async Task RPCFromClientAsync(RPCFromClientArgs args)
  {
    try
    {
      await client.PostAsJsonAsync(Url + "/from/client", args);
    }
    catch (HttpRequestException)
    {
      Console.WriteLine($"node {Url} is down");
    }
  }

  public async Task RPCFromFollowerAsync(RPCFromFollowerArgs args)
  {
    try
    {
      await client.PostAsJsonAsync(Url + "/from/follower", args);
    }
    catch (HttpRequestException)
    {
      Console.WriteLine($"node {Url} is down");
    }
  }

  public async Task RPCFromLeaderAsync(RPCFromLeaderArgs args)
  {
    try
    {
      await client.PostAsJsonAsync(Url + "/from/leader", args);
    }
    catch (HttpRequestException)
    {
      Console.WriteLine($"node {Url} is down");
    }
  }
}