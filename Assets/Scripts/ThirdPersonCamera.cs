using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float mouseSensitivity = 3f;
    [SerializeField] float minYAngle = -30f;
    [SerializeField] float maxYAngle = 60f;

    float rotationX = 0f;
    float rotationY = 15f;

    void Awake()
    {
        EnsurePlayerReference();
    }

    void Start()
    {
        EnsurePlayerReference();
        LockCursor();
    }

    void OnEnable()
    {
        EnsurePlayerReference();
        LockCursor();
    }

    void EnsurePlayerReference()
    {
        if (player == null)
        {
            if (transform.parent != null)
            {
                player = transform.parent;
            }
            else
            {
                var p = FindAnyObjectByType<PlayerMovement>();
                if (p != null) player = p.transform;
            }
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        // Only run third person controls if the attached or child camera is active
        var cam = GetComponentInChildren<Camera>(false);
        if (cam != null && !cam.enabled)
        {
            return;
        }

        EnsurePlayerReference();
        if (player == null) return;

        // Keep camera pivot locked strictly to the player's position and world orbit rotation.
        // Doing this ALWAYS prevents player rotation from spinning or glitching the camera.
        transform.position = player.position + Vector3.up * 1.5f;
        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);

        // When in interaction zone or microscope view, cursor is freed for UI button clicks:
        // do not read mouse delta to rotate the camera, and do not auto-relock cursor on click
        if (LabSetup.MicroscopeInteraction.IsPlayerInZoneOrMicroscope())
        {
            return;
        }

        float mouseX = 0f;
        float mouseY = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue() * 0.1f;
            mouseX = delta.x * mouseSensitivity;
            mouseY = delta.y * mouseSensitivity;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Mathf.Approximately(mouseX, 0f) && Mathf.Approximately(mouseY, 0f))
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }
#endif

        rotationX += mouseX;
        rotationY -= mouseY;

        rotationY = Mathf.Clamp(rotationY, minYAngle, maxYAngle);

        // Apply updated orientation
        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
    }

    void OnDisable()
    {
        UnlockCursor();
    }
}