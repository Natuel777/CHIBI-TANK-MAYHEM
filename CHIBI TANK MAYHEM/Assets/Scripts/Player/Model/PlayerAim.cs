using UnityEngine;

public class PlayerAim
{
    private readonly Transform _tankHeadTransform;
    private readonly float _aimRotationSpeed;
    private Vector2 _screenAimPosition;

    public PlayerAim(Transform tankHeadTransform, float aimRotationSpeed)
    {
        _tankHeadTransform = tankHeadTransform;
        _aimRotationSpeed = aimRotationSpeed;
    }

    public void Initialize(InputReader inputReader)
    {
        inputReader.Aim += SetAimPosition;
    }

    private void SetAimPosition(Vector2 screenPosition)
    {
        _screenAimPosition = screenPosition;
    }

    public void ArtificialUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(_screenAimPosition);
        Plane groundPlane = new Plane(Vector3.up, _tankHeadTransform.position);

        if(!groundPlane.Raycast(ray, out float distance))
            return;

        Vector3 worldAimPoint = ray.GetPoint(distance);
        Vector3 direction = worldAimPoint - _tankHeadTransform.position;
        direction.y = 0;

        if(direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        _tankHeadTransform.rotation = Quaternion.RotateTowards(
            _tankHeadTransform.rotation,
            targetRotation,
            _aimRotationSpeed * Time.deltaTime);
    }
}
