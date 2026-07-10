using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public event Action<Vector2> Move;
    public event Action ShootCannon;
    public event Action ShootGun;
    public event Action<Vector2> Aim;

    public void OnMove(InputAction.CallbackContext ctx)
    {
        Move?.Invoke(ctx.ReadValue<Vector2>());
    }

    public void OnAim(InputAction.CallbackContext ctx)
    {
        Aim?.Invoke(ctx.ReadValue<Vector2>());
    }

    public void OnShootCannon(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
            ShootCannon?.Invoke();
    }
}