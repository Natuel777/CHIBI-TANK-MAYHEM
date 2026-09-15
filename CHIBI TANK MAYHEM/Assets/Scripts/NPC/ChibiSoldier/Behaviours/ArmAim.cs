using UnityEngine;

public class ArmAim
{
    private Transform _transform, _target = null;
    private ChibiSoldier _parent;
    private IState _currentState;
    private float _rotationSpeed;
    
    public ArmAim(Transform t, ChibiSoldier parent, float rotationSpeed)
    {
        _transform = t;
        _parent = parent;
        _rotationSpeed = rotationSpeed;
        
        PlayerEvents.OnAnyStateChanged += UpdateCurrentParentState;
    }

    public void ArtificialUpdate()
    {
        if(_currentState != _parent.capturingState) return;

        if(_target == null) return;

        Vector3 aimDirection = (_target.position - _transform.position).normalized;

        //Convertimos la dirección al espacio local del padre del hombro: así el ángulo de pitch
        //queda relativo a hacia dónde ya mira el cuerpo, sin importar su rotación en el mundo.
        Vector3 localAimDirection = _transform.parent != null
            ? _transform.parent.InverseTransformDirection(aimDirection)
            : aimDirection;

        //Pitch necesario para que el forward (Z local) apunte al target, visto en el plano Y-Z.
        //Negativo porque un X positivo inclina el forward hacia abajo (convención de Unity).
        float targetPitch = -Mathf.Atan2(localAimDirection.y, localAimDirection.z) * Mathf.Rad2Deg;

        //Y y Z se dejan tal cual están: solo el eje X (pitch) rota para apuntar al target.
        Vector3 currentEuler = _transform.localEulerAngles;
        Quaternion targetRotation = Quaternion.Euler(targetPitch, currentEuler.y, currentEuler.z);
        _transform.localRotation = Quaternion.Slerp(_transform.localRotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    private void UpdateCurrentParentState() => _currentState = _parent.GetCurrentState();

    public void SetTarget(Transform target) => _target = target;
}
