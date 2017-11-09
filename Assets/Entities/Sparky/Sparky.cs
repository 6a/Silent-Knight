using System.Collections;
using UnityEngine;

public class Sparky : MonoBehaviour
{
    JKnightControl m_knight;
    float m_vOffset;

    private void Awake()
    {
        m_knight = FindObjectOfType<JKnightControl>();
        m_vOffset = 0;
        transform.parent = null;
    }

    void Start ()
    {
		
	}
	

	void LateUpdate ()
    {
        m_vOffset = Mathf.Sin(Time.time * 3);

        var diff = Vector3.up * m_vOffset * 0.1f;

        transform.position = new Vector3(m_knight.transform.position.x, 1, m_knight.transform.position.z) + diff;

        bool willMove = (Random.Range(0, 120) == 0);

        if (willMove)
        {
            StopAllCoroutines();
            StartCoroutine(Rotate());
        }
	}

    IEnumerator Rotate()
    {
        int movement = Random.Range(0, 20);
        int limit = movement;
        int mod = (Random.Range(0, 2) == 0) ? -1 : 1;

        while (movement > 0)
        {
            float nextMovement = 10 * Mathf.Sin(Mathf.Deg2Rad * (((float)movement / (float)limit) * 180));

            transform.Rotate(transform.up, nextMovement * mod);

            movement--;
            yield return new WaitForFixedUpdate();
        }

    }
}