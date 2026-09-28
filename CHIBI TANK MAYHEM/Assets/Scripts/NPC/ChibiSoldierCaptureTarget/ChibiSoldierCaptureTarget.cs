using UnityEngine;
using UnityEngine.UI;
using System;

public class ChibiSoldierCaptureTarget : MonoBehaviour
{
    [SerializeField] private Slider _captureBar;
    private Action _OnCaptured;

    #region Getters
    public Action OnCaptured => _OnCaptured;
    #endregion

    #region MVC
    public ChibiSoldierCaptureTargetView view;
    #endregion

    private void Awake()
    {
        view = new ChibiSoldierCaptureTargetView(_captureBar, _OnCaptured);
    }
}
