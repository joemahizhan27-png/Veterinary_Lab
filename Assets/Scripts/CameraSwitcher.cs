using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LabSetup
{
    /// <summary>
    /// Switches between development cameras when the user presses the Spacebar.
    /// Supports both Unity's New Input System and Legacy Input Manager.
    /// </summary>
    public class CameraSwitcher : MonoBehaviour
    {
        [Header("Cameras to Switch Between")]
        [Tooltip("List of cameras to cycle through. If empty, will auto-detect cameras in children or scene.")]
        public List<Camera> cameras = new List<Camera>();

        [Header("Settings")]
        public int currentCameraIndex = 0;
        public bool showOnScreenHint = true;

        private void Awake()
        {
            // Auto-detect cameras if not explicitly assigned in inspector
            if (cameras == null || cameras.Count == 0)
            {
                // First check children
                GetComponentsInChildren(true, cameras);

                // If still empty, search all cameras in the scene
                if (cameras.Count == 0)
                {
                    cameras.AddRange(FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None));
                }
            }

            // Ensure only the initial camera is active
            ApplyActiveCamera();
        }

        private void Update()
        {
            bool switchRequested = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                switchRequested = true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Space))
            {
                switchRequested = true;
            }
#endif

            if (switchRequested)
            {
                SwitchToNextCamera();
            }
        }

        public void SwitchToNextCamera()
        {
            if (cameras == null || cameras.Count == 0) return;

            currentCameraIndex = (currentCameraIndex + 1) % cameras.Count;
            ApplyActiveCamera();
        }

        public void SwitchToCamera(int index)
        {
            if (cameras == null || cameras.Count == 0) return;

            currentCameraIndex = Mathf.Clamp(index, 0, cameras.Count - 1);
            ApplyActiveCamera();
        }

        private void ApplyActiveCamera()
        {
            if (cameras == null || cameras.Count == 0) return;

            for (int i = 0; i < cameras.Count; i++)
            {
                if (cameras[i] == null) continue;

                bool isActive = (i == currentCameraIndex);
                cameras[i].gameObject.SetActive(isActive);
                cameras[i].enabled = isActive;

                // Handle AudioListener on camera to prevent multiple audio listener warnings
                var audioListener = cameras[i].GetComponent<AudioListener>();
                if (audioListener != null)
                {
                    audioListener.enabled = isActive;
                }
            }
        }

        private void OnGUI()
        {
            if (!showOnScreenHint || cameras == null || cameras.Count == 0) return;

            string currentCamName = (currentCameraIndex < cameras.Count && cameras[currentCameraIndex] != null)
                ? cameras[currentCameraIndex].name
                : "Unknown";

            // Box styling for top-left HUD
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.alignment = TextAnchor.UpperLeft;
            boxStyle.fontSize = 14;
            boxStyle.normal.textColor = Color.white;

            Rect rect = new Rect(20, 20, 360, 65);
            GUI.Box(rect, GUIContent.none);

            GUILayout.BeginArea(new Rect(30, 26, 340, 55));
            GUILayout.Label($"<b>Active Camera ({currentCameraIndex + 1}/{cameras.Count}):</b> {currentCamName}", boxStyle);
            GUILayout.Label("Press <b>[SPACE]</b> to switch perspective", boxStyle);
            GUILayout.EndArea();
        }
    }
}
