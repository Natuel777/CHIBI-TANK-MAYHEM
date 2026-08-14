using UnityEngine;
using System;

public class TankHealthModel
{
    #region Health Models
    public ParticularHealthModel bodyHealth;
    public ParticularHealthModel headHealth;
    public ParticularHealthModel turretHealth;
    public ParticularHealthModel trailLFTHealth;
    public ParticularHealthModel trailRGTHealth;
    #endregion

    public TankHealthModel(float bodyMaxHealth, float headMaxHealth, float turretMaxHealth, float trailLFTMaxHealth, float trailRGTMaxHealth)
    {
        #region Health Model Initialization
        bodyHealth = new ParticularHealthModel(bodyMaxHealth, PlayerEvents.PlayerTankDeath);
        headHealth = new ParticularHealthModel(headMaxHealth, PlayerEvents.PlayerTankDeath);
        turretHealth = new ParticularHealthModel(turretMaxHealth, PlayerEvents.PlayerTankTurretDestroyed);
        //trailLFTHealth = new ParticularHealthModel(trailLFTMaxHealth, PlayerEvents.PlayerTankTrailDestroyed);
        //trailRGTHealth = new ParticularHealthModel(trailRGTMaxHealth, PlayerEvents.PlayerTankTrailDestroyed);
        #endregion

        #region Event Subscription
        PlayerEvents.PlayerTankDeath += Death;
        PlayerEvents.PlayerTankBodyTakesDamage += bodyHealth.TakeDamage;
        PlayerEvents.PlayerTankHeadTakesDamage += headHealth.TakeDamage;
        PlayerEvents.PlayerTankTurretTakesDamage += turretHealth.TakeDamage;
        //PlayerEvents.PlayerTankTrailTakesDamage += trailLFTHealth.TakeDamage;
        //PlayerEvents.PlayerTankTrailTakesDamage += trailRGTHealth.TakeDamage;

        PlayerEvents.PlayerTankBodyHeals += bodyHealth.Heal;
        PlayerEvents.PlayerTankHeadHeals += headHealth.Heal;
        PlayerEvents.PlayerTankTurretHeals += turretHealth.Heal;
        #endregion
    }

    public void BodyTakeDamage(float damage) => PlayerEvents.PlayerTankBodyTakesDamage.Invoke(damage);

    public void HeadTakeDamage(float damage) => PlayerEvents.PlayerTankHeadTakesDamage.Invoke(damage);

    public void TurretTakeDamage(float damage) => PlayerEvents.PlayerTankTurretTakesDamage.Invoke(damage);

    public void BodyHeal(float amount) => PlayerEvents.PlayerTankBodyHeals.Invoke(amount);

    public void HeadHeal(float amount) => PlayerEvents.PlayerTankHeadHeals.Invoke(amount);

    public void TurretHeal(float amount) => PlayerEvents.PlayerTankTurretHeals.Invoke(amount);

    //public void TrailLFTTakeDamage(float damage) =>

    //public void TrailRGTTakeDamage(float damage) =>

    private void Death() {}
}
