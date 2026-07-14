using UnityEngine;

public class PlayerAim
{
    private readonly Transform _tankHeadTransform;
    private readonly float _aimRotationSpeed;
    private readonly Camera _camera;

    public PlayerAim(Transform tankHeadTransform, float aimRotationSpeed, Camera camera)
    {
        _tankHeadTransform = tankHeadTransform;
        _aimRotationSpeed = aimRotationSpeed;
        _camera = camera;
    }

    public void ArtificialUpdate()
    {
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
        direction.y = 0f;

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
    }
}
