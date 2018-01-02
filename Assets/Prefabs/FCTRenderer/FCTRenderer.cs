using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class FCTRequest
{
    public FCT_TYPE Type { get; set; }
    public string Text { get; set; }
    public Vector3 WorldPos { get; set; }
    public Vector2? Dir { get; set; }

public FCTRequest(FCT_TYPE type, string text, Vector3 worldPos, Vector2? dir)
    {
        Type = type;
        Text = text;
        WorldPos = worldPos;

        Dir = dir;
    }
}

public enum FCT_TYPE { HIT, CRIT, DOTHIT, DOTCRIT, REBOUNDHIT, REBOUNDCRIT, HEALTH }

public class FCTRenderer : MonoBehaviour
{
    static FCTRenderer instance;

    [SerializeField] GameObject [] m_textObjects;
    [SerializeField] Transform m_fctParent;

    Queue<FCTRequest> m_fctQueue;

    void Awake ()
    {
        m_fctQueue = new Queue<FCTRequest>();
        instance = this;
    }
	
	void Update ()
    {
        while (m_fctQueue.Count > 0)
        {
            var req = m_fctQueue.Dequeue();
            var fct = Instantiate(m_textObjects[(int)req.Type], m_fctParent);

            var script = fct.GetComponent<FCT>();

            script.Init(req.Text, req.WorldPos, req.Dir);
        }
    }

    public static void AddFCT(FCT_TYPE type, string text, Vector3 worldPos, Vector2? dir = null)
    {
        instance.m_fctQueue.Enqueue(new FCTRequest(type, text, worldPos, dir));
    }
}
