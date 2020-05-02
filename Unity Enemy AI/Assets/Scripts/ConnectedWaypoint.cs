using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedWaypoint : Waypoint
{
    [SerializeField]
    protected float _connectivityRadius = 50f;

    List<ConnectedWaypoint> _connections;

    // Start is called before the first frame update
    void Start()
    {
        //Grab all the objects with Waypoint tag in the scene
        GameObject[] allWaypoints = GameObject.FindGameObjectsWithTag("Waypoint");

        _connections = new List<ConnectedWaypoint>();

        //Check if they are connected waypoints
        for (int i = 0; i < allWaypoints.Length; i++) {
            ConnectedWaypoint nextWaypoint = allWaypoints[i].GetComponent<ConnectedWaypoint>();

            //if we found a waypoint

            if (nextWaypoint != null) {
                if (Vector3.Distance(this.transform.position, nextWaypoint.transform.position) <= _connectivityRadius && nextWaypoint != this) {
                    _connections.Add(nextWaypoint);
                }
            }
        }
        
    }


    public override void OnDrawGizmos()
    {
        float debugDrawRadius = 0.5f;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, debugDrawRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, _connectivityRadius);
    }



    public ConnectedWaypoint NextWaypoint(ConnectedWaypoint previousWaypoint) {
        if (_connections.Count == 0)
        {
            Debug.LogError("Insufficient waypoint count");
            return null;
        }
        else if (_connections.Count == 1 && _connections.Contains(previousWaypoint))
        {
            return previousWaypoint;
        }
        else {
            ConnectedWaypoint nextWaypoint;
            int nextIndex = 0;

            do
            {
                nextIndex = UnityEngine.Random.Range(0, _connections.Count);
                nextWaypoint = _connections[nextIndex];
            } while (nextWaypoint == previousWaypoint);

            return nextWaypoint;
        }
    }
}
