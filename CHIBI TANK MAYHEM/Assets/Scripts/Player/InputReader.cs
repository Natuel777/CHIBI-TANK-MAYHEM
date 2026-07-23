using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public event Action<Vector2> Move;
    public event Action ShootCannon;
    public event Action<bool> ShootGun;
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

    public void OnShootGun(InputAction.CallbackContext ctx)
    {
        if(ctx.started)
            ShootGun?.Invoke(true);

        else if(ctx.canceled)
            ShootGun?.Invoke(false);
    }

    //Permite disparar el evento ShootGun desde fuera del Input System (ej. un stick táctil en pantalla
    //que además de panear la cámara debe disparar mientras se lo mantiene tocado). Mismo contrato que
    //OnShootGun: true al empezar a tocar, false al soltar.
    public void SetShootGun(bool isShooting)
    {
        ShootGun?.Invoke(isShooting);
    }
}