using UnityEngine;

[CreateAssetMenu(fileName = "NPCSettingsSO", menuName = "Scriptable Objects/NPCSettingsSO")]
public class NPCSettingsSO : ScriptableObject
{
    public float moveSpeed = 10f;
    public float rotationSpeed = 5f;
    public float maxHealth = 100f;

    [Header("Idle Behaviour Settings")]
    public float idleSearchTimer = 5f;

    [Header("Flocking Settings")]
    public float neighborDetectionRadius = 10f;

    [Header("Shooting Settings")]
    public float gunFireCooldown = 0.1f;

}
