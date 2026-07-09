using UnityEngine;

public enum BulletType
{
    CommonCannonBullet
}

public abstract class ShooteableObject : MonoBehaviour
{
    [SerializeField] protected float speed;
    [SerializeField] protected float initialDamage;
    [SerializeField] protected float lifetime;

    protected float currentLifetime;

    [SerializeField] protected BulletType bulletType;

    public float Speed => speed;
    public float InitialDamage => initialDamage;
    public BulletType BulletType => bulletType;

    public virtual void ResetState()
    {
        currentLifetime = lifetime;
    }

    public abstract void Shoot(Vector3 direction);
}
