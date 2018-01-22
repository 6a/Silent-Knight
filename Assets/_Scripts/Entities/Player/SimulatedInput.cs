/// <summary>
/// Provides simple input simulation functionality.
/// </summary>
class SimulatedInput
{
    bool[] m_triggers = new bool[5] { false, false, false, false, false };

    /// <summary>
    /// Checks the state of a particular trigger.
    /// </summary>
    public bool Get(Enums.PLAYER_ATTACK a)
    {
        return m_triggers[(int)a];
    }

    /// <summary>
    /// Sets a particular trigger.
    /// </summary>
    public void Set(Enums.PLAYER_ATTACK a, bool value)
    {
        m_triggers[(int)a] = value;
    }
}
