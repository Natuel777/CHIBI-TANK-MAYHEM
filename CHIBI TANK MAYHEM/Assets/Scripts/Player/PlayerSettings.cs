using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettingsSO", menuName = "Scriptable Objects/PlayerSettingsSO")]
public class PlayerSettingsSO : ScriptableObject
{
    public float rotationSpeed = 5f;
    public float movementSpeed = 5f;
}