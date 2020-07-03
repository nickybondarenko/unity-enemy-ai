using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/*
 * COPYRIGHT: Waypoint and patrolling logic by Table Flip Games. 
 * Modifications by Bondarenko: expanding the code, adding functionality like patrol agents go to a halt,
 * approach each other, reach on each other and other elements in the environment. 
 * ADD COPYRIGHT
*/

public class EnemyAI : MonoBehaviour
{
    //The total time we wait at each node
    [SerializeField]
    float _totalWaitTime = 3f;

    //This is where the functionality of Enemy AI will be: speed, aggressiveness, attention
    [Space]

    [Header("Movement: AI speed")]
    [Tooltip("Sets the AI patrolling/walking speed")]
    [SerializeField][Range(1,10)]
    float _enemySpeed = 3f;

    [Header("Behaviour: general settings")]
    [Tooltip("View angle for AI's perception")]
    [SerializeField]
    float _viewAngle = 45f;
    [Tooltip("View radius for AI's perception")]
    [SerializeField]
    float _viewRadius = 3f;



    [Header("Behaviour: other AI perception")]
    [Tooltip("Determines whether this AI entity will force conversations with the other entity")]
    [SerializeField]
    bool _isTalkative;
    [SerializeField]
    [Tooltip("Length of chat in seconds")]
    float _chatLength;

    [Header("Behaviour: interaction with other objects")]
    [SerializeField]
    bool _interactsWithWindows;
    [SerializeField]
    float _interactionLength;
  

    //Private variables for base behaviour
    NavMeshAgent _navMeshAgent;
    ConnectedWaypoint _currentWaypoint;
    ConnectedWaypoint _previousWaypoint;

    
    bool _travelling;
    bool _waiting;
    float _waitTimer;
    int _waypointsVisited;


    private float timer;
    private float interactionTimer;
    private GameObject objectOfInterest;

    private GameObject otherEnemyAI;

    private GameObject interactableObject;

    public void Start()
    {
        //Set up the timers
        timer = _chatLength;
        interactionTimer = _interactionLength;
        //Remembering which Enemy AI entity is the "other one" for future references. ONLY WORKS FOR 2 ENEMY AIS
        // GameObject[] enemyAIs = GameObject.FindGameObjectsWithTag("EnemyAI");

        // if (enemyAIs[0].name == this.gameObject.name)
        // {
        //     otherEnemyAI = enemyAIs[1];
        // }
        // else
        // {
        //     otherEnemyAI = enemyAIs[0];
        // }




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
        //Set the first destination
        SetDestination();
    }

    public void Update()
    {
        //Check if we're close to the destination: waypoint. If we are, reset the destination
        if (_travelling && _navMeshAgent.remainingDistance <= 1.0f)
        {
            _travelling = false;
            _waypointsVisited++;
          
            SetDestination();
            
        }

        // //If I can see another enemy, check if I'm talkative
        if (CanSee(otherEnemyAI))
        {
            if (_isTalkative)
            {
                //If I am, talk to the other AI
                TalkToAnotherAI();
            }
        }

        if (CanSee(interactableObject))
        {
            // Debug.Log("I can see an interactable object");

            if (interactableObject.CompareTag("Window") && _interactsWithWindows)
            {
                Debug.Log("I can see and interact with windows");
                objectOfInterest = interactableObject;
                checkObjectOut();
            }

        }
        //Debug.Log(timer);
        Debug.Log(interactionTimer);


    }


    private void OnTriggerEnter(Collider other) {
        // if (this.name == "EnemyAlice"){
        // Debug.Log("I can see something! It's " + other.gameObject.name);
        // }
        //Check if the gameObject triggering is another AI
        if (other.gameObject.tag == "EnemyAI" && other.gameObject != this)
        {
            otherEnemyAI = other.gameObject;
            // Debug.Log("Other enemy AI set correctly");
        }
        else 
        {
            interactableObject = other.gameObject;
            // Debug.Log("Interactable object set to " + other.gameObject.name);
        }
    }

    private IEnumerator ResetTimer()
    {
        yield return new WaitForSeconds(20f);
        timer = _chatLength;
        interactionTimer = _interactionLength;
    }


    //Patrolling speed set to enemySpeed indicated in the inspector
    private void Patrol(NavMeshAgent navMeshAgent)
    {
        navMeshAgent.speed = _enemySpeed;
    }


    //Brings the NavMeshAgent to a halt
    private void Halt(NavMeshAgent navMeshAgent)
    {
        navMeshAgent.speed = 0f;
    }

    //Go to the next waypoint
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

    //Checking if the current agent can see another object (another AI, or any other game object)
    private bool CanSee(GameObject obj)
    {
        Ray ray = new Ray(transform.position, obj.transform.position);
        RaycastHit hit;

        float angleToObject = Vector3.Angle(this.transform.position, obj.transform.position);
        //Debug.DrawLine(this.transform.position, obj.transform.position, Color.blue);


        if (_viewAngle > angleToObject)
        {
            //Debug.Log("In the view range");
            //If there is an object in the radius and it's in the view
                if (Physics.Raycast(ray, out hit, _viewRadius) && hit.collider.CompareTag(obj.tag))
                {
                   return true;
                }
                else
                {
                    return false;
                }
        }
        else
        {
            //Debug.Log("Not in the view range");
            return false;
        }
    }


    //If the agent sees another agent, they will approach them and strike up a conversation
    //There's a timer reset to prevent the agents from chatting eternally
    private void TalkToAnotherAI()
    {

        Debug.Log("I see " + otherEnemyAI.gameObject.name);
        timer -= Time.deltaTime;
        //Check if timer hasn't run out yet. If it hasn't, approach the other agent
        //Stop the other agent from moving
        if (timer > 1)
        { 
            Halt(otherEnemyAI.GetComponent<NavMeshAgent>());
            Debug.Log(otherEnemyAI.gameObject.name + " is stopped");
            ComeOverTo(otherEnemyAI);
            Debug.Log("Approaching it");

        }
        //If the timer ran out, continue patrolling
        if (timer <= 1)
        {
            SetDestination();

            Debug.Log("destination was reset for " + this.gameObject.name);
            Debug.Log(this.gameObject.name + " goes to " + _currentWaypoint.name);

            Patrol(_navMeshAgent);
            Patrol(otherEnemyAI.GetComponent<NavMeshAgent>());

            //Reset the timer for the next time they see each other - with delay
            StartCoroutine(ResetTimer());
        }
    }

     private void checkObjectOut(){
        interactionTimer -= Time.deltaTime;
        if (interactionTimer > 1)
        {
            ComeOverTo(objectOfInterest);
            Debug.Log("Approaching " + objectOfInterest.name);
        }
        
        if (interactionTimer <= 1)
        {
            Debug.Log("Continuing patrol");
            SetDestination();
            Patrol(_navMeshAgent);
            StartCoroutine(ResetTimer());
        }
    }

    //Approaching another game object
    private void ComeOverTo(GameObject obj)
    {
        Vector3 targetVector = obj.transform.position;
        _navMeshAgent.SetDestination(targetVector);
        if (_navMeshAgent.remainingDistance <= 2f)
        {
            Halt(_navMeshAgent);
        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, maxRadius);

        Vector3 fovLine1 = Quaternion.AngleAxis(_viewAngle, transform.up) * transform.forward * _viewRadius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-_viewAngle, transform.up) * transform.forward * _viewRadius;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);

        Gizmos.color = Color.red;
//        Gizmos.DrawRay(transform.position, (otherEnemyAI.transform.position - transform.position).normalized * maxRadius);

        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, transform.forward * _viewRadius);

    }


}
