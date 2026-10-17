using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LabSetup
{
    /// <summary>
    /// Handles player interaction with the microscope workstation:
    /// - Detects when the player enters/exits the Interaction Zone.
    /// - Displays a "Use Microscope" button when in the zone.
    /// - Switches to microscopeCamera upon button click (or pressing [E]).
    /// - Displays an "Exit Microscope" button while in microscope view.
    /// - Switches back to Main Camera upon exit button click (or pressing [ESC] / [E]).
    /// </summary>
    public class MicroscopeInteraction : MonoBehaviour
    {
        public static MicroscopeInteraction Instance { get; private set; }

        [Header("Cameras")]
        public Camera mainCamera;
        public Camera microscopeCamera;
        public CameraSwitcher cameraSwitcher;

        [Header("State")]
        public bool isPlayerInZone = false;
        public bool isMicroscopeViewActive = false;

        [Header("Player Reference")]
        public Transform playerTransform;

        [Header("UI Customization")]
        public string useButtonText = "Use Microscope";
        public string exitButtonText = "Exit Microscope";

        private Texture2D btnNormalTex;
        private Texture2D btnHoverTex;
        private Texture2D exitNormalTex;
        private Texture2D exitHoverTex;
        private GUIStyle useButtonStyle;
        private GUIStyle exitButtonStyle;
        private bool stylesInitialized = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            FindReferences();
        }

        public static bool IsPlayerInZoneOrMicroscope()
        {
            if (Instance == null) return false;
            return Instance.isPlayerInZone || Instance.isMicroscopeViewActive;
        }

        private void FindReferences()
        {
            if (cameraSwitcher == null)
            {
                cameraSwitcher = FindAnyObjectByType<CameraSwitcher>();
            }

            if (mainCamera == null)
            {
                var mainGo = GameObject.Find("Main Camera");
                if (mainGo != null) mainCamera = mainGo.GetComponent<Camera>();
                if (mainCamera == null) mainCamera = Camera.main;
            }

            if (microscopeCamera == null)
            {
                var microGo = GameObject.Find("microscopeCamera");
                if (microGo != null) microscopeCamera = microGo.GetComponent<Camera>();
            }

            if (playerTransform == null)
            {
                var player = FindAnyObjectByType<PlayerMovement>();
                if (player != null) playerTransform = player.transform;
            }

            // Sync initial state
            if (microscopeCamera != null)
            {
                isMicroscopeViewActive = microscopeCamera.gameObject.activeInHierarchy && microscopeCamera.enabled;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsPlayer(other))
            {
                SetPlayerInZone(true);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (IsPlayer(other) && !isPlayerInZone)
            {
                SetPlayerInZone(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsPlayer(other))
            {
                SetPlayerInZone(false);
            }
        }

        private bool IsPlayer(Collider other)
        {
            if (other.CompareTag("Player")) return true;
            if (other.name.Equals("Player", System.StringComparison.OrdinalIgnoreCase)) return true;
            if (other.GetComponentInParent<PlayerMovement>() != null) return true;
            return false;
        }

        private void SetPlayerInZone(bool inZone)
        {
            isPlayerInZone = inZone;

            if (isPlayerInZone)
            {
                // Unlock cursor so user can freely click the on-screen button
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // If exiting zone while not in microscope view, re-lock cursor for third-person camera
                if (!isMicroscopeViewActive)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        private void Update()
        {
            // Fallback proximity check in case physics trigger missed
            CheckBoundsProximity();

            // Synchronize active view state
            if (microscopeCamera != null)
            {
                isMicroscopeViewActive = microscopeCamera.gameObject.activeInHierarchy && microscopeCamera.enabled;
            }

            // Handle keyboard shortcuts
            bool ePressed = false;
            bool escPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame) ePressed = true;
                if (Keyboard.current.escapeKey.wasPressedThisFrame) escPressed = true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.E)) ePressed = true;
            if (Input.GetKeyDown(KeyCode.Escape)) escPressed = true;
#endif

            if (isPlayerInZone && !isMicroscopeViewActive)
            {
                if (ePressed)
                {
                    EnterMicroscopeView();
                }
            }
            else if (isMicroscopeViewActive)
            {
                if (escPressed || ePressed)
                {
                    ExitMicroscopeView();
                }
            }
        }

        private void CheckBoundsProximity()
        {
            var col = GetComponent<Collider>();
            if (col != null && playerTransform != null)
            {
                bool inside = col.bounds.Contains(playerTransform.position);
                if (inside && !isPlayerInZone)
                {
                    SetPlayerInZone(true);
                }
                else if (!inside && isPlayerInZone)
                {
                    // Verify distance from collider center
                    float dist = Vector3.Distance(playerTransform.position, col.bounds.center);
                    if (dist > col.bounds.extents.magnitude + 0.3f)
                    {
                        SetPlayerInZone(false);
                    }
                }
            }
        }

        public void EnterMicroscopeView()
        {
            FindReferences();
            isMicroscopeViewActive = true;

            if (cameraSwitcher != null)
            {
                cameraSwitcher.SwitchToCamera(1); // 1 = microscopeCamera
            }
            else
            {
                if (mainCamera != null)
                {
                    mainCamera.gameObject.SetActive(false);
                    mainCamera.enabled = false;
                }
                if (microscopeCamera != null)
                {
                    microscopeCamera.gameObject.SetActive(true);
                    microscopeCamera.enabled = true;
                }
            }

            // Keep cursor free and visible for eyepiece interaction & exit button
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ExitMicroscopeView()
        {
            FindReferences();
            isMicroscopeViewActive = false;

            if (cameraSwitcher != null)
            {
                cameraSwitcher.SwitchToCamera(0); // 0 = Main Camera
            }
            else
            {
                if (microscopeCamera != null)
                {
                    microscopeCamera.gameObject.SetActive(false);
                    microscopeCamera.enabled = false;
                }
                if (mainCamera != null)
                {
                    mainCamera.gameObject.SetActive(true);
                    mainCamera.enabled = true;
                }
            }

            // If still in the interaction zone, keep cursor unlocked for "Use Microscope" button;
            // otherwise lock cursor for third-person gameplay
            if (isPlayerInZone)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            btnNormalTex = MakeTexture(2, 2, new Color(0.08f, 0.42f, 0.72f, 0.95f)); // Vivid Medical Blue
            btnHoverTex = MakeTexture(2, 2, new Color(0.12f, 0.55f, 0.92f, 1f));

            exitNormalTex = MakeTexture(2, 2, new Color(0.72f, 0.18f, 0.18f, 0.95f)); // Clinical Red / Danger
            exitHoverTex = MakeTexture(2, 2, new Color(0.88f, 0.25f, 0.25f, 1f));

            useButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            useButtonStyle.normal.textColor = Color.white;
            useButtonStyle.hover.textColor = Color.yellow;
            useButtonStyle.normal.background = btnNormalTex;
            useButtonStyle.hover.background = btnHoverTex;

            exitButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            exitButtonStyle.normal.textColor = Color.white;
            exitButtonStyle.hover.textColor = Color.white;
            exitButtonStyle.normal.background = exitNormalTex;
            exitButtonStyle.hover.background = exitHoverTex;

            stylesInitialized = true;
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            Texture2D tex = new Texture2D(width, height);
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private void OnGUI()
        {
            InitStyles();

            // 1. POP-UP "Use Microscope" button when inside Interaction Zone & not currently looking through microscope
            if (isPlayerInZone && !isMicroscopeViewActive)
            {
                float btnWidth = 240f;
                float btnHeight = 52f;
                float btnX = 35f;
                float btnY = 35f;

                Rect rect = new Rect(btnX, btnY, btnWidth, btnHeight);

                // Subtle shadow/outline box behind button
                GUI.color = new Color(0f, 0f, 0f, 0.6f);
                GUI.Box(new Rect(btnX - 4, btnY - 4, btnWidth + 8, btnHeight + 8), GUIContent.none);
                GUI.color = Color.white;

                string buttonLabel = $"🔬  {useButtonText}\n<size=11><color=#E0E0E0>[ Click or Press 'E' ]</color></size>";
                if (GUI.Button(rect, buttonLabel, useButtonStyle))
                {
                    EnterMicroscopeView();
                }
            }

            // 2. "Exit Microscope" button while in microscope view
            if (isMicroscopeViewActive)
            {
                float btnWidth = 230f;
                float btnHeight = 48f;
                // Positioned comfortably in the top-left corner, outside the circular optical iris
                float btnX = 35f;
                float btnY = 35f;

                Rect rect = new Rect(btnX, btnY, btnWidth, btnHeight);

                // Shadow backing
                GUI.color = new Color(0f, 0f, 0f, 0.7f);
                GUI.Box(new Rect(btnX - 3, btnY - 3, btnWidth + 6, btnHeight + 6), GUIContent.none);
                GUI.color = Color.white;

                string exitLabel = $"◀  {exitButtonText}\n<size=11><color=#FFD0D0>[ Click or Press 'ESC' ]</color></size>";
                if (GUI.Button(rect, exitLabel, exitButtonStyle))
                {
                    ExitMicroscopeView();
                }
            }
        }

        private void OnDestroy()
        {
            if (btnNormalTex != null) Destroy(btnNormalTex);
            if (btnHoverTex != null) Destroy(btnHoverTex);
            if (exitNormalTex != null) Destroy(exitNormalTex);
            if (exitHoverTex != null) Destroy(exitHoverTex);
        }
    }
}
