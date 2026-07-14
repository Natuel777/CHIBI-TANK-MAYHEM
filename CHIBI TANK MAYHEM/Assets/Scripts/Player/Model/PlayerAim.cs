using UnityEngine;

public class PlayerAim : IWeaponAimProvider
{
    private readonly Transform _tankHeadTransform, _cannonMuzzleTransform;
    private readonly float _aimRotationSpeed, _minPitch, _maxPitch;
    private readonly Camera _camera;
    private readonly int _crosshairRaycastMask;
    private Vector3 _crosshairPoint;
    private const float _MaxDistance = 1000f;
    private const float _FallbackDistance = 50f;


    public PlayerAim(Transform tankHeadTransform, float aimRotationSpeed, 
                                                Camera camera, 
                                                Transform cannonMuzzle, 
                                                LayerMask crosshairRaycastMask,
                                                float minPitch, 
                                                float maxPitch)
    {
        _tankHeadTransform = tankHeadTransform;
        _cannonMuzzleTransform = cannonMuzzle;
        _aimRotationSpeed = aimRotationSpeed;
        _camera = camera;
        _crosshairRaycastMask = crosshairRaycastMask;
        _minPitch = minPitch;
        _maxPitch = maxPitch;
    }

    public void ArtificialUpdate()
    {
        #region Camera Raycast to get aim target point
        Vector2 screenCenter = new Vector2(_camera.pixelWidth / 2f, _camera.pixelHeight / 2f);
        Ray ray = _camera.ScreenPointToRay(screenCenter);
        Vector3 targetPoint;

        //Si hay racast en la dirección del mouse, se toma el punto de impacto como objetivo
        if(Physics.Raycast(ray, out RaycastHit hit))
            targetPoint = hit.point;
        
        //Si no hay impacto, se toma un punto lejano en la dirección del mouse
        else
            targetPoint = ray.origin + ray.direction * 500f;

        Vector3 direction = targetPoint - _tankHeadTransform.position;
        direction.y = Mathf.Clamp(direction.y, _minPitch, _maxPitch);

        if(direction.sqrMagnitude > 0.001f)
        {
            //Convertimos la dirección objetivo en quaternion
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            //Aplicamos la rotación en quaterniones suavemente hacia el objetivo
            _tankHeadTransform.rotation = Quaternion.RotateTowards(
                _tankHeadTransform.rotation,
                targetRotation,
                _aimRotationSpeed * Time.deltaTime);
        }
        #endregion

        #region Raycast from cannon muzzle to get crosshair point
        Ray muzzleRay = new Ray(_cannonMuzzleTransform.position, _cannonMuzzleTransform.up);
        
        //Si hay racast en la dirección del cañón, se toma el punto de impacto como objetivo
        //(se excluyen las balas propias vía LayerMask para que el rayo no choque con el disparo recién salido del cañón)
        if(Physics.Raycast(muzzleRay, out RaycastHit crosshairHit, _MaxDistance, _crosshairRaycastMask))
            _crosshairPoint = crosshairHit.point;

        //Si no hay impacto, se toma un punto lejano (pero acotado) en la dirección del cañón.
        //Un punto a _MaxDistance (1000) puede quedar casi perpendicular al forward de la cámara
        //al orbitar, y WorldToScreenPoint lo proyecta a coordenadas de pantalla absurdas aunque
        //técnicamente esté "delante" de cámara — por eso el fallback usa una distancia acotada.
        else _crosshairPoint = muzzleRay.origin + muzzleRay.direction * _FallbackDistance;
        #endregion
    }

    public Vector3 GetCrosshairPoint() { return _crosshairPoint; }
}
