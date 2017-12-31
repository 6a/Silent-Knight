using Delaunay;
using Delaunay.Geo;
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
                var x = (Vertices[0].x + Vertices[1].x + Vertices[2].x / 3);
                var y = (Vertices[0].y + Vertices[1].y + Vertices[2].y / 3);
                var z = (Vertices[0].z + Vertices[1].z + Vertices[2].z / 3);
                return new Vector3(x, y, z);
            }
        }


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

    public void OnPostRender()
    {

    }

    float offset = 0;
    float alpha = 1;

    bool m_underlayEnabled;

    IEnumerator RenderTriangles()
    {
        if (m_tex == null) yield return null;

        while (true)
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
                // Unity has a built-in shader that is useful for drawing
                // simple colored things. In this case, we just want to use
                // a blend mode that inverts destination colors.
                var shader = Shader.Find("J/Shatter");
                m_mat = new Material(shader);
                m_mat.hideFlags = HideFlags.HideAndDontSave;

                m_mat.mainTexture = m_tex;
            }

            GL.LoadOrtho();
            m_mat.SetPass(0);

            for (int i = 0; i < m_triData.Count; i++)
            {
                GL.Begin(GL.TRIANGLES);

                for (int j = 0; j < 3; j++)
                {
                    GL.TexCoord(m_triData[i].UV[j]);
                    GL.Vertex(m_triData[i].Vertices[j]);
                }

                var m = Matrix4x4.TRS(m_triData[i].Dir * offset * m_triData[i].Speed, Quaternion.Euler(0, 0, m_triData[i].Rotation.z), Vector3.one);

                // Any other transformations
                GL.MultMatrix(m);

                m_mat.SetFloat("_Alpha", alpha);

                GL.End();
                m_triData[i].Rotation.z += 1;
            }

            alpha -= 0.5f * Time.deltaTime;
            offset += Time.deltaTime;

        }
    }

    private void OnDrawGizmos()
    {
        if(triangles.Count > 0)
        {
            int c = -1;

            foreach (var t in triangles)
            {
                c++;

                if (c != 2 || c != 0) continue;
                Gizmos.color = Color.cyan;

                foreach (var site in t.sites)
                {
                    Gizmos.DrawSphere(new Vector3(site.x, 0, site.y), 0.01f);
                }
            }
        }
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

        StartCoroutine(RenderTriangles());
    }

    public static void StartShatter()
    {
        m_instance.m_underlayEnabled = false;

        List<Vector2> randomPoints = new List<Vector2>();

        List<uint> colors = new List<uint>();

        for (int i = 0; i < 20; i++)
        {
            randomPoints.Add(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
            colors.Add(0);
        }

        //Add guarunteed edge points
        for (int i = 0; i < 20; i++)
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

        // Add guarunteed corners
        randomPoints.Add(new Vector2(-1, 1)); colors.Add(0);
        randomPoints.Add(new Vector2(1, 1)); colors.Add(0);
        randomPoints.Add(new Vector2(1, -1)); colors.Add(0);
        randomPoints.Add(new Vector2(-1, -1)); colors.Add(0);

        Voronoi voronoi = new Voronoi(randomPoints, colors, new Rect(0, 0, 2, 2));

        m_instance.triangles = voronoi.Triangles();
        float speed = 0.05f;

        foreach (var triangle in m_instance.triangles)
        {
            var v1 = new Vector3(triangle.sites[0].x, triangle.sites[0].y, 0);
            var v2 = new Vector3(triangle.sites[1].x, triangle.sites[1].y, 0);
            var v3 = new Vector3(triangle.sites[2].x, triangle.sites[2].y, 0);

            var t = new Tri()
            {
                Vertices = new Vector3[3] { v1, v2, v3 },
                UV = new Vector3[3] { (v1 + Vector3.one) / 2, (v2 + Vector3.one) / 2, (v3 + Vector3.one) / 2 }
            };
            t.Dir = t.Center.normalized;
            t.Speed = speed;
            m_instance.m_triData.Add(t);
        }

        m_instance.StartCoroutine(m_instance.RecordFrame());
    }
}