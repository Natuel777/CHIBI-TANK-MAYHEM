using UnityEngine;
using System;

public class ParticularHealthModel : IDamageable
{
    private float _currentHealth, _maxHealth;
    private Action _DeathSequence = null;

    public ParticularHealthModel(float maxHealth, Action DeathSequence = null)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;

        if(DeathSequence != null)
            _DeathSequence = DeathSequence;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;

        if(_currentHealth <= 0)
        {
            _currentHealth = 0;
            
            if(_DeathSequence != null) _DeathSequence.Invoke();
        }
    }
}
