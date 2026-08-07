using UnityEngine;

public class FlockingSteering
{
    private readonly Transform _transform;
    private readonly LayerMask _neighborLayerMask;
    private readonly float _neighborDetectionRadius;
    private readonly float _speed;
    private readonly float _rotationSpeed;
    private Vector3 _separationForce;

    public FlockingSteering(Transform transform, LayerMask neighborLayerMask,
                            float neighborDetectionRadius, float speed, float rotationSpeed)
    {
        _transform = transform;
        _neighborLayerMask = neighborLayerMask;
        _neighborDetectionRadius = neighborDetectionRadius;
        _speed = speed;
        _rotationSpeed = rotationSpeed;
    }

    //Separación + alineación + cohesión con los vecinos detectados este frame. Vector3.zero si no
    //hay ninguno. Quien llama decide qué hacer con esta fuerza (combinarla con una dirección hacia
    //un target, usarla sola para simplemente mantenerse agrupado, etc.).
    public Vector3 CalculateFlockingForce()
    {
        _separationForce = Vector3.zero;
        Collider[] neighbors = GetNeighbors();

        if(neighbors.Length > 0)
        {
            CalculateSeparationForce(neighbors);
            ApplyAlignment(neighbors);
            ApplyCohesion(neighbors);
        }

        return _separationForce;
    }

    public void Move(Vector3 direction)
    {
        direction.y = 0f;
        _transform.position += direction * _speed * Time.deltaTime;
    }

    public void RotateTowards(Vector3 direction)
    {
        if(direction != Vector3.zero)
        {
            direction.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    private Collider[] GetNeighbors() => Physics.OverlapSphere(_transform.position, _neighborDetectionRadius, _neighborLayerMask);

    private void CalculateSeparationForce(Collider[] neighbors)
    {
        foreach(Collider n in neighbors)
        {
            Vector3 direction = VectorMinusVector(n.transform.position, _transform.position);
            float distance = direction.magnitude;
            Vector3 away = -direction.normalized;

            if(distance > 0)
                _separationForce += away / distance;
        }
    }

    private void ApplyAlignment(Collider[] neighbors)
    {
        Vector3 neighborForward = Vector3.zero;

        foreach(Collider n in neighbors)
            neighborForward += n.transform.forward;

        if(neighborForward != Vector3.zero)
            neighborForward.Normalize();

        _separationForce += neighborForward;
    }

    private void ApplyCohesion(Collider[] neighbors)
    {
        Vector3 neighborCenter = Vector3.zero;

        foreach(Collider n in neighbors)
            neighborCenter += n.transform.position;

        neighborCenter /= neighbors.Length;
        Vector3 cohesionDirection = VectorMinusVector(neighborCenter, _transform.position).normalized;
        _separationForce += cohesionDirection;
    }

    private Vector3 VectorMinusVector(Vector3 pos1, Vector3 pos2) => (pos1 - pos2);
}
