namespace logic;

public static class GenerateKey
{
  public static int UniqueServerNodeId()
  {
    return Random.Shared.Next(int.MinValue, int.MaxValue);
  }
}