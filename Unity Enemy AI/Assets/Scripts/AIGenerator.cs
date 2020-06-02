using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGenerator : MonoBehaviour
{
    public GameObject EnemyAI;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 300; i++)
        {
            Instantiate(EnemyAI, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }

}
