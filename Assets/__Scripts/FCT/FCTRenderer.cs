using System.Collections.Generic;
using UnityEngine;
using SilentKnight.Utility;

namespace SilentKnight.FCT
{
    /// <summary>
    /// Container for storing information for a floating combat text instance.
    /// </summary>
    struct FCTRequest
    {
        // Type of FCT request.
        public Enums.FCT_TYPE Type { get; set; }

        // Text to display (ensure text is either found in, or added to, the relevent TMP texture in the editor.
        public string Text { get; set; }

        // Position at which to place this FCT instance on spawn.
        public Vector3 WorldStartPos { get; set; }

        // Direction in which this FCT instance should travel on spawn, if any.
        public Vector2? Dir { get; set; }

        public FCTRequest(Enums.FCT_TYPE type, string text, Vector3 worldStartPos, Vector2? dir)
        {
            Type = type;
            Text = text;
            WorldStartPos = worldStartPos;

            Dir = dir;
        }
    }

    /// <summary>
    /// Handles all floating combat text rendering.
    /// </summary>
    public class FCTRenderer : MonoBehaviour
    {
        static FCTRenderer instance;

        // References to the various different FCT prefabs.
        [SerializeField] GameObject[] m_textObjects;

        // UI Rect to act as a parent within the UI.
        [SerializeField] Transform m_fctParent;

        // Storage for all pending FCT requests.
        Queue<FCTRequest> m_fctQueue;

        void Awake()
        {
            m_fctQueue = new Queue<FCTRequest>();
            instance = this;
        }

        void Update()
        {
            // Every frame, process all the FCT jobs in the queue.
            while (m_fctQueue.Count > 0)
            {
                var req = m_fctQueue.Dequeue();
                var fct = Instantiate(m_textObjects[(int)req.Type], m_fctParent);

                var script = fct.GetComponent<FCT>();

                script.Init(req.Text, req.WorldStartPos, req.Dir);
            }
        }

        /// <summary>
        /// Adds a floating combat text request to this manager.
        /// </summary>
        public static void AddFCT(Enums.FCT_TYPE type, string text, Vector3 worldPos, Vector2? dir = null)
        {
            instance.m_fctQueue.Enqueue(new FCTRequest(type, text, worldPos, dir));
        }
    }
}