# Raft

The superior consensus algorithm

## Test Cases

1. <br/>

- **Given** a leader is active
- **When** the leader sends a heartbeat
- **Then** it sends the heartbeat within 50ms

2. <br/>

- **Given** a node receives an AppendEntries from another node
- **When** the AppendEntries message is processed
- **Then** the node remembers that the other node is the current leader

3. <br/>

- **Given** a new node is initialized
- **When** the node is created
- **Then** it should be in follower state

4. <br/>

- **Given** a follower node does not receive any message for 300ms
- **When** the follower node waits
- **Then** it starts an election

5. <br/>

- **Given** the election time is reset
- **When** the election timeout is set
- **Then** it should be a random value between 150ms and 300ms

6. <br/>

- **Given** the election timeout is a random value
- **When** n calls are made to check election timeouts
- **Then** some of the timeout values should be different (other properties of the distribution can be asserted as well)

7. <br/>

- **Given** a new election begins
- **When** the election starts
- **Then** the term is incremented by 1

8. <br/>

- **Given** a new node is created
- **When** the node is initialized
- **Then** store the ID in a variable and wait 300ms, then reread the term and assert that it is greater by at least 1

9. <br/>

- **Given** a follower node receives an AppendEntries message
- **When** the AppendEntries message is received
- **Then** the election timer is reset, and no new election starts

10. <br/>

- **Given** an election begins
- **When** the candidate receives a majority of votes
- **Then** the candidate becomes a leader

11. <br/>

- **Given** an election begins with a candidate
- **When** the candidate receives a majority of votes while waiting for an unresponsive node
- **Then** the candidate becomes a leader

12. <br/>

- **Given** a follower has not voted and is in an earlier term
- **When** the follower receives a RequestForVoteRPC
- **Then** the follower responds with "yes"

13. <br/>

- **Given** a candidate server just became a candidate
- **When** the candidate votes for itself
- **Then** the candidate votes for itself

14. <br/>

- **Given** a candidate receives an AppendEntries message from a node with a later term
- **When** the candidate processes the AppendEntries message
- **Then** the candidate loses and becomes a follower

15. <br/>

- **Given** a candidate receives an AppendEntries message from a node with an equal term
- **When** the candidate processes the AppendEntries message
- **Then** the candidate loses and becomes a follower

16. <br/>

- **Given** a node receives a second request for vote for the same term
- **When** the node processes the request
- **Then** the node responds with "no"

17. <br/>

- **Given** a node receives a second request for vote for a future term
- **When** the node processes the request
- **Then** the node votes for that node

18. <br/>

- **Given** a candidate's election timer expires during an election
- **When** the timer expires
- **Then** a new election is started

19. <br/>

- **Given** a follower node receives an AppendEntries request
- **When** the request is processed
- **Then** the follower sends a response

20. <br/>

- **Given** a candidate receives an AppendEntries message from a previous term
- **When** the candidate processes the message
- **Then** the candidate rejects the message
