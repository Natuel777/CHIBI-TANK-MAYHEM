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

    public bool tankTurretFollowsCamera = false;

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