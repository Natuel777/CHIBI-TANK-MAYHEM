using UnityEngine;

public enum BulletType
{
    CommonCannonBullet,
    CommonTurretBullet,
    CommonChibiSoldierBullet
}

public abstract class ShooteableObject : MonoBehaviour, IServiceConsumer
{
    [SerializeField] protected float speed;
    [SerializeField] protected float initialDamage;
    [SerializeField] protected float lifetime;

    protected float currentLifetime;
    protected ParticleSystemFactory _PSFactory;

    [SerializeField] protected BulletType bulletType;

    public float Speed => speed;
    public float InitialDamage => initialDamage;
    public BulletType BulletType => bulletType;

    public virtual void ResetState()
    {
        currentLifetime = lifetime;
    }

    public abstract void Shoot(Vector3 direction);

    public bool TryResolveService()
    {
        if(_PSFactory != null) return true;

        return ServiceLocator.Instance.TryGet(out _PSFactory);
    }
}
