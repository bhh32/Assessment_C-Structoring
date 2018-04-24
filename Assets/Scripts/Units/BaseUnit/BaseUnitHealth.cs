using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnitHealth : MonoBehaviour, IDamagable
{
    protected delegate void UnitHeal(float healAmt);
    UnitHeal OnUnitHeal; 

    float maxHealth;
    protected float MaxHealth
    {
        get { return maxHealth; }
        set { maxHealth = value; }
    }

    float currentHealth;
    protected float CurrentHealth
    {
        get { return currentHealth; }
        set { currentHealth = value; }
    }

    void Awake()
    {
        currentHealth = MaxHealth;
        OnUnitHeal += Heal;
    }

    public void TakeDamage(float damageAmt)
    {
        CurrentHealth -= damageAmt;

        if (CurrentHealth <= 0f)
            Destroy(gameObject);
    }

    public void Heal(float healAmt)
    {
        if (CurrentHealth < MaxHealth)
        {
            CurrentHealth += healAmt;
            if (CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;
        }
    }


}
