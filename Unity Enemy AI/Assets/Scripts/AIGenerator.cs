/*
* Copyright 2020, code by Veronika Bondarenko
* This file is a part of AI in games Bachelor thesis project
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGenerator : MonoBehaviour
{
    public GameObject EnemyAI;
    
    // This generator can be called for testing purposes
    void Start()
    {
        for (int i = 0; i < 300; i++)
        {
            Instantiate(EnemyAI, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }

}
