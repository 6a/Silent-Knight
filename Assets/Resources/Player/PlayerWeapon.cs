using Entities;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    PlayerPathFindingObject m_player;
    BoxCollider m_col;

    void Awake()
    {
        m_player = GetComponentInParent<PlayerPathFindingObject>();
        m_col = GetComponent<BoxCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        m_player.OnSwordCollision(other.GetComponent<IAttackable>());
    }

    public void Switch(bool on)
    {
        m_col.enabled = on;
    }
}
