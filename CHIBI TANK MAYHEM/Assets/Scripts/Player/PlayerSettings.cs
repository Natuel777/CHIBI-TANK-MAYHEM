using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettingsSO", menuName = "Scriptable Objects/PlayerSettingsSO")]
public class PlayerSettingsSO : ScriptableObject
{
    [Header("Tank Settings")]
    
    //Modelo de velocidad-objetivo (estándar de vehicle controllers): el input define una velocidad
    //deseada y el tanque acelera/desacelera hacia ella. Es estable por diseño — la velocidad nunca
    //puede dispararse porque el objetivo mismo está acotado, a diferencia de aplicar fuerzas brutas.
    [Tooltip("Velocidad máxima de avance/retroceso, en m/s. Con una oruga dañada este límite baja a menos de la mitad.")]
    public float maxSpeed = 8f;

    [Tooltip("Aceleración al presionar avance, en m/s². Cuán rápido llega a maxSpeed.")]
    public float acceleration = 12f;

    [Tooltip("Desaceleración al soltar (o al frenar/invertir), en m/s². Cuán rápido se detiene. Más alto = frena más seco.")]
    public float deceleration = 18f;

    [Tooltip("Velocidad máxima de giro, en grados/s. Aplica tanto girando parado como en movimiento.")]
    public float maxTurnRate = 90f;

    [Tooltip("Aceleración de giro, en grados/s². Cuán rápido alcanza maxTurnRate. Más alto = giro más inmediato.")]
    public float turnAcceleration = 360f;

    [Tooltip("Ángulo (grados) del cabeceo visual mientras el tanque arranca, frena o invierte dirección. 0 = nada. Se apaga solo al llegar a velocidad crucero, y NUNCA se activa por girar en movimiento. Es puramente visual (sobre el mesh), no afecta el avance ni puede volcar el tanque.")]
    public float pitchTiltAmount = 4f;

    [Tooltip("Tiempo de suavizado del cabeceo, en segundos (aprox. cuánto tarda en llegar al ángulo objetivo y en volver a 0). Más alto = balanceo más lento y amortiguado; más bajo = más reactivo. ~0.2-0.4 se siente natural.")]
    public float pitchTiltSmoothTime = 0.25f;

    [Tooltip("Centro de masa del tanque, en espacio local del root. Ajustalo a la posición real del centro geométrico para que el tanque se asiente natural.")]
    public Vector3 centerOfMassOffset = new Vector3(0f, -0.3f, 0f);

    [Tooltip("Distancia (m) del raycast hacia abajo para detectar si el tanque está apoyado en el suelo. Aprox. la mitad de la altura del tanque + un margen. Si está más lejos del suelo que esto, se considera 'en el aire' (cayendo) y el control de avance se suelta para que la gravedad maneje la caída.")]
    public float groundCheckDistance = 1f;

    [Header("Tank Head Settings")]
    public float aimRotationSpeed = 720f;
    public float minTankHeadPitch = -20f;
    public float maxTankHeadPitch = 20f;

    [Header("Turret Settings")]
    public float turretRotationSpeed = 900f;
    public float minTurretPitch = -20f;
    public float maxTurretPitch = 20f;

    [Tooltip("The rate at which the turret can fire, in shots per second.")]
    public float turretFireRate = 10f;

    [Tooltip("The cooldown time between turret shots, in seconds.")]
    public float turretFireCooldown = 0.1f;

    [Tooltip("If true, the tank's turret will follow the camera's aim direction. If false, it will not rotate with the camera.")]
    public bool tankTurretFollowsCamera = false;

    [Header("Secondary Turrets Settings")]
    
    [Tooltip("The rate at which the secondary turrets can fire, in shots per second.")]
    public float secondaryTurretFireRate = 5f;

    [Tooltip("The cooldown time between secondary turret shots, in seconds.")]
    public float secondaryTurretFireCooldown = 0.2f;

    [Tooltip("Vertical (pitch) rotation limits for secondary turrets, relative to their fixed forward direction. Always applied.")]
    public float minSecondaryTurretPitch = -10f;
    public float maxSecondaryTurretPitch = 10f;

    [Tooltip("Horizontal (yaw) rotation limits for secondary turrets, relative to their fixed forward direction. Only used when secondaryTurretsCanAim is true.")]
    public float minSecondaryTurretYaw = -30f;
    public float maxSecondaryTurretYaw = 30f;

    [Tooltip("If true, secondary turrets also rotate horizontally (yaw) to aim at the target, clamped by minSecondaryTurretYaw/maxSecondaryTurretYaw. If false, they only pitch and fire straight along their fixed forward direction.")]
    public bool secondaryTurretsCanAim = false;

    [Header("Camera Settings")]
    public float orbitSensitivityX = 3f;
    public float orbitSensitivityY = 2f;

    [Tooltip("Minimum pitch angle for the camera orbit.")]
    public float orbitMinPitch = -20f;

    [Tooltip("Maximum pitch angle for the camera orbit.")]
    public float orbitMaxPitch = 60f;
    
    [Tooltip("Distance from the orbit target for the camera.")]
    public float orbitDistance = 10f;
    
    [Tooltip("Height offset for the camera's pivot point.")]
    public float orbitPivotHeightOffset = 1.5f;
}