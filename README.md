# Raft

The superior consensus algorithm

## Test Cases

1. When a leader is active, it sends a heartbeat within 50ms.
2. When a node receives an AppendEntries from another node, then the first node remembers that the other node is the current leader.
3. When a new node is initialized, it should be in follower state.
4. When a follower doesn't get a message for 300ms, it starts an election.
5. When the election time is reset, it is a random value between 150 and 300ms.

- Between
- random times, call n times and make sure that there are some that are different (other properties of the distribution if you like).

6. When a new election begins, the term is incremented by 1.

- Create a new node, store id in a variable.
- Wait 300ms.
- Reread term (?).
- Assert after the term is greater (by at least 1).

7. When a follower does get an AppendEntries message, it resets the election timer (i.e., it doesn't start an election even after more than 300ms).
8. Given an election begins, when the candidate gets a majority of votes, it becomes a leader (think of the easy case; can use two tests for single and multi-node clusters).
9. Given a candidate receives a majority of votes while waiting for an unresponsive node, it still becomes a leader.
10. A follower that has not voted and is in an earlier term responds to a RequestForVoteRPC with yes. (The reply will be a separate RPC).
11. Given a candidate server that just became a candidate, it votes for itself.
12. Given a candidate, when it receives an AppendEntries message from a node with a later term, then the candidate loses and becomes a follower.
13. Given a candidate, when it receives an AppendEntries message from a node with an equal term, then the candidate loses and becomes a follower.
14. If a node receives a second request for a vote for the same term, it should respond no (again, separate RPC for response).
15. If a node receives a second request for a vote for a future term, it should vote for that node.
16. Given a candidate, when an election timer expires inside of an election, a new election is started.
17. When a follower node receives an AppendEntries request, it sends a response.
18. Given a candidate receives an AppendEntries from a previous term, then it rejects.
19. When a candidate wins an election, it immediately sends a heartbeat.
