using Entities;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    IAttackable m_target, m_parent;
    float m_speed;
    float m_endOfLife;
    float m_damage;

    public void Init(IAttackable target, IAttackable parent, float speed, float lifeTime, float damage)
    {
        m_target = target;
        m_parent = parent;
        m_speed = speed;
        m_endOfLife = Time.time + lifeTime;
        m_damage = damage;
    }

    public void Reflect(IAttackable parent, float lifeTime, float speed = -1, float damage = -1)
    {
        m_target = m_parent;
        m_parent = parent;
        m_endOfLife = Time.time + lifeTime;

        if (speed != -1) m_speed = speed;
        if (damage != -1) m_damage = damage;
    }

    public bool CanBeReflected(IAttackable parent)
    {
        if (parent == m_parent) return false;
        else return true;
    }
	
	void LateUpdate ()
    {
        if (m_target == null) return;

        var targetPos = m_target.Position() + (Vector3.up * 0.5f);

        if (Time.time > m_endOfLife || (m_parent != null && m_parent.IsDead)) Destroy(gameObject);

        if (Vector3.Distance(targetPos, transform.position) < 0.1f)
        {
            m_target.Damage(m_parent as IAttacker, m_damage);
            // TODO particles
            Destroy(gameObject);
        }

        var tVec = m_speed * Time.deltaTime * (targetPos - transform.position).normalized;

        transform.position += tVec;
	}
}
