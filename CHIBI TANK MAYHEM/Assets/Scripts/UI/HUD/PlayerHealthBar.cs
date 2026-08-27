using UnityEngine;

[System.Serializable]
public class PlayerHealthBar
{
    [SerializeField] private HealthBar _bodyHealthBar;
    [SerializeField] private HealthBar _headHealthBar;
    [SerializeField] private HealthBar _turretHealthBar;
    [SerializeField] private HealthBar _trailLFTHealthBar;
    [SerializeField] private HealthBar _trailRGTHealthBar;

    public void Initialize(float bodyMaxHealth, float headMaxHealth, float turretMaxHealth, float trailLFTMaxHealth, float trailRGTMaxHealth)
    {
        _bodyHealthBar.Initialize(bodyMaxHealth);
        _headHealthBar.Initialize(headMaxHealth);
        _turretHealthBar.Initialize(turretMaxHealth);
        _trailLFTHealthBar.Initialize(trailLFTMaxHealth);
        _trailRGTHealthBar.Initialize(trailRGTMaxHealth);

        PlayerEvents.PlayerTankBodyTakesDamage += _bodyHealthBar.TakeDamage;
        PlayerEvents.PlayerTankHeadTakesDamage += _headHealthBar.TakeDamage;
        PlayerEvents.PlayerTankTurretTakesDamage += _turretHealthBar.TakeDamage;
        PlayerEvents.PlayerTankTrailLFTTakesDamage += _trailLFTHealthBar.TakeDamage;
        PlayerEvents.PlayerTankTrailRGTTakesDamage += _trailRGTHealthBar.TakeDamage;

        PlayerEvents.PlayerTankBodyHeals += _bodyHealthBar.Heal;
        PlayerEvents.PlayerTankHeadHeals += _headHealthBar.Heal;
        PlayerEvents.PlayerTankTurretHeals += _turretHealthBar.Heal;
        PlayerEvents.PlayerTankTrailLFTHeals += _trailLFTHealthBar.Heal;
        PlayerEvents.PlayerTankTrailRGTHeals += _trailRGTHealthBar.Heal;
    }

    public void ArtificialUpdate()
    {
        _bodyHealthBar.ArtificialUpdate();
        _headHealthBar.ArtificialUpdate();
        _turretHealthBar.ArtificialUpdate();
        _trailLFTHealthBar.ArtificialUpdate();
        _trailRGTHealthBar.ArtificialUpdate();
    }
}
