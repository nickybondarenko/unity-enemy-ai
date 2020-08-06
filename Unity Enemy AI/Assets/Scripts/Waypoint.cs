/*
* Copyright 2020: visualization of waypoints by Table Flip Games
* This file is a part of AI in games Bachelor thesis project
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [SerializeField]
    protected float debugDrawRadius = 1.0F;

    // Visualization of the waypoints
    public virtual void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, debugDrawRadius);
    }
}
