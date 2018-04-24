using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerHealth : BaseUnitHealth
{
    [SerializeField] float MaxHealth
    {
        get{ return base.MaxHealth; }
    }

    [SerializeField] float CurrentHealth
    {
        get{ return base.CurrentHealth; }
        set { base.CurrentHealth = value; }
    }

    void Awake()
    {
        base.MaxHealth = 50f;
    }

    void TakeDamage(float damageAmt)
    {
        base.TakeDamage(damageAmt);
    }

    void Heal(float healAmt)
    {
        base.Heal(healAmt);
    }
	
}
