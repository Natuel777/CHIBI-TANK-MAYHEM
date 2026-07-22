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

    //Clampea yaw y pitch de una dirección DESEADA respecto a un eje de reposo (restAxis), todo
    //expresado en el MISMO espacio (típicamente el espacio local del padre del pivote de la torreta).
    //Trabajar en un espacio local que ya rota con el tanque evita tener que recomponer la rotación
    //del tanque a mano: el llamador convierte la dirección al espacio local con InverseTransformDirection
    //y el resultado se aplica como localRotation, así que el clamp es correcto para cualquier orientación
    //del tanque sin aritmética extra de quaternions.
    //
    //restAxis = hacia dónde apunta el cañón "de fábrica" (su eje de disparo en reposo, sin desviación).
    //yaw se mide alrededor de rotationUp (típicamente Vector3.up del espacio local); pitch es la
    //elevación por encima/debajo del plano horizontal de ese espacio.
    public static Vector3 ClampYawPitch(Vector3 direction, Vector3 restAxis, Vector3 rotationUp,
                                        float minYaw, float maxYaw, float minPitch, float maxPitch)
    {
        //Base ortonormal centrada en restAxis: 'forward' = restAxis proyectado al plano horizontal,
        //'right' = perpendicular en ese plano. Yaw 0 y pitch 0 corresponden exactamente a restAxis.
        Vector3 restHorizontal = Vector3.ProjectOnPlane(restAxis, rotationUp).normalized;
        if(restHorizontal.sqrMagnitude < 0.0001f) restHorizontal = restAxis; //restAxis casi vertical: degenerado, no clampeamos yaw
        Vector3 rightAxis = Vector3.Cross(rotationUp, restHorizontal).normalized;

        //Descomponer la dirección deseada en yaw (alrededor de rotationUp) y pitch (elevación).
        float dirForward = Vector3.Dot(direction, restHorizontal);
        float dirRight = Vector3.Dot(direction, rightAxis);
        float dirUp = Vector3.Dot(direction, rotationUp);

        float yawAngle = Mathf.Atan2(dirRight, dirForward) * Mathf.Rad2Deg;
        yawAngle = Mathf.Clamp(yawAngle, minYaw, maxYaw);
        float yawRad = yawAngle * Mathf.Deg2Rad;

        float horizontalDistance = new Vector2(dirForward, dirRight).magnitude;
        float pitchAngle = Mathf.Atan2(dirUp, horizontalDistance) * Mathf.Rad2Deg;
        pitchAngle = Mathf.Clamp(pitchAngle, minPitch, maxPitch);
        float pitchRad = pitchAngle * Mathf.Deg2Rad;

        //Reconstruir en la misma base: forward/right dan el yaw, rotationUp da el pitch.
        return (restHorizontal * Mathf.Cos(yawRad) + rightAxis * Mathf.Sin(yawRad)) * Mathf.Cos(pitchRad)
                + rotationUp * Mathf.Sin(pitchRad);
    }
}
