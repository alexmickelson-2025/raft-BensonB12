# 1/13/2025

## Questions (Answers)

- There will be a true or false response when asking for the vote
- There can also be a follow up call separate from the response to the candidate node
- Heartbeats should be less then the election time outs (20 milliseconds)
- Election time outs are longer than heartbeats (150 - 300 milliseconds)

```
blahName()
{
  blah(node in otherNodes)
  {
    node.RequestAppendEntries();
    // node is an interface (ISeverNode)
    // (To Test) Verified that it was called with nothing in it
  }
}
```

- Knowing a difference between an algorithm need vs a testing need
- Getting a state is a testing need
- From a follower perspective
  - Voted for node 1
  - If you receive a heartbeat from node 1, then it became a leader
- We want the tests to reflect the algorithm, not the implantation of the algorithm
- If the candidate knows that it failed the election, it waits for the time out to try again
- Do not test the infinite candidate looping

# Docker

### Rest CLient project

n = new Node();

What steps need to happen to set up your cluster

- Two other nodes
- timers start
- Turn on endpoints
- /request/append
- /request/vote
- /response/append
- /response/vote
- /command
