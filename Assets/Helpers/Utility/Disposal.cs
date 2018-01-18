using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Struct for marking objects for deletion.
/// </summary>
struct DisposalObject
{
    public GameObject Reference { get; set; }
    public float TimeToDelete { get; set; }

    public DisposalObject(GameObject gameObjectReference, float delay)
    {
        Reference = gameObjectReference;
        TimeToDelete = Time.time + delay;
    }
}

/// <summary>
/// Wrapper for Unity GameObject disposal. 
/// </summary>
public class Disposal : MonoBehaviour
{
    Queue<DisposalObject> m_disposalQueue;
    static Disposal instance;

	void Awake ()
    {
        m_disposalQueue = new Queue<DisposalObject>();
        instance = this;
	}
	
	void LateUpdate ()
    {
        // If there is an object due to be disposed, it is removed from the queue and deleted.
        // Only one object is deleted per frame, to reduce per-frame GC calls.

        if (m_disposalQueue.Count == 0) return;

        lock (m_disposalQueue)
        {
            if (Time.time > m_disposalQueue.Peek().TimeToDelete)
            {
                var objectToDelete = m_disposalQueue.Dequeue();
                Destroy(objectToDelete.Reference);
            }
        }
    }

    /// <summary>
    /// Mark a GameObject for deletion
    /// </summary>
    public static void Dispose(GameObject reference, float delay = 1f)
    {
        lock (instance.m_disposalQueue)
        {
            instance.m_disposalQueue.Enqueue(new DisposalObject(reference, delay));
        }
    }
}
