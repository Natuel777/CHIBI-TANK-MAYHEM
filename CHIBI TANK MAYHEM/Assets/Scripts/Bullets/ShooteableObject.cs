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

    //Aceleración vertical (m/s²) que la bala se aplica a sí misma mientras vuela. Negativa = hacia abajo, como Physics.gravity.y.
    //Las balas rectas no caen, por eso el valor por defecto es 0; las que sí caen lo sobrescriben.
    public virtual float Gravity => 0f;

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
