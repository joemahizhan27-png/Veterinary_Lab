using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LabSetup
{
    /// <summary>
    /// Controls the optical view of the microscope:
    /// - Optical Zoom (FOV adjustment)
    /// - Stage / Slide XY Navigation (Panning)
    /// - Focus adjustment
    /// Compatible with both New Input System and Legacy Input Manager.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class MicroscopeController : MonoBehaviour
    {
        [Header("Optical Zoom")]
        public float zoomSpeed = 15f;
        public float minFOV = 1.5f;   // High magnification
        public float maxFOV = 25f;    // Low magnification / overview
        public float currentFOV = 7.5f;

        [Header("Slide Pan (Stage Movement)")]
        public float panSpeed = 0.05f;
        public Vector2 panLimits = new Vector2(0.04f, 0.04f); // Bounds within glass slide

        [Header("Focus Fine-Tuning")]
        public float focusSpeed = 0.01f;

        private Camera microscopeCamera;
        private Vector3 initialLocalPosition;
        private Vector2 currentPanOffset = Vector2.zero;

        private void Awake()
        {
            microscopeCamera = GetComponent<Camera>();
            initialLocalPosition = transform.localPosition;
            if (microscopeCamera != null)
            {
                microscopeCamera.fieldOfView = currentFOV;
            }
        }

        private void Update()
        {
            HandleZoomInput();
            HandlePanInput();
        }

        private void HandleZoomInput()
        {
            float zoomDelta = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                zoomDelta -= Mouse.current.scroll.ReadValue().y * 0.01f;
            }
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed)
                {
                    zoomDelta -= 1f * Time.deltaTime * zoomSpeed;
                }
                if (Keyboard.current.sKey.isPressed)
                {
                    zoomDelta += 1f * Time.deltaTime * zoomSpeed;
                }
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            zoomDelta -= Input.GetAxis("Mouse ScrollWheel") * 10f;
            if (Input.GetKey(KeyCode.W))
            {
                zoomDelta -= 1f * Time.deltaTime * zoomSpeed;
            }
            if (Input.GetKey(KeyCode.S))
            {
                zoomDelta += 1f * Time.deltaTime * zoomSpeed;
            }
#endif

            if (Mathf.Abs(zoomDelta) > 0.0001f && microscopeCamera != null)
            {
                currentFOV = Mathf.Clamp(currentFOV + zoomDelta, minFOV, maxFOV);
                microscopeCamera.fieldOfView = currentFOV;
            }
        }

        private void HandlePanInput()
        {
            Vector2 panInput = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed) panInput.x -= 1f;
                if (Keyboard.current.dKey.isPressed) panInput.x += 1f;
                if (Keyboard.current.qKey.isPressed) panInput.y += 1f;
                if (Keyboard.current.eKey.isPressed) panInput.y -= 1f;
            }
            if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                panInput.x -= mouseDelta.x * 0.1f;
                panInput.y -= mouseDelta.y * 0.1f;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKey(KeyCode.A)) panInput.x -= 1f;
            if (Input.GetKey(KeyCode.D)) panInput.x += 1f;
            if (Input.GetKey(KeyCode.Q)) panInput.y += 1f;
            if (Input.GetKey(KeyCode.E)) panInput.y -= 1f;
            if (Input.GetMouseButton(1))
            {
                panInput.x -= Input.GetAxis("Mouse X") * 2f;
                panInput.y -= Input.GetAxis("Mouse Y") * 2f;
            }
#endif

            if (panInput.sqrMagnitude > 0.0001f)
            {
                // Scale pan speed with current zoom FOV (slower movement at higher zoom)
                float zoomFactor = Mathf.Clamp01(currentFOV / maxFOV);
                float effectiveSpeed = panSpeed * Mathf.Max(0.15f, zoomFactor) * Time.deltaTime;

                currentPanOffset.x = Mathf.Clamp(currentPanOffset.x + panInput.x * effectiveSpeed, -panLimits.x, panLimits.x);
                currentPanOffset.y = Mathf.Clamp(currentPanOffset.y + panInput.y * effectiveSpeed, -panLimits.y, panLimits.y);

                transform.localPosition = initialLocalPosition + new Vector3(currentPanOffset.x, 0, currentPanOffset.y);
            }
        }
    }
}
