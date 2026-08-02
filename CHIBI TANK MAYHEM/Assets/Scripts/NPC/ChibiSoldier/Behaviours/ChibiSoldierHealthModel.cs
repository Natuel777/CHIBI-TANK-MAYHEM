using UnityEngine;

public class ChibiSoldierHealthModel : IDamageable
{
    private float _currentHealth, _maxHealth;

    public ChibiSoldierHealthModel(float maxHealth)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;

        if(_currentHealth <= 0)
        {
            _currentHealth = 0;
            //Muerte
        }
    }
}
