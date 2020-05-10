using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    //Dictates whether the agent waits on each node
    [SerializeField]
    bool _patrolWaiting;

    //The total time we wait at each node
    [SerializeField]
    float _totalWaitTime = 3f;

    //This is where the functionality of Enemy AI will be: speed, aggressiveness, attention
    [Space]

    [SerializeField][Range(1,10)]
    float _enemySpeed = 3f;

    //Private variables for base behaviour
    NavMeshAgent _navMeshAgent;
    ConnectedWaypoint _currentWaypoint;
    ConnectedWaypoint _previousWaypoint;

    
    bool _travelling;
    bool _waiting;
    float _waitTimer;
    int _waypointsVisited;

    public void Start()
    {

        _navMeshAgent = this.GetComponent<NavMeshAgent>();

        if (_navMeshAgent == null)
        {
            Debug.LogError("The nav mesh agent component is not attached to " + gameObject.name);
        }
        else
        {
            if (_currentWaypoint == null)
            {
                //Set it as random
                GameObject[] allWaypoints = GameObject.FindGameObjectsWithTag("Waypoint");

                if (allWaypoints.Length > 0) {
                    while (_currentWaypoint == null) {
                        int random = UnityEngine.Random.Range(0, allWaypoints.Length);
                        ConnectedWaypoint startingWaypoint = allWaypoints[random].GetComponent<ConnectedWaypoint>();

                        if (startingWaypoint != null) {
                            _currentWaypoint = startingWaypoint;
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Insufficient patrol points for basic patrolling behaviour");
            }
        }

        SetDestination();
    }

    public void Update()
    {
        //Check if we're close to the destination

        if (_travelling && _navMeshAgent.remainingDistance <= 1.0f)
        {
            _travelling = false;
            _waypointsVisited++;


            //If we're going to wait, then wait.
            if (_patrolWaiting)
            {
                _waiting = true;
                _waitTimer = 0f;
            }
            else
            {
                SetDestination();
            }
        }

        //If we're waiting
        if (_waiting) {
            _waitTimer += Time.deltaTime;
            //Prevents the agent from clipping
            //Halt(_navMeshAgent);

            if (_waitTimer >= _totalWaitTime) {
                _waiting = false;
                Patrol(_navMeshAgent);

                SetDestination();
            }
        }

        //Starting the partolling behaviour
        Patrol(_navMeshAgent);
    }



    //Patrolling speed set to enemySpeed indicated in the inspector
    //Stopping distance set up for the agent who stops
    private void Patrol(NavMeshAgent navMeshAgent)
    {
        navMeshAgent.speed = _enemySpeed;

    }


    //Brings the NavMeshAgent to a halt
    // TO-DO: check if you need it
    private void Halt(NavMeshAgent navMeshAgent)
    {
        navMeshAgent.speed = 0f;
    }



    private void SetDestination()
    {
        if (_waypointsVisited > 0) {
            //To maintain where I've just been and where I'm going
            ConnectedWaypoint nextWaypoint = _currentWaypoint.NextWaypoint(_previousWaypoint);
            _previousWaypoint = _currentWaypoint;
            _currentWaypoint = nextWaypoint;
        }

        Vector3 targetVector = _currentWaypoint.transform.position;
        _navMeshAgent.SetDestination(targetVector);
        _travelling = true;
    }
}
