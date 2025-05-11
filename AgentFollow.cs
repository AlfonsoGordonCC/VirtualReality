using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentFollow : MonoBehaviour
{
    public Transform agentToFollow;
    NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = agentToFollow.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance < 0.2f)
            agent.destination = agentToFollow.position;
    }
}
