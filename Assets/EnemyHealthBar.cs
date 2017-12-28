using UnityEngine;

public class EnemyHealthBar : HealthBar
{
    [SerializeField] GameObject [] cells;

    public void ToggleVisibility (bool on)
    {
        foreach (var c in cells)
        {
            c.SetActive(on);
        }
    }
}
