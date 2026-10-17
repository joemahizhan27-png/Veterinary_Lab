using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField] float movementSpeed = 6f;
    [Tooltip("If true, movement follows the camera's viewing angle. If false, moves along world axes.")]
    [SerializeField] bool cameraRelative = true;

    void Awake()
    {
        EnsureRigidbody();
    }

    void Start()
    {
        EnsureRigidbody();
    }

    void EnsureRigidbody()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void FixedUpdate()
    {
        EnsureRigidbody();
        if (rb == null) return;

        // If microscope camera is currently active, don't move the player
        if (IsMicroscopeViewActive())
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        float horizontalInput = 0f;
        float verticalInput = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            // Arrow keys (from user's code) + WASD
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                horizontalInput -= 1f;

            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                horizontalInput += 1f;

            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
                verticalInput += 1f;

            if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
                verticalInput -= 1f;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Mathf.Approximately(horizontalInput, 0f) && Mathf.Approximately(verticalInput, 0f))
        {
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                horizontalInput -= 1f;

            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                horizontalInput += 1f;

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                verticalInput += 1f;

            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                verticalInput -= 1f;
        }
#endif

        Vector3 moveInput = new Vector3(horizontalInput, 0f, verticalInput);
        Vector3 movement = Vector3.zero;

        if (moveInput.sqrMagnitude > 0.0001f)
        {
            moveInput.Normalize();

            if (cameraRelative && Camera.main != null)
            {
                Vector3 camForward = Camera.main.transform.forward;
                camForward.y = 0f;
                camForward.Normalize();

                Vector3 camRight = Camera.main.transform.right;
                camRight.y = 0f;
                camRight.Normalize();

                movement = (camForward * moveInput.z + camRight * moveInput.x).normalized * movementSpeed;
            }
            else
            {
                movement = moveInput * movementSpeed;
            }

            // Smoothly rotate character toward the direction of travel
            if (movement.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(new Vector3(movement.x, 0f, movement.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.fixedDeltaTime);
            }
        }

        rb.linearVelocity = new Vector3(
            movement.x,
            rb.linearVelocity.y,
            movement.z
        );
    }

    private bool IsMicroscopeViewActive()
    {
        var microscopeCam = GameObject.Find("microscopeCamera");
        if (microscopeCam != null && microscopeCam.activeInHierarchy)
        {
            var cam = microscopeCam.GetComponent<Camera>();
            return cam != null && cam.enabled;
        }
        return false;
    }
}