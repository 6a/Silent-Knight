class SimulatedInput
{
    bool[] m_triggers = new bool[5] { false, false, false, false, false };

    public bool Get(Enums.PLAYER_ATTACK a)
    {
        return m_triggers[(int)a];
    }

    public void Set(Enums.PLAYER_ATTACK a, bool value)
    {
        m_triggers[(int)a] = value;
    }
}
