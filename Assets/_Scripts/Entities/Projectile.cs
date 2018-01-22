using Entities;
using UnityEngine;

/// <summary>
/// Represents a projectile that can be fired by an enemy and reflected by the player.
/// </summary>
public class Projectile : MonoBehaviour
{
    // Whether or not this projectile will be a critical hit.
    public bool Crit { get; set; }

    // The target and source of this projectile.
    IAttackable m_target, m_parent;

    // The speed at which this projectile should travel.
    float m_speed;

    // The time at which this projectile should be automatically destroyed.
    float m_endOfLife;

    // The amount of damage that this projectile will cause.
    float m_damage;

    // State for preventing multiple damage instances.
    bool m_dead;

    /// <summary>
    /// Initialise an instantiated projectile and start it's behaviour.
    /// </summary>
    public void Init(IAttackable target, IAttackable parent, float speed, float lifeTime, float damage)
    {
        m_target = target;
        m_parent = parent;
        m_speed = speed;
        m_endOfLife = Time.time + lifeTime;
        m_damage = damage;
    }

    /// <summary>
    /// Reflect this projectile towards a new target.
    /// </summary>
    public void Reflect(IAttackable parent, float lifeTime, float speed = -1, float damage = -1)
    {
        m_target = m_parent;
        m_parent = parent;
        m_endOfLife = Time.time + lifeTime;

        if (speed != -1) m_speed = speed;
        if (damage != -1) m_damage = damage;
    }

    /// <summary>
    /// Returns true if the query object is not of it's own type (to avoid infinite reflection loops on collision with the player).
    /// </summary>
    public bool CanBeReflected(IAttackable parent)
    {
        if (m_dead || parent == m_parent) return false;
        else return true;
    }
	
	void LateUpdate ()
    {
        // Early exits for null, expiration and death cases.
        if (m_target == null || m_target.IsDead || Time.time > m_endOfLife )
        {
            Disposal.Dispose(gameObject, 0.1f);
            return;
        }

        // Calculates the target position in world space.
        var targetPos = m_target.GetPosition() + (Vector3.up * 0.5f);

        // Once the unit reaches its target, apply damage and destroy itself.
        if (!m_dead && Vector3.Distance(targetPos, transform.position) < 0.1f)
        {
            m_dead = true;

            var t = (Crit) ? Enums.FCT_TYPE.REBOUNDCRIT : Enums.FCT_TYPE.REBOUNDHIT;

            m_target.OnDamage(m_parent as IAttacker, m_damage, t);

            Disposal.Dispose(gameObject, 0.1f);
        }

        // Vector representing the change in position for this object, for this frame.
        var tVec = m_speed * Time.deltaTime * (targetPos - transform.position).normalized;

        // Apply positional change.
        transform.position += tVec;
	}
}
