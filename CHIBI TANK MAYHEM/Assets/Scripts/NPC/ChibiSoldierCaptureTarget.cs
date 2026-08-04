using UnityEngine;
using UnityEngine.UI;

public class ChibiSoldierCaptureTarget : MonoBehaviour
{
    [SerializeField] private Slider _captureBar;

    public void SetCaptureProgress(float normalizedProgress)
    {
        _captureBar.value = normalizedProgress;
    }
}
