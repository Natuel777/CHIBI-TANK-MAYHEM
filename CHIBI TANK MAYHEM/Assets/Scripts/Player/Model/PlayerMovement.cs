using UnityEngine;

public class PlayerMovement : IInputInitialize
{
    private Transform _transform;
    private float _rotationSpeed, _movementSpeed;
    private bool _initialized = false;
    private Vector2 _moveInput;

    public PlayerMovement(Transform playerTransform, float rotationSpeed, float movementSpeed)
    {
        _transform = playerTransform;
        _rotationSpeed = rotationSpeed;
        _movementSpeed = movementSpeed;
    }

    public void Initialize(InputReader inputReader)
    {
        inputReader.Move += SetMoveInput;
        _initialized = true;
    }

    private void SetMoveInput(Vector2 inputs) {_moveInput = inputs;}

    public void ArtificialUpdate()
    {
        if(!_initialized) return;

        if(_moveInput.y == 0)
        {
            Rotate(_moveInput.x);
            return;
        }

        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
        _transform.position += _transform.forward * direction.z * _movementSpeed * Time.deltaTime;
        Rotate(direction.x);
    }

    private void Rotate(float horizontalInput)
    {
        _transform.rotation *= Quaternion.Euler(0, horizontalInput * 100 * Time.deltaTime, 0);
    }
}
