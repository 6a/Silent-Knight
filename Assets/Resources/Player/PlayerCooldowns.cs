/// <summary>
/// Storage class for player cooldown current live values
/// </summary>
public class PlayerCooldowns
{
    const int COUNT = 5;

    float[] m_cd = new float[COUNT] { 0, 0, 0, 0, 0 };

    public int Count { get { return COUNT; } }

    /// <summary>
    /// Returns the current cooldown for a particular attack.
    /// </summary>
    public float Get(Enums.PLAYER_ATTACK attack)
    {
        return m_cd[(int)attack];
    }

    /// <summary>
    /// Sets a value for a particular attacks current cooldown.
    /// </summary>
    public void Set(Enums.PLAYER_ATTACK attack, float val)
    {
        m_cd[(int)attack] = val;
    }
}
