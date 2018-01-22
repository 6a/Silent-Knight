using Delaunay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SilentKnight.Engine;

namespace SilentKnight.Graphics
{
    /// <summary>
    /// Handles screen-shatter effect. Note that edge rendering functionality is present, but commented out.
    /// </summary>
    public class Shatter : MonoBehaviour
    {
        // List of all triangles to be rendered.
        List<Tri> m_triData = new List<Tri>();

        // Reference to the material to be used.
        Material m_mat;

        // Reference to the current screenshot to be used.
        Texture2D m_tex;

        static Shatter m_instance;
        static Coroutine m_currentRenderingRoutine;

        // publicly accessible static state boolean for external state checks.
        public static bool ShatterFinished;

        // State booleans for gating progress during the rendering coroutine.
        bool m_underlayEnabled;
        bool m_endShatter;

        /// <summary>
        /// Asynchronously execute all shatter behaviour.
        /// </summary>
        /// <returns></returns>
        IEnumerator RenderTriangles()
        {
            if (m_tex == null) yield return null;
            float offset = 0;
            float alpha = 1;
            float rotation = 0;
            m_endShatter = false;

            // Coroutine will continue running until the triangles alpha values are 0 (and they are no longer visible).
            while (alpha > 0)
            {
                yield return new WaitForEndOfFrame();

                // Enable the loading screen if it has been disabled. Also unpause just incase.
                if (!m_underlayEnabled)
                {
                    GameManager.EnableLoadingScreen();
                    m_underlayEnabled = true;

                    Time.timeScale = 1;
                }

                // Find the shatter shader if not yet found, and add it to a new material.
                // Shader is loaded from Resources folder.
                if (!m_mat)
                {
                    var shader = Resources.Load("Shaders/Shatter") as Shader;
                    m_mat = new Material(shader)
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                        mainTexture = m_tex
                    };
                }

                // Set OpenGL to orthographic view.
                GL.LoadOrtho();

                // Set the pass to 0 (there is only 1 pass anyway).
                m_mat.SetPass(0);

                // Calculate screen ratio and send value to shader.
                var screenratio = (float)Screen.width / Screen.height;
                m_mat.SetFloat("_ScreenRatio", screenratio);

                // Send current alpha value to shader.
                m_mat.SetFloat("_Alpha", alpha);

                // Transform (based on deltatime) and render all triangles.
                for (int i = 0; i < m_triData.Count; i++)
                {
                    GL.Begin(GL.TRIANGLES);

                    for (int j = 0; j < 3; j++)
                    {
                        GL.MultiTexCoord(0, m_triData[i].UV[j]);
                        GL.Vertex(m_triData[i].Vertices[j]);
                        //GL.MultiTexCoord(2, m_triData[i].BC[j]);
                    }

                    var c = m_triData[i].Center;
                    c.x *= screenratio;
                    m_triData[i].Matrix = Matrix4x4.Translate(m_triData[i].Dir * offset);
                    m_triData[i].Matrix = m_triData[i].Matrix * Matrix4x4.Translate(c);
                    m_triData[i].Matrix = m_triData[i].Matrix * Matrix4x4.Rotate(Quaternion.Euler(m_triData[i].Rotation * rotation));
                    m_triData[i].Matrix = m_triData[i].Matrix * Matrix4x4.Scale(new Vector3(0.97f, 0.97f, 0.97f));
                    m_triData[i].Matrix = m_triData[i].Matrix * Matrix4x4.Translate(-c);

                    GL.MultMatrix(m_triData[i].Matrix);

                    GL.End();
                }

                if (m_endShatter)
                {
                    alpha -= 0.4f * Time.deltaTime;
                    offset += 0.1f * Time.deltaTime;
                    rotation += 0.4f * Time.deltaTime;
                }
                else
                {
                    GameManager.ContinueLevelStart();
                }
            }

            ShatterFinished = true;
        }

        void Awake()
        {
            m_instance = this;
        }

        /// <summary>
        /// Asynchronously take a screenshot of the screen and then start the overlay rendering function.
        /// </summary>
        IEnumerator RecordFrame()
        {
            yield return new WaitForEndOfFrame();
            m_tex = ScreenCapture.CaptureScreenshotAsTexture();

            m_tex.filterMode = FilterMode.Point;
            m_tex.Apply();

            Time.timeScale = 0;

            if (m_currentRenderingRoutine != null) StopCoroutine(m_currentRenderingRoutine);
            m_currentRenderingRoutine = StartCoroutine(RenderTriangles());
        }

        /// <summary>
        /// Trigger the completion of the shatter function.
        /// </summary>
        public static void CompleteShatter()
        {
            m_instance.m_endShatter = true;
        }

        /// <summary>
        /// Start the shatter function.
        /// </summary>
        public static void StartShatter()
        {
            // Create a new blank list of triangles (Delauney lib).
            List<Triangle> triangles = new List<Triangle>();

            // Reset internal values.
            ShatterFinished = false;
            m_instance.m_underlayEnabled = false;
            m_instance.m_mat = null;

            // Create a new blank container for points.
            List<Vector2> randomPoints = new List<Vector2>();

            // Make a new blank list of colours (required for delauney lib).
            List<uint> colors = new List<uint>();

            //Random.InitState(1);

            // Fill the random points container with 20 random points.
            for (int i = 0; i < 20; i++)
            {
                randomPoints.Add(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
                colors.Add(0);
            }

            //Add 10 guaranteed edge points.
            for (int i = 0; i < 10; i++)
            {
                var rand = Random.Range(0f, 8f);

                float x = 0, y = 0;
                if (rand < 2)
                {
                    x = -1 + rand;
                    y = 1;
                }
                else if (rand < 4)
                {
                    x = 1;
                    y = -3 + rand;
                }
                else if (rand < 6)
                {
                    x = -5 + rand;
                    y = -1;
                }
                else
                {
                    x = -1;
                    y = -7 + rand;
                }

                randomPoints.Add(new Vector2(x, y));
                colors.Add(0);
            }

            // Add guaranteed corners.
            randomPoints.Add(new Vector2(-1, 1)); colors.Add(0);
            randomPoints.Add(new Vector2(1, 1)); colors.Add(0);
            randomPoints.Add(new Vector2(1, -1)); colors.Add(0);
            randomPoints.Add(new Vector2(-1, -1)); colors.Add(0);

            // Use the Delauney lib to convert the points into triangles.
            Voronoi voronoi = new Voronoi(randomPoints, colors, new Rect(0, 0, 2, 2));
            triangles = voronoi.Triangles();

            float speed = 0.05f;

            // Clear the internal triangle data storage.
            m_instance.m_triData.Clear();

            // Calculate the screen ratio.
            var screenratio = (float)Screen.width / Screen.height;

            // Load each triangle from the list into the internal storage, loading values appropriately.
            foreach (var triangle in triangles)
            {
                var v1 = new Vector3(triangle.sites[0].x, triangle.sites[0].y, 0);
                var v2 = new Vector3(triangle.sites[1].x, triangle.sites[1].y, 0);
                var v3 = new Vector3(triangle.sites[2].x, triangle.sites[2].y, 0);

                var t = new Tri()
                {
                    Vertices = new Vector3[3] { v1, v2, v3 },
                    UV = new Vector3[3] { (v1 + Vector3.one) / 2, (v2 + Vector3.one) / 2, (v3 + Vector3.one) / 2 }
                };
                //t.BC = new Vector3[3];
                //t.BC[0] = new Vector3(1, 0, 0);
                //t.BC[1] = new Vector3(0, 1, 0);
                //t.BC[2] = new Vector3(0, 0, 1);
                t.Dir = t.Center.normalized;
                t.Speed = speed;
                t.Rotation = (Random.rotation.eulerAngles * Random.Range(0.8f, 1f));

                m_instance.m_triData.Add(t);
            }

            // Start the next stage of the shatter process.
            m_instance.StartCoroutine(m_instance.RecordFrame());
        }
    }
}