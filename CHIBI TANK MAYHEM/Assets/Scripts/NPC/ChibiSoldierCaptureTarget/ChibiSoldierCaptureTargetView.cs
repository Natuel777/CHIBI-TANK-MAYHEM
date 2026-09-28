using UnityEngine;
using UnityEngine.UI;
using System;

public class ChibiSoldierCaptureTargetView
{
    private Slider _captureBar;
    private Action _OnCaptured;

    public ChibiSoldierCaptureTargetView(Slider captureBar, Action OnCaptured)
    {
        _captureBar = captureBar;
        _OnCaptured = OnCaptured;
    }

    public void SetCaptureProgress(float normalizedProgress)
    {
        _captureBar.value = normalizedProgress;
    }

    //Metodo para cambiar mesh ONCAPTURED
}
