/*
* Copyright 2020, Veronika Bondarenko
* This file is a part of AI in games Bachelor thesis project
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerAIGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject playerSimAI;
    void Start()
    {
        playerSimAI = GameObject.FindGameObjectWithTag("OtherAI");
    }

    // Update is called once per frame
    void Update()
    {
        if (playerSimAI == null)
        {
            Instantiate(playerSimAI, this.transform);
        }
    }
}
