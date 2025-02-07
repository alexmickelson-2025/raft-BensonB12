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

  //   public async Task RequestAppendEntries(AppendEntriesData request)
  //   {
  //     try
  //     {
  //       await client.PostAsJsonAsync(Url + "/request/appendEntries", request);
  //     }
  //     catch (HttpRequestException)
  //     {
  //       Console.WriteLine($"node {Url} is down");
  //     }
  //   }

  //   public async Task RequestVote(VoteRequestData request)
  //   {
  //     try
  //     {
  //       await client.PostAsJsonAsync(Url + "/request/vote", request);
  //     }
  //     catch (HttpRequestException)
  //     {
  //       Console.WriteLine($"node {Url} is down");
  //     }
  //   }

  //   public async Task RespondAppendEntries(RespondEntriesData response)
  //   {
  //     try
  //     {
  //       await client.PostAsJsonAsync(Url + "/response/appendEntries", response);
  //     }
  //     catch (HttpRequestException)
  //     {
  //       Console.WriteLine($"node {Url} is down");
  //     }
  //   }

  //   public async Task ResponseVote(VoteResponseData response)
  //   {
  //     try
  //     {
  //       await client.PostAsJsonAsync(Url + "/response/vote", response);
  //     }
  //     catch (HttpRequestException)
  //     {
  //       Console.WriteLine($"node {Url} is down");
  //     }
  //   }

  //   public async Task SendCommand(ClientCommandData data)
  //   {
  //     await client.PostAsJsonAsync(Url + "/request/command", data);
  //   }

  public ServerNodeState State => throw new NotImplementedException();

  public uint Term => throw new NotImplementedException();

  public void InitializeClusterWithServers(IEnumerable<IServerNode> otherServers)
  {
    throw new NotImplementedException();
  }

  public Task RPCFromCandidateAsync(RPCFromCandidateArgs args)
  {
    throw new NotImplementedException();
  }

  public Task RPCFromClientAsync(RPCFromClientArgs args)
  {
    throw new NotImplementedException();
  }

  public Task RPCFromFollowerAsync(RPCFromFollowerArgs args)
  {
    throw new NotImplementedException();
  }

  public Task RPCFromLeaderAsync(RPCFromLeaderArgs args)
  {
    throw new NotImplementedException();
  }
}