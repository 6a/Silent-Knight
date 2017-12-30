using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulateShatter : MonoBehaviour
{
    RawImage m_output;
    bool m_running;

    List<GameObject> m_frags = new List<GameObject>();
    Vector3[] m_velocities;
    float[] m_rotations;

    void Start()
    {
        var all = GetComponentsInChildren<MeshRenderer>();

        foreach (var frag in all)
        {
            m_frags.Add(frag.gameObject);
        }
    }

    public void Run(RawImage output)
    {
        m_output = output;
        m_running = true;
        m_output.color = new Color(m_output.color.r, m_output.color.g, m_output.color.b, 1);

        m_velocities = new Vector3[m_frags.Count];
        m_rotations = new float[m_frags.Count];

        for (int i = 0; i < m_frags.Count; i++)
        {
            m_velocities[i] = new Vector3(Random.Range(-1, 2), Random.Range(-1, 2), Random.Range(-1, 2));

            var rot = (Random.Range(0, 2) == 0) ? -20f : 20f;

            m_rotations[i] = rot;
        }
    }

    public void Reset()
    {

    }

    void Update()
    {
        if (m_running)
        {
            m_output.color = new Color(m_output.color.r, m_output.color.g, m_output.color.b, m_output.color.a - 0.01f);

            for (int i = 0; i < m_frags.Count; i++)
            {
                m_frags[i].transform.position += (Time.deltaTime * m_velocities[i]);

                //m_frags[i].transform.

                //m_frags[i].transform.Rotate(Vector3.up, m_rotations[i] * Time.deltaTime);
            }
        }
    }

}
