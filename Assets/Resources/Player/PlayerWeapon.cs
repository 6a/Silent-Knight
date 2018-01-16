using Entities;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    PlayerPathFindingObject m_knight;
    BoxCollider m_col;

    void Awake()
    {
        m_knight = GetComponentInParent<PlayerPathFindingObject>();
        m_col = GetComponent<BoxCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        m_knight.OnSwordCollision(other.GetComponent<IAttackable>());
    }

    public void Switch(bool on)
    {
        m_col.enabled = on;
    }
}
