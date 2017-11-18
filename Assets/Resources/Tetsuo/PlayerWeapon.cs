using Entities;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    JPlayerUnit m_knight;
    BoxCollider m_col;

    void Awake()
    {
        m_knight = GetComponentInParent<JPlayerUnit>();
        m_col = GetComponent<BoxCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        m_knight.OnContactEnemy(other.GetComponent<IAttackable>());
    }

    public void Switch(bool on)
    {
        m_col.enabled = on;
    }
}
