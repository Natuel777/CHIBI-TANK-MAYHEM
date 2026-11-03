using UnityEngine;
using UnityEngine.UI;

public class MobileInputUIManager : MonoBehaviour
{
    public Image cannonCooldownImage;

    private void Start()
    {
        ServiceLocator.Instance.Register<MobileInputUIManager>(this);
    }
}
