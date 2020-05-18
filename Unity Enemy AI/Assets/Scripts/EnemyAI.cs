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
    float _maxRayDistance = 15f;

    private static float timer = 5f;

    GameObject otherEnemyAI;

    public void Start()
    {

        //Remembering which Enemy AI entity is the "other one" for future references
        GameObject[] enemyAIs = GameObject.FindGameObjectsWithTag("Enemy AI");

        if (enemyAIs[0].name == this.gameObject.name)
        {
            otherEnemyAI = enemyAIs[1];
        }
        else
        {
            otherEnemyAI = enemyAIs[0];
        }


        //Working with NavMesh
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

        Debug.Log(timer);

        //If the enemy can see the other enemy, they halt for 5 seconds and continue walking
        if (canSee(otherEnemyAI))
        {
            talkToAnotherAI(otherEnemyAI);
        }

    }


    private IEnumerator resetTimer()
    {
        yield return new WaitForSeconds(10f);
        timer = 5f;
    }

    //Patrolling speed set to enemySpeed indicated in the inspector
    //Stopping distance set up for the agent who stops
    private void Patrol(NavMeshAgent navMeshAgent)
    {
        navMeshAgent.speed = _enemySpeed;
    }


    //Brings the NavMeshAgent to a halt
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


    private bool canSee(GameObject obj) {
        Ray ray = new Ray(transform.position, obj.transform.position);
        RaycastHit hit;
        Debug.DrawLine(transform.position, obj.transform.position, Color.red);
        if (Physics.Raycast(ray, out hit, _maxRayDistance) && (hit.collider.tag == "Enemy AI"))
        {
            //Debug.Log("You have hit " + obj.name);
            return true;
        }
        else
        {
            //Debug.Log("There was a collider in between");
            return false;
        }
    }

    private void talkToAnotherAI(GameObject anotherAI) {
        timer -= Time.deltaTime;
        //Check if timer hasn't run out yet. If it hasn't, stop to chat.
        if (timer > 0)
        {
            Halt(_navMeshAgent);
            Halt(anotherAI.GetComponent<NavMeshAgent>());
        }
        //If it has, continue patrolling
        else
        {
            Patrol(_navMeshAgent);
            Patrol(anotherAI.GetComponent<NavMeshAgent>());
        }
        //Reset the timer for the next time they see each other - with delay
        StartCoroutine(resetTimer());
    }

}
