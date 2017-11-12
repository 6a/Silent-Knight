﻿using Entities;
using PathFinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI : MonoBehaviour
{
    public AISet Enemies { get; set; }
    static AI instance;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Load information about what enemy units are on which platforms
    /// </summary>
    public static void LoadAIData(AISet data)
    {
        instance.Enemies = data;
    }

    public static void ActivateUnits(int platformID)
    {
        foreach (var enemy in instance.Enemies.Enemies[platformID])
        {
            if (enemy == null) continue;
            var enemyObj = (PathFindingObject)enemy;
            if (!enemyObj.IsDead) enemyObj.Running = true;
        }
    }

    public static void RemoveUnit(int platformID, IAttackable unit)
    {
        instance.Enemies.GetEnemies(platformID).Remove(unit);
    }

    public static IAttackable GetNewTarget(Vector3 seekerPosition, int platformID)
    {
        var potentialTargets = instance.Enemies.GetEnemies(platformID);

        if (potentialTargets == null || potentialTargets.Count == 0) return null;

        IAttackable closest = null;

        float minDistance = float.MaxValue;

        foreach (var targ in potentialTargets)
        {
            if (targ == null) continue;

            var dist = Vector3.Distance(targ.Position(), seekerPosition);

            if (dist < minDistance)
            {
                minDistance = dist;
                closest = targ;
            }
        }

        return closest;
    }
}

public class AISet
{
    public Dictionary<int, List<IAttackable>> Enemies { get; set; }

    public AISet(Dictionary<int, List<IAttackable>> enemies)
    {
        Enemies = enemies;
    }

    public List<IAttackable> GetEnemies(int platformID)
    {
        return Enemies[platformID];
    }

    public void Remove(int platformID, IAttackable reference)
    {
        Enemies[platformID].Remove(reference);
    }
}
