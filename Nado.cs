using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Nado : MonoBehaviour
{
    public Transform[] posiciones;
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;
    }

    void GoToNextPosition()
    {
        agent.destination = posiciones[UnityEngine.Random.Range(0, posiciones.Length)].position;

    }

    // Update is called once per frame
    void Update()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.Log(agent.navMeshOwner.name);
            return;
        }
        

        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            GoToNextPosition();
        }
    }
}
