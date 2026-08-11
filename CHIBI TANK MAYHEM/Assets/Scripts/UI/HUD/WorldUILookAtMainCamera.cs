using UnityEngine;

public class WorldUILookAtMainCamera : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        if(Camera.main != null)
            mainCameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if(mainCameraTransform != null)
        {
            Vector3 targetPosition = mainCameraTransform.position;
            targetPosition.y = transform.position.y; 
            transform.LookAt(targetPosition);
            transform.Rotate(0, 180, 0); 
        }
    }
}