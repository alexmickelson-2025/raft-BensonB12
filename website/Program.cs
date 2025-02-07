using System.Text.Json;
using Logic.Models.Args;
using Logic.Models.Server;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace Website;

public class Program
{
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.UseUrls("http://0.0.0.0:8080");

    var nodeId = Environment.GetEnvironmentVariable("NODE_ID") ?? throw new Exception("NODE_ID environment variable not set");
    var otherNodesRaw = Environment.GetEnvironmentVariable("OTHER_NODES") ?? throw new Exception("OTHER_NODES environment variable not set");
    var nodeIntervalScalarRaw = Environment.GetEnvironmentVariable("NODE_INTERVAL_SCALAR") ?? throw new Exception("NODE_INTERVAL_SCALAR environment variable not set");
    int nodeIdInt = 0;

    try
    {
      nodeIdInt = int.Parse(nodeId);
    }
    catch
    {
      throw new Exception("NODE_ID was not an int");
    }

    builder.Services.AddLogging();
    var serviceName = "Node" + nodeId;
    builder.Logging.AddOpenTelemetry(options =>
    {
      options
        .SetResourceBuilder(
            ResourceBuilder
              .CreateDefault()
              .AddService(serviceName)
        )
        .AddOtlpExporter(options =>
        {
          options.Endpoint = new Uri("http://dashboard:18889");
        });
    });
    var app = builder.Build();

    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Node ID {name}", nodeId);
    logger.LogInformation("Other nodes environment config: {}", otherNodesRaw);

    IServerNode[] otherNodes = otherNodesRaw
      .Split(";")
      .Select(s => new HttpServer(int.Parse(s.Split(",")[0]), s.Split(",")[1]))
      .ToArray();

    logger.LogInformation("other nodes {nodes}", JsonSerializer.Serialize(otherNodes));

    ServerNode node = new(otherServers: otherNodes, id: nodeIdInt); // could add a logger here

    // ServerNode.NodeIntervalScalar = double.Parse(nodeIntervalScalarRaw);

    // node.RunElectionLoop();

    app.MapGet("/health", () => "healthy");

    app.MapGet("/info", () =>
        Results.Json(new ServerInfo() { Id = node.Id, Term = node.Term, ServerStateId = (int)node.State })
      );

    // app.MapGet("/nodeData", () =>
    // {
    //   return new NodeData(
    //     Id: node.Id,
    //     Status: node.Status,
    //     ElectionTimeout: node.ElectionTimeout,
    //     Term: node.CurrentTerm,
    //     CurrentTermLeader: node.CurrentTermLeader,
    //     CommittedEntryIndex: node.CommittedEntryIndex,
    //     Log: node.Log,
    //     State: node.State,
    //     NodeIntervalScalar: RaftNode.NodeIntervalScalar
    //   );
    // });

    app.MapPost("/from/leader", async (RPCFromLeaderArgs request) =>
    {
      logger.LogInformation("received request from leader {request}", request);
      await node.RPCFromLeaderAsync(request);
    });

    app.MapPost("/from/follower", async (RPCFromFollowerArgs response) =>
    {
      logger.LogInformation("received response from follower {request}", response);
      await node.RPCFromFollowerAsync(response);
    });

    app.MapPost("/from/candidate", async (RPCFromCandidateArgs request) =>
    {
      logger.LogInformation("received request from candidate {request}", request);
      await node.RPCFromCandidateAsync(request);
    });

    app.MapPost("/from/client", async (RPCFromClientArgs command) =>
    {
      logger.LogInformation("received command from client {request}", command);
      await node.RPCFromClientAsync(command);
    });

    app.Run();
  }
}