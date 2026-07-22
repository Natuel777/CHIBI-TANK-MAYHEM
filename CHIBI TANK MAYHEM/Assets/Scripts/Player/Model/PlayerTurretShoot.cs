using UnityEngine;
using System.Collections.Generic;

public class PlayerTurretShoot : IInputInitialize, IShootable
{
    private Transform _turretMuzzleTransform;
    private Transform _tankTransform;
    private Transform[] _secondaryMuzzleTransforms;
    private Transform[] _secondaryPivots;              //el transform que se rota (padre del cañón, con la rotación de fábrica)
    private Quaternion[] _secondaryPivotRestRotations; //localRotation de reposo de cada pivote (su orientación de fábrica)
    private Vector3[] _secondaryRestAxesLocal;         //eje de disparo de reposo, en el espacio del PADRE del pivote
    private bool _initialized = false, _isShooting = false;
    private BulletType _currentBulletType = BulletType.CommonTurretBullet;
    private float _fireRate, _secondaryFireRate;
    private float _fireCooldown, _secondaryFireCooldown;
    private float _minSecondaryPitch, _maxSecondaryPitch;
    private bool _secondaryTurretsCanAim;
    private float _minSecondaryYaw, _maxSecondaryYaw;
    private List<int> _secondaryTurretsAbleToShoot;   //índices de las torretas del lado del target
    private Vector3 _aimTargetPoint;

    public PlayerTurretShoot(Transform turretMuzzleTransform, Transform tankTransform,
                            float fireRate, float fireCooldown,
                            float secondaryFireRate, float secondaryFireCooldown,
                            float minSecondaryPitch, float maxSecondaryPitch,
                            bool secondaryTurretsCanAim, float minSecondaryYaw, float maxSecondaryYaw)
    {
        _turretMuzzleTransform = turretMuzzleTransform;
        _tankTransform = tankTransform;
        _fireRate = fireRate;
        _fireCooldown = fireCooldown;
        _secondaryFireRate = secondaryFireRate;
        _secondaryFireCooldown = secondaryFireCooldown;
        _minSecondaryPitch = minSecondaryPitch;
        _maxSecondaryPitch = maxSecondaryPitch;
        _secondaryTurretsCanAim = secondaryTurretsCanAim;
        _minSecondaryYaw = minSecondaryYaw;
        _maxSecondaryYaw = maxSecondaryYaw;
    }

    public void Initialize(InputReader inputReader)
    {
        inputReader.ShootGun += SetShooting;
        _secondaryTurretsAbleToShoot = new List<int>();
        _initialized = true;
    }

    public void SetShooting(bool isShooting)
    {
        _isShooting = isShooting;
    }

    public PlayerTurretShoot SetSecondaryMuzzleTransforms(Transform[] secondaryMuzzleTransforms)
    {
        if(secondaryMuzzleTransforms.Length == 0) return this;

        _secondaryMuzzleTransforms = secondaryMuzzleTransforms;
        _secondaryPivots = new Transform[secondaryMuzzleTransforms.Length];
        _secondaryPivotRestRotations = new Quaternion[secondaryMuzzleTransforms.Length];
        _secondaryRestAxesLocal = new Vector3[secondaryMuzzleTransforms.Length];

        for(int i = 0; i < secondaryMuzzleTransforms.Length; i++)
        {
            Transform muzzle = secondaryMuzzleTransforms[i];

            //El pivote es el cañón visual (padre del muzzle): rotarlo hace que el mesh apunte
            //y el muzzle (hijo) lo sigue. Su localRotation de reposo es el "centro" del apuntado.
            Transform pivot = muzzle.parent != null ? muzzle.parent : muzzle;
            _secondaryPivots[i] = pivot;
            _secondaryPivotRestRotations[i] = pivot.localRotation;

            //Eje de disparo de reposo (.up del muzzle) expresado en el espacio del PADRE del pivote,
            //que es donde luego calculamos y clampeamos la dirección. Se cachea antes de rotar nada.
            //Se usa el padre del pivote (no el pivote) para que su escala no distorsione la dirección.
            Transform pivotParent = pivot.parent;
            _secondaryRestAxesLocal[i] = pivotParent != null
                ? pivotParent.InverseTransformDirection(muzzle.up).normalized
                : muzzle.up;
        }

        return this;
    }

    public void ArtificialUpdate(Vector3 aimTargetPoint)
    {
        if(!_initialized || !_isShooting) return;

        _fireCooldown -= Time.deltaTime;

        if(_fireCooldown <= 0f)
        {
            Shoot();
            _fireCooldown = 1f / _fireRate;
        }

        if(_secondaryMuzzleTransforms != null && _secondaryMuzzleTransforms.Length > 0)
        {
            _aimTargetPoint = aimTargetPoint;
            SelectSecondaryTurretsBySide(aimTargetPoint);
            AimSecondaryTurrets();

            _secondaryFireCooldown -= Time.deltaTime;

            if(_secondaryFireCooldown <= 0f)
            {
                ShootSecondaryTurrets();
                _secondaryFireCooldown = 1f / _secondaryFireRate;
            }
        }

        _secondaryTurretsAbleToShoot.Clear();
    }

    public void Shoot()
    {
        ShooteableObject bullet = TurretBulletFactory.Instance.Create(_currentBulletType, _turretMuzzleTransform.position, _turretMuzzleTransform.rotation);
        bullet.Shoot(_turretMuzzleTransform.up);

        #if UNITY_EDITOR
        Debug.Log($"PlayerShoot: Shot a bullet of type {_currentBulletType} from position {_turretMuzzleTransform.position} in direction {_turretMuzzleTransform.forward}");
        #endif
    }

    //Selecciona TODAS las torretas del mismo lado que el target (izquierda/derecha), no solo la más cercana.
    //El lado se decide por el signo del producto cruzado contra tankUp: mismo signo = mismo lado.
    private void SelectSecondaryTurretsBySide(Vector3 aimTargetPoint)
    {
        Vector3 tankForward = _tankTransform.forward;
        Vector3 tankUp = _tankTransform.up;
        Vector3 tankPosition = _tankTransform.position;

        Vector3 toTarget = aimTargetPoint - tankPosition;
        float targetSide = Mathf.Sign(Vector3.Dot(Vector3.Cross(tankForward, toTarget), tankUp));

        for(int i = 0; i < _secondaryMuzzleTransforms.Length; i++)
        {
            Vector3 toMuzzle = _secondaryMuzzleTransforms[i].position - tankPosition;
            float muzzleSide = Mathf.Sign(Vector3.Dot(Vector3.Cross(tankForward, toMuzzle), tankUp));

            if(muzzleSide == targetSide)
                _secondaryTurretsAbleToShoot.Add(i);
        }
    }

    private void AimSecondaryTurrets()
    {
        //De fábrica (secondaryTurretsCanAim == false) el yaw queda fijo en 0 — solo pitch vertical.
        float minYaw = _secondaryTurretsCanAim ? _minSecondaryYaw : 0f;
        float maxYaw = _secondaryTurretsCanAim ? _maxSecondaryYaw : 0f;

        foreach(int i in _secondaryTurretsAbleToShoot)
        {
            Transform pivot = _secondaryPivots[i];
            Transform pivotParent = pivot.parent;
            if(pivotParent == null) continue;

            //Se trabaja en WORLD space para que "arriba" sea inequívocamente Vector3.up mundial:
            //el pitch (elevación vertical) se mide contra la vertical real del mundo, no contra el
            //up del espacio local del pivote (que está rotado 90° de fábrica y confundiría los ejes).
            //
            //Eje de disparo de reposo del cañón en world: es su orientación de fábrica compuesta con
            //la rotación actual del padre — así rota con el tanque sin cálculos extra.
            Vector3 restAxisWorld = pivotParent.rotation * _secondaryRestAxesLocal[i];

            Vector3 worldDir = _aimTargetPoint - pivot.position;
            Vector3 clampedWorldDir = AimMath.ClampYawPitch(worldDir, restAxisWorld, Vector3.up,
                                                            minYaw, maxYaw, _minSecondaryPitch, _maxSecondaryPitch);

            //Rotación world de reposo del cañón (fábrica * padre), y la desviación que lleva su eje
            //de disparo de reposo a la dirección clampeada. Se aplica como rotación world del pivote.
            Quaternion restRotationWorld = pivotParent.rotation * _secondaryPivotRestRotations[i];
            Quaternion aimRotation = Quaternion.FromToRotation(restAxisWorld, clampedWorldDir);
            pivot.rotation = aimRotation * restRotationWorld;
        }
    }

    public void ShootSecondaryTurrets()
    {
        foreach(int i in _secondaryTurretsAbleToShoot)
        {
            Transform muzzle = _secondaryMuzzleTransforms[i];
            ShooteableObject bullet = TurretBulletFactory.Instance.Create(_currentBulletType, muzzle.position, muzzle.rotation);
            bullet.Shoot(muzzle.up);
        }
    }
}
