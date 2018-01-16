using Entities;
using System.Collections.Generic;
/// <summary>
/// Data container for player character state information
/// </summary>
struct PlayerStateData
{
    public int AttackState                      { get; set; }
    public float BaseDamageHolder               { get; set; }
    public bool DidDeflect                      { get; set; }
    public ITargetable EndTarget                { get; set; }
    public List<int> EnemiesHitByWeapon         { get; set; }
    public bool IsDeflecting                    { get; set; }
    public bool IsStartingSpec                  { get; set; }
    public int Level                            { get; set; }
    public float MaxHealth                      { get; set; }
    public float NextRegenTick                  { get; set; }
    public float NextUltTick                    { get; set; }
    public float PreviousAttackTime             { get; set; }
    public bool ShouldApplyUltiDamage           { get; set; }
    public bool ShouldTickUlt                   { get; set; }
    public float SpeedTemp                      { get; set; }
    public int XP                               { get; set; }
}
