# First Test Cases Benson Created

1. <br/>

- Given a ServerNode just created
- When asked its state
- Then it returns that it is a Follower

2. <br/>

- Given a ServerNode just created
- When asked its election timeout status
- Then it returns a time it has been waiting for a leader's update

3. <br/>

- Given a ServerNode is a follower
- When there is no update from a leader
- Then the election timeout status decreases/continues accordingly

4. <br/>

- Given a ServerNode is a follower, the serverNode's election timeout status is 0
- When it checks its election timeout status
- Then it becomes a candidate

5. <br/>

- Given a ServerNode
- When asked who it votes for
- Then it returns true

6. <br/>

- Given a ServerNode that has set its own flag to false
- When asked who it votes for
- Then it returns false

7. <br/>

- Given a ServerNode that is alone
- When asked for a total vote
- Then it returns true for its majority

8. <br/>

- Given a ServerNode that is alone and has a flag set to false
- When asked for a total vote
- Then it returns false for its majority

9. <br/>

- Given a group of ServerNodes
- When one sends out a vote request
- Then all other nodes receive a vote request

10. <br/>

- Given a ServerNode is a follower, the ServerNode's election timeout status is 0
- When it checks its election timeout status, turns into a candidate
- Then it sends out vote requests to all other ServerNodes

11. <br/>

- Given a group of ServerNodes
- When one sends out a vote request
- Then all other nodes respond with a true vote request

12. <br/>

- Given a group of ServerNodes
- When one sends out a vote request, they all respond true
- The node that sent out the vote request becomes a leader

13. <br/>

- Given a ServerNode is a follower, there is a leader ServerNode
- When the leader gives a follower an update
- Then the follower's election timeout status increases/restarts accordingly

14. <br/>

- Given you have a serverNode that is down
- When you ask for a vote
- It does not respond

15. <br/>

- Given you have a ServerNode that is a newly turned candidate, two other servers up, two down
- When it requests a vote from all other ServerNodes
- It got 3 votes for it, and turned into a leader

16. <br/>

- Given you have a ServerNode that is a newly turned candidate, three other servers are down
- When it asks for votes
- It does not turn into a leader

17. <br/>

- Given You have Two candidates that were created at the same time
- When they ask for votes
- Then it is a split vote

18. <br/>

- Given you have a candidate
- When it asks for all votes
- Then the election timer is still counting down

19. <br/>

- Given that you have a ServerNode
- When asked what term it is
- Then it returns the right term, such as 1

20. <br/>

- Given that you have a ServerNode that just voted in term 1
- When asked to vote for someone else in term 1
- Then the server does not vote again

21. <br/>

- Given that you have a ServerNode that just voted in term 1
- When it dies, turns back on, and then asked to vote in term 1 again
- Then the server does not vote again

22. <br/>

- Given when a ServerNode is on term 1, already voted in term 1
- When it is asked to vote for term 2
- It votes again

### Other Considerations

- What does it look like to 'shut down' in my code?
- How will I test something that should never end because it is always waiting?
- Do nodes return false if they are asked to vote again, or do they just not respond? I need to alter some tests if that is the case (Yes they do)
- The request and data return needs to be async
- How will I test time passing in my tests? Is there a way to 'mock' it instead of actually waiting?
