using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonTest : MonoBehaviour
{
    [SerializeField] private int m_iterations;
    [SerializeField] private int m_start;
    [SerializeField] private bool m_fabricate;
    [SerializeField] private int m_platforms = 10;
    [SerializeField] private int m_nodes = 2;
    [SerializeField] private float m_wait = 0.5f;

    void Start ()
    {
        GetComponent<DungeonGenerator>().DiscoverValidLevels(m_iterations, m_start, m_fabricate, m_platforms, m_nodes, m_wait);
	}

}
