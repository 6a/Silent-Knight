﻿using Delaunay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Shatter : MonoBehaviour
{
    class Tri
    {
        public Vector3 Center
        {
            get
            {
                var x = (Vertices[0].x + Vertices[1].x + Vertices[2].x) / 3;
                var y = (Vertices[0].y + Vertices[1].y + Vertices[2].y) / 3;
                var z = (Vertices[0].z + Vertices[1].z + Vertices[2].z) / 3;
                return new Vector3(x, y, z);
            }
        }

        public Matrix4x4 Matrix;
        public Vector3 Dir;
        public Vector3 Rotation;
        public Vector3[] UV;
        public Vector3[] Vertices;

        public float Speed;

    }
    List<Tri> m_triData = new List<Tri>();

    Material m_mat;
    Texture2D m_tex;
    [SerializeField] RenderTexture m_rt;
    [SerializeField] RawImage m_renderTarget;
    [SerializeField] Material m_targetMaterial;

    SimulateShatter m_shatterSim;

    static Shatter m_instance;
    static Coroutine m_currentRenderingRoutine;

    public void OnPostRender()
    {
    }

    bool m_underlayEnabled;

    IEnumerator RenderTriangles()
    {
        if (m_tex == null) yield return null;

        float offset = 0;
        float alpha = 1;
        float rotation = 0;

        while (alpha > 0)
        {
            yield return new WaitForEndOfFrame();

            if (!m_underlayEnabled)
            {
                GameManager.EnableLoadingScreen();
                m_underlayEnabled = true;

                Time.timeScale = 1;
            }

            if (!m_mat)
            {
                var shader = Shader.Find("J/Shatter");
                m_mat = new Material(shader);
                m_mat.hideFlags = HideFlags.HideAndDontSave;

                m_mat.mainTexture = m_tex;

            }

            GL.LoadOrtho();
            m_mat.SetPass(0);

            var screenratio = (float)Screen.width / Screen.height;
            m_mat.SetFloat("_ScreenRatio", screenratio);
            m_mat.SetFloat("_Alpha", alpha);

            for (int i = 0; i < m_triData.Count; i++)
            {
                GL.Begin(GL.TRIANGLES);

                for (int j = 0; j < 3; j++)
                {
                    GL.TexCoord(m_triData[i].UV[j]);
                    GL.Vertex(m_triData[i].Vertices[j]);
                }

                var c = m_triData[i].Center;
                c.x *= screenratio;
                m_triData[i].Matrix = Matrix4x4.Translate(m_triData[i].Dir * offset);
                m_triData[i].Matrix = m_triData[i].Matrix * Matrix4x4.Translate(c);
                m_triData[i].Matrix = m_triData[i].Matrix * Matrix4x4.Rotate(Quaternion.Euler(m_triData[i].Rotation * rotation));
                m_triData[i].Matrix = m_triData[i].Matrix * Matrix4x4.Translate(-c);

                GL.MultMatrix(m_triData[i].Matrix);

                GL.End();
            }
            alpha -= 0.4f * Time.deltaTime;
            offset += 0.1f * Time.deltaTime;
            rotation += 0.4f * Time.deltaTime;

        }

        //GameManager.DisableLoadingScreen();
    }

    private void OnDrawGizmos()
    {
        //if(triangles.Count > 0)
        //{
        //    int c = -1;

        //    foreach (var t in triangles)
        //    {
        //        c++;

        //        if (c != 2 || c != 0) continue;
        //        Gizmos.color = Color.cyan;

        //        foreach (var site in t.sites)
        //        {
        //            Gizmos.DrawSphere(new Vector3(site.x, 0, site.y), 0.01f);
        //        }
        //    }
        //}
    }

    List<Triangle> triangles = new List<Triangle>();

    void Awake()
    {
        m_instance = this;
    }

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

    public static void StartShatter()
    {
        m_instance.m_underlayEnabled = false;

        m_instance.m_mat = null;

        List<Vector2> randomPoints = new List<Vector2>();

        List<uint> colors = new List<uint>();

        //Random.InitState(1);

        for (int i = 0; i < 20; i++)
        {
            randomPoints.Add(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
            colors.Add(0);
        }

        //Add guaranteed edge points
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

        // Add guaranteed corners
        randomPoints.Add(new Vector2(-1, 1)); colors.Add(0);
        randomPoints.Add(new Vector2(1, 1)); colors.Add(0);
        randomPoints.Add(new Vector2(1, -1)); colors.Add(0);
        randomPoints.Add(new Vector2(-1, -1)); colors.Add(0);

        Voronoi voronoi = new Voronoi(randomPoints, colors, new Rect(0, 0, 2, 2));

        m_instance.triangles = voronoi.Triangles();
        float speed = 0.05f;

        m_instance.m_triData.Clear();

        var screenratio = (float)Screen.width / Screen.height;

        foreach (var triangle in m_instance.triangles)
        {
            var v1 = new Vector3(triangle.sites[0].x/* * screenratio*/, triangle.sites[0].y, 0);
            var v2 = new Vector3(triangle.sites[1].x/* * screenratio*/, triangle.sites[1].y, 0);
            var v3 = new Vector3(triangle.sites[2].x/* * screenratio*/, triangle.sites[2].y, 0);

            var t = new Tri()
            {
                Vertices = new Vector3[3] { v1, v2, v3 },
                UV = new Vector3[3] { (v1 + Vector3.one) / 2, (v2 + Vector3.one) / 2, (v3 + Vector3.one) / 2 }
            };
            t.Dir = t.Center.normalized;
            t.Speed = speed;
            t.Rotation = (Random.rotation.eulerAngles * Random.Range(0.8f, 1f));
            m_instance.m_triData.Add(t);
        }

        m_instance.StartCoroutine(m_instance.RecordFrame());
    }
}