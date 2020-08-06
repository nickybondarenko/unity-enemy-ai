/*
* COPYRIGHT: code by Veronika Bondarenko
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
