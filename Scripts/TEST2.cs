using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST2 : MonoBehaviour
{
    public int var = 0;

    private void Start()
    {
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            var++;
        } 
    }
}

