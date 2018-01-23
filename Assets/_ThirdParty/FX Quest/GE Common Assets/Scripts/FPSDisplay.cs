using UnityEngine;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
    Text t;
    int frameCount = 0;
    float dt = 0f;
    float fps = 0f;
    float updateRate = 4f;  // 4 updates per sec.

    void Awake()
    {
        t = GetComponent<Text>();
    }

    void Update()
    {
        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1f / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1f / updateRate;
            t.text = fps.ToString("F0");
        }
    }
}
