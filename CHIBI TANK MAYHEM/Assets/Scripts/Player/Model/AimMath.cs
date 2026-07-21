using UnityEngine;

public static class AimMath
{
    //El clamp de pitch tiene que aplicarse sobre un ÁNGULO, no sobre direction.y directamente:
    //direction.y es una distancia en unidades de mundo, y la misma altura absoluta representa
    //ángulos de elevación distintos según qué tan lejos esté el punto apuntado — por eso clampear
    //la componente Y directamente hacía que el límite de pitch pareciera cambiar según la distancia.
    public static Vector3 ClampPitch(Vector3 direction, float minPitch, float maxPitch)
    {
        Vector3 horizontalDirection = direction;
        horizontalDirection.y = 0f;
        float horizontalDistance = horizontalDirection.magnitude;

        float pitchAngle = Mathf.Atan2(direction.y, horizontalDistance) * Mathf.Rad2Deg;
        pitchAngle = Mathf.Clamp(pitchAngle, minPitch, maxPitch);
        float pitchRad = pitchAngle * Mathf.Deg2Rad;

        Vector3 horizontalDirNormalized = horizontalDistance > 0.0001f ? horizontalDirection / horizontalDistance : Vector3.forward;
        return horizontalDirNormalized * Mathf.Cos(pitchRad) + Vector3.up * Mathf.Sin(pitchRad);
    }
}
