using UnityEngine;

[CreateAssetMenu(fileName = "NPCSettingsSO", menuName = "Scriptable Objects/NPCSettingsSO")]
public class NPCSettingsSO : ScriptableObject
{
    public float moveSpeed = 10f;
    public float idleSearchTimer = 5f;
}
