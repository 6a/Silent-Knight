using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct DisposalObject
{
    public GameObject Reference { get; set; }
    public float DisposalTime { get; set; }
    public float Delay { get; set; }

    public DisposalObject(GameObject reference, float disposalTime, float delay)
    {
        Reference = reference;
        DisposalTime = disposalTime;
        Delay = delay;
    }
}

public class Disposal : MonoBehaviour
{
    Queue<DisposalObject> m_disposalQueue;
    static Disposal instance;

	void Awake ()
    {
        m_disposalQueue = new Queue<DisposalObject>();
        instance = this;
	}
	
	void Update ()
    {
        if (m_disposalQueue.Count == 0) return;

        var next = m_disposalQueue.Dequeue();
        if (Time.time - next.DisposalTime > next.Delay)
        {
            Destroy(next.Reference);
        }
        else
        {
            m_disposalQueue.Enqueue(next);
        }
    }

    public static void Dispose(GameObject reference, float delay = 1f)
    {
        instance.m_disposalQueue.Enqueue(new DisposalObject(reference, Time.time, delay));
    }
}
