using UnityEngine;
using TMPro;

public class LoadingScreen : Screen
{
    [SerializeField] private TextMeshProUGUI _statusText;

    private void Start()
    {
        Activate();
    }

    public void SetStatus(string status)
    {
        _statusText.text = status;
    }
}
