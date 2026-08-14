using UnityEngine;

[System.Serializable]
public class PlayerHealthBar
{
    [SerializeField] private HealthBar _bodyHealthBar;
    [SerializeField] private HealthBar _headHealthBar;
    [SerializeField] private HealthBar _turretHealthBar;

    // TODO: LFT/RGT trail health bars - esperando a que termines trailLFTHealth/trailRGTHealth
    // en TankHealthModel.cs. Mismo patrón que las 3 de arriba una vez estén activas.

    public void Initialize(float bodyMaxHealth, float headMaxHealth, float turretMaxHealth)
    {
        _bodyHealthBar.Initialize(bodyMaxHealth);
        _headHealthBar.Initialize(headMaxHealth);
        _turretHealthBar.Initialize(turretMaxHealth);

        PlayerEvents.PlayerTankBodyTakesDamage += _bodyHealthBar.TakeDamage;
        PlayerEvents.PlayerTankHeadTakesDamage += _headHealthBar.TakeDamage;
        PlayerEvents.PlayerTankTurretTakesDamage += _turretHealthBar.TakeDamage;

        PlayerEvents.PlayerTankBodyHeals += _bodyHealthBar.Heal;
        PlayerEvents.PlayerTankHeadHeals += _headHealthBar.Heal;
        PlayerEvents.PlayerTankTurretHeals += _turretHealthBar.Heal;
    }

    public void ArtificialUpdate()
    {
        _bodyHealthBar.ArtificialUpdate();
        _headHealthBar.ArtificialUpdate();
        _turretHealthBar.ArtificialUpdate();
    }
}
