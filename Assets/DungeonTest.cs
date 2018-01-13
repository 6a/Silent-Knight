using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonTest : MonoBehaviour
{
    [SerializeField] private int m_iterations;
    [SerializeField] private int m_start;
    [SerializeField] private bool m_fabricate;

    void Start ()
    {
        GetComponent<DungeonGenerator>().DiscoverValidLevels(m_iterations, m_start, m_fabricate);
	}

}
