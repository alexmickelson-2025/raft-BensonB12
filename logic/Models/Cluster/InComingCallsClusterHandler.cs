using Logic.Models.Server;

namespace Logic.Models.Cluster;

public class IncomingCallsClusterHandler
{
  ClusterData _clusterData;
  public IncomingCallsClusterHandler(ClusterData clusterData)
  {
    _clusterData = clusterData;
  }

  public async Task RegisterVoteForAsync(int id, uint term)
  {
    if (_clusterData.ServerData.ServerIsDown())
    {
      return;
    }

    if (_clusterData.ServerData.CannotVoteInTerm(term))
    {
      await registerInFavorVoteToCandidateAsync(false, id, term);
    }
    else
    {
      await registerInFavorVoteToCandidateAsync(true, id, term);
    }
  }

  async Task registerInFavorVoteToCandidateAsync(bool inFavor, int id, uint newTerm)
  {
    IServerNode candidate = _clusterData.GetServer(id);

    await candidate.CountVoteAsync(inFavor);
    _clusterData.ServerData.HasVotedInTerm[newTerm] = true;
  }

  public async Task CountVoteAsync(bool inFavor) // Does not care who sent the vote, the servers are restricted to only vote once per term. Maybe I should take the term then?
  {
    if (_clusterData.ServerData.ServerIsDown())
    {
      return;
    }

    if (inFavor)
    {
      _clusterData.VotesInFavorForServer++;
    }
    else
    {
      _clusterData.VotesNotInFavorForServer++;
    }

    await Task.CompletedTask;
  }

  public async Task RPCFromFollowerAsync(int id, bool rejected)
  {
    await Task.CompletedTask;
  }
}