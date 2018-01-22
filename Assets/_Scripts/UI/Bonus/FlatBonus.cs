/// <summary>
/// Represents a flat bonus. Derives from abstract class 'Bonus'.
/// </summary>
public class FlatBonus : Bonus
{
    // The amount by which 1 point increases the value for this bonus.
    public float m_increasePerLevel;

    public FlatBonus(float increasePerLevel, int times, int limit)
    {
        m_increasePerLevel = increasePerLevel;
        Increments = times;
        Limit = limit;
    }

    /// <summary>
    /// Returns the total bonus as a flat percentage increase.
    /// </summary>
    public override float Get(float current)
    {
        return current + (m_increasePerLevel * Increments);
    }

    /// <summary>
    /// Returns the total bonus as a decimal between 0 and 1.
    /// </summary>
    public override float GetDecimal(float current)
    {
        return current + ((Increments * m_increasePerLevel) / 100f);
    }

    /// <summary>
    /// Returns the total bonus as a flat percentage increase, for the next level.
    /// </summary>
    public override float GetNext(float current)
    {
        return current + (m_increasePerLevel * (Increments + 1));
    }
}
