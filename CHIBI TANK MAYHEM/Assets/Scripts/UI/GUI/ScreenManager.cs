using UnityEngine;
using System.Collections.Generic;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance { get; private set; }

    private Stack<IScreen> _screens = new Stack<IScreen>();
    private IScreen _lastScreen = null;

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(this.gameObject);
        
        else Instance = this;
    }

    public void ActiveScreen(IScreen screen)
    {
        screen.Activate();
        
        if(_screens.Count > 0) _lastScreen = _screens.Pop();
        
        _screens.Push(screen);

        if(_lastScreen != null)
        {
            _lastScreen.Deactivate();
            _lastScreen = null;
        } 
    }

    public void DesactiveScreen()
    {
        if(_screens.Count <= 0) return;

        _screens.Pop().Deactivate();
    }
}
