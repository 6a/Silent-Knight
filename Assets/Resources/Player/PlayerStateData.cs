/// <summary>
/// Data container for player character state information
/// </summary>
struct PlayerStateData
{
    public bool DidDeflect                  { get; set; }
    public bool ShouldApplyUltiDamage       { get; set; }
    public bool IsDeflecting                { get; set; }
    public bool ShouldTickUlt               { get; set; }
    public bool IsStartingSpec              { get; set; }
    public float PreviousAttackTime         { get; set; }
    public float NextUltTick                { get; set; }
    public float MaxHealth                  { get; set; }
    public float NextRegenTick              { get; set; }
    public float BaseDamageHolder           { get; set; }
    public float SpeedTemp                  { get; set; }
    public int AttackState                  { get; set; }
    public int XP                           { get; set; }
    public int Level                        { get; set; }
    public Entities.ITargetable EndTarget   { get; set; }
    public System.Collections.Generic.List<int> EnemiesHitByWeapon;
}
