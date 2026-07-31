using UnityEngine;
using UnityEngine.EventSystems;

//Va en el MISMO GameObject que un OnScreenStick (ej. el botón Shootgun táctil): mientras el stick
//esté siendo tocado, mantiene disparada la ametralladora. Así un solo dedo panea la cámara (vía el
//OnScreenStick) y dispara al mismo tiempo (vía este componente), sin componentes en conflicto.
public class ShootWhileHeld : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private InputReader _inputReader;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(_inputReader != null) _inputReader.SetShootGun(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(_inputReader != null) _inputReader.SetShootGun(false);
    }

    //Si el objeto se desactiva mientras se estaba tocando, OnPointerUp podría no dispararse
    //y la ametralladora quedaría trabada disparando — se corta el disparo por seguridad.
    private void OnDisable()
    {
        if(_inputReader != null) _inputReader.SetShootGun(false);
    }
}
