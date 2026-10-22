using UnityEngine;
using UnityEngine.UI;

public sealed class ChibiSoldierCaptureTargetView
{
    private Slider _captureBar;
    private GameObject _idleMesh, _cannonMesh;

    public ChibiSoldierCaptureTargetView(Slider captureBar, GameObject idleMesh, GameObject cannonMesh)
    {
        _captureBar = captureBar;
        _idleMesh = idleMesh;
        _cannonMesh = cannonMesh;
    }

    public void SetCaptureProgress(float normalizedProgress)
    {
        _captureBar.value = normalizedProgress;
    }

    public void ShowIdleMesh()
    {
        _idleMesh.SetActive(true);
        _cannonMesh.SetActive(false);
    }

    public void ShowCannonMesh()
    {
        _idleMesh.SetActive(false);
        _cannonMesh.SetActive(true);
    }
}
