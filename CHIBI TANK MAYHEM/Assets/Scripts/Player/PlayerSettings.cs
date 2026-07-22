using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettingsSO", menuName = "Scriptable Objects/PlayerSettingsSO")]
public class PlayerSettingsSO : ScriptableObject
{
    [Header("Tank Settings")]
    public float rotationSpeed = 5f;
    public float movementSpeed = 5f;
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