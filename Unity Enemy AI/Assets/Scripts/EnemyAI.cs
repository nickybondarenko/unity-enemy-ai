/*
 * Copyright 2020
 * Waypoint and patrolling logic by Table Flip Games. 
 * Modifications by Veronika Bondarenko to waypoints and patrolling logic:
 * expanding the code, adding functionality like patrol agents go to a halt,
 * approach each other, reach on each other and other elements in the environment. 
 * Timer, field of view, interaction, perception, behaviours and all the other 
 * logic by Veronika Bondarenko.
 * This file is a part of AI in games Bachelor thesis project
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // This is where the functionality of Enemy AI will be: speed, attention and other behaviours
    [Space]

    [Header("Movement: AI speed")]
    [Tooltip("Sets the AI patrolling/walking speed")]
    [SerializeField][Range(1,10)]
    float _enemySpeed = 3f;
    [SerializeField][Range(1,15)]
    float _maxSpeed = 7f;

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
    [Tooltip("Adjust to create a unique AI entity")]
    [SerializeField]
    bool _interactsWithWindows;
    [SerializeField]
    bool _interactsWithPainting;
    [SerializeField]
    bool _interactsWithShelf;
    [SerializeField]
    [Tooltip("For how long AI interacts with a game object in the scene, in seconds")]
    float _interactionLength;

    // Private variables for base behaviour
    NavMeshAgent _navMeshAgent;
    ConnectedWaypoint _currentWaypoint;
    ConnectedWaypoint _previousWaypoint;
    private GameObject objectOfInterest;
    private GameObject otherEnemyAI;
    private GameObject interactableObject;
    private GameObject playerSimAI;
    bool _travelling;
    bool _waiting;
    float _waitTimer;
    int _waypointsVisited;

    // Timers
    private float timer;
    private float interactionTimer;

    private void Awake() 
    {
        // Setup timers
        timer = _chatLength;
        interactionTimer = _interactionLength;
        // Set initial player AI simulator. For this setting, these should only be one player AI simulator
        // If more playerSimAI need to be taking into consideration, playerSimAI variable shound be assigned in
        // OnTriggerEnter method (as were otherEnemyAI and interactableObject)
        playerSimAI = GameObject.FindGameObjectWithTag("OtherAI");
        // Set initial reference to the other Enemy AI entity. This variable will be updated
        // if another Enemy AI entity enters the Trigger Collider zone
        otherEnemyAI = GameObject.FindGameObjectWithTag("EnemyAI");
    }

    public void Start()
    {
        // The waypoint logic. Based on the code by Table Flip Games (See Copyright above)
        _navMeshAgent = this.GetComponent<NavMeshAgent>();

        if (_navMeshAgent == null)
        {
            Debug.LogError("The nav mesh agent component is not attached to " + gameObject.name);
        }
        else
        {
            if (_currentWaypoint == null)
            {
                // If the next waypoint hasn't been set yet, set it as a random waypoint from the array of waypoints
                GameObject[] allWaypoints = GameObject.FindGameObjectsWithTag("Waypoint");
                if (allWaypoints.Length > 0) 
                {
                    while (_currentWaypoint == null) 
                    {
                        int random = UnityEngine.Random.Range(0, allWaypoints.Length);
                        ConnectedWaypoint startingWaypoint = allWaypoints[random].GetComponent<ConnectedWaypoint>();

                        if (startingWaypoint != null)
                        {
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
        // Set the first destination
        SetDestination();
    }

    public void Update()
    {
        // Check if we're close to the waypoint. If we are, reset the destination to a new waypoint
        if (_travelling && _navMeshAgent.remainingDistance <= 1.0f)
        {
            _travelling = false;
            _waypointsVisited++;
          
            SetDestination();
        }

        // If active AI can see another enemy, check if its talkative
        if (CanSee(otherEnemyAI))
        {
            if (_isTalkative)
            {
                // If it is, talk to the other AI
                TalkToAnotherAI();
            }
        }



        // If the gameObject AI sees is player simulator
        if (CanSee(playerSimAI))
        {
            _travelling = false;
            Hunt(playerSimAI);

            // Respawning player simulator to continue the simulation
            GameObject respawn = GameObject.FindGameObjectWithTag("Respawn");
            playerSimAI.transform.position = respawn.transform.position;
            playerSimAI.SetActive(true);
        }


        // If the gameObject AI sees is interactable
        if (CanSee(interactableObject))
        {
            objectOfInterest = interactableObject;

            // These statements are placeholders for custom animation or any other types of custom interaction with
            // separate objects
            if (objectOfInterest.CompareTag("Window") && _interactsWithWindows)
            {
                checkObjectOut();
            }
            if (objectOfInterest.CompareTag("Painting") && _interactsWithPainting)
            {
                checkObjectOut();
            }

            if (objectOfInterest.CompareTag("MedalsShelf") && _interactsWithShelf)
            {
                checkObjectOut();
            }

        }

        Debug.Log(timer);
        Debug.Log(interactionTimer);

    }


    private void OnTriggerEnter(Collider other) {
        // Check if the gameObject entering the trigger is another AI
        // If it isn't, save the entity for future reference anyway
        if (other.gameObject.tag == "EnemyAI" && other.gameObject != this)
        {
            otherEnemyAI = other.gameObject;
        }
        else
        {
            interactableObject = other.gameObject;
        }
    }

    // Takes care of the timer and resetting it with 30 seconds delay
    private IEnumerator ResetTimer()
    {
        yield return new WaitForSeconds(30f);
        timer = _chatLength;
        interactionTimer = _interactionLength;
    }


    // Patrolling speed set to enemySpeed indicated in the inspector
    private void Patrol(NavMeshAgent navMeshAgent)
    {
        navMeshAgent.speed = _enemySpeed;
    }


    // Brings the NavMeshAgent to a halt by setting speed to 0
    private void Halt(NavMeshAgent navMeshAgent)
    {
        navMeshAgent.speed = 0f;
    }

    // Going to the next waypoint
    private void SetDestination()
    {
        if (_waypointsVisited > 0) {
            // To maintain where AI has just been and where its going
            ConnectedWaypoint nextWaypoint = _currentWaypoint.NextWaypoint(_previousWaypoint);
            _previousWaypoint = _currentWaypoint;
            _currentWaypoint = nextWaypoint;
        }

        Vector3 targetVector = _currentWaypoint.transform.position;
        _navMeshAgent.SetDestination(targetVector);
        _travelling = true;
    }

    // Checking if the current agent can see another object (another AI, or any other game object)
    private bool CanSee(GameObject obj)
    {
        Ray ray = new Ray(transform.position, obj.transform.position);
        RaycastHit hit;
        float angleToObject = Vector3.Angle(this.transform.position, obj.transform.position);
        if (_viewAngle > angleToObject)
        {
            // If there is an object in the radius and the tag is what we are looking for
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
            return false;
        }
    }


    // If the agent sees another agent, they will approach them and "strike up a conversation"
    // There's a timer reset to prevent the agents from chatting eternally
    private void TalkToAnotherAI()
    {

        Debug.Log("I see " + otherEnemyAI.gameObject.name);
        timer -= Time.deltaTime;
        // Check if timer hasn't run out yet. If it hasn't, approach the other agent
        // Stop the other agent from moving
        if (timer > 1)
        { 
            Halt(otherEnemyAI.GetComponent<NavMeshAgent>());
            Debug.Log(otherEnemyAI.gameObject.name + " is stopped");
            ComeOverTo(otherEnemyAI);
            Debug.Log("Approaching it");

        }
        // If the timer ran out, continue patrolling
        if (timer <= 1)
        {
            SetDestination();

            Debug.Log("destination was reset for " + this.gameObject.name);
            Debug.Log(this.gameObject.name + " goes to " + _currentWaypoint.name);

            Patrol(_navMeshAgent);
            Patrol(otherEnemyAI.GetComponent<NavMeshAgent>());

            // Reset the timer for the next time they see each other - with delay
            interactionTimer = 0;
            StartCoroutine(ResetTimer());
        }
    }

     private void checkObjectOut(){
        interactionTimer -= Time.deltaTime;
        if (interactionTimer > 1)
        {
            ComeOverTo(objectOfInterest);
        }
        
        if (interactionTimer <= 1)
        {
            SetDestination();
            Patrol(_navMeshAgent);
            timer = 0;
            StartCoroutine(ResetTimer());
        }
    }

    // Approaching another game object
    private void ComeOverTo(GameObject obj)
    {
        Vector3 targetVector = obj.transform.position;
        _navMeshAgent.SetDestination(targetVector);
        if (_navMeshAgent.remainingDistance <= 2f)
        {
            Halt(_navMeshAgent);
        }
    }

    // Hunting behaviour sets the AI speed to max and deactivates the PlayerSimAI
    // if it's close enough for a potential hit
    private void Hunt(GameObject prey)
    {
        _navMeshAgent.speed = _maxSpeed;
        _navMeshAgent.SetDestination(prey.transform.position);
        if (_navMeshAgent.remainingDistance <= 1f)
        {
            prey.SetActive(false);
            _travelling = true;
           
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 fovLine1 = Quaternion.AngleAxis(_viewAngle, transform.up) * transform.forward * _viewRadius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-_viewAngle, transform.up) * transform.forward * _viewRadius;

        // Drawing the field of view
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);

        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, transform.forward * _viewRadius);

    }


}
