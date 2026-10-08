using UnityEngine;
using UnityEngine.InputSystem;

public class MicroscopeZoom : MonoBehaviour
{
    [Header("Zoom")]
    public float zoomSpeed = 20f;
    public float minFOV = 5f;
    public float maxFOV = 60f;

    [Header("Movement")]
    public float moveSpeed = 0.5f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // =========================
        // ZOOM
        // =========================

        if (Keyboard.current.wKey.isPressed)
        {
            cam.fieldOfView -= zoomSpeed * Time.deltaTime;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            cam.fieldOfView += zoomSpeed * Time.deltaTime;
        }

        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFOV, maxFOV);


        // =========================
        // MOVEMENT
        // =========================

        // Left
        if (Keyboard.current.aKey.isPressed)
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.World);
        }

        // Right
        if (Keyboard.current.dKey.isPressed)
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime, Space.World);
        }

        // Forward
        if (Keyboard.current.qKey.isPressed)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.World);
        }

        // Backward
        if (Keyboard.current.eKey.isPressed)
        {
            transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}