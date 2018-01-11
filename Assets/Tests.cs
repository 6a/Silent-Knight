using Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tests : MonoBehaviour
{
	void Start ()
    {
        for (int i = 0; i < 20; i++)
        {
            print(LevelScaling.GetScaledDamage(i + 1, 20));
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
