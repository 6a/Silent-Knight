using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonTest : MonoBehaviour
{
	void Start ()
    {
        GetComponent<DungeonGenerator>().DiscoverValidLevels(100, 0, true);
	}

}
