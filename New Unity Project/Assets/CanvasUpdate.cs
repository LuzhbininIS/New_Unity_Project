using UnityEngine;
using UnityEngine.UI;

public class CanvasUpdate : MonoBehaviour
{
    // Update is called once per frame
    private void Update()
    {
        Camera camera = Camera.main;
        transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward,
            camera.transform.rotation * Vector3.up);
    }
}
