using UnityEngine;
using UnityEngine.UI;

namespace LabSetup
{
    /// <summary>
    /// Creates a circular eyepiece aperture mask over the screen for the microscope camera.
    /// Everything outside the circular viewing area is solid dark/black, replicating an optical microscope eyepiece.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class MicroscopeEyepieceOverlay : MonoBehaviour
    {
        [Header("Aperture Settings")]
        [Range(0.2f, 0.95f)]
        public float apertureDiameterFraction = 0.75f; // Diameter relative to screen height
        public Color maskColor = new Color(0.02f, 0.02f, 0.025f, 1f); // Deep clinical matte black
        [Range(0.001f, 0.05f)]
        public float edgeSoftness = 0.008f; // Soft optical field stop edge

        [Header("Eyepiece Details")]
        public bool showFieldStopRing = true;
        public Color ringColor = new Color(0.08f, 0.08f, 0.09f, 1f);

        private Texture2D maskTexture;
        private Canvas overlayCanvas;
        private RawImage maskImage;
        private Camera targetCamera;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            GenerateMaskTexture();
            BuildOverlayUI();
        }

        private void OnValidate()
        {
            if (maskTexture != null)
            {
                GenerateMaskTexture();
            }
        }

        private void OnEnable()
        {
            if (overlayCanvas != null)
            {
                overlayCanvas.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            if (overlayCanvas != null)
            {
                overlayCanvas.gameObject.SetActive(false);
            }
        }

        private void GenerateMaskTexture()
        {
            int size = 1024;
            if (maskTexture == null || maskTexture.width != size)
            {
                maskTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                maskTexture.wrapMode = TextureWrapMode.Clamp;
                maskTexture.filterMode = FilterMode.Bilinear;
            }

            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float radius = center * 0.92f;
            float invRadius = 1.0f / radius;
            float softness = edgeSoftness * size;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist < radius - softness)
                    {
                        // Completely transparent inside the circular viewing aperture
                        pixels[y * size + x] = Color.clear;
                    }
                    else if (dist > radius + softness)
                    {
                        // Completely solid dark outside the aperture
                        pixels[y * size + x] = maskColor;
                    }
                    else
                    {
                        // Smoothly anti-aliased optical boundary
                        float t = Mathf.SmoothStep(radius - softness, radius + softness, dist);
                        Color c = Color.Lerp(Color.clear, maskColor, t);
                        pixels[y * size + x] = c;
                    }
                }
            }

            maskTexture.SetPixels(pixels);
            maskTexture.Apply();

            if (maskImage != null)
            {
                maskImage.texture = maskTexture;
            }
        }

        private void BuildOverlayUI()
        {
            // Find or create UI Canvas for the overlay
            var existingCanvas = transform.GetComponentInChildren<Canvas>(true);
            if (existingCanvas != null)
            {
                overlayCanvas = existingCanvas;
                maskImage = overlayCanvas.GetComponentInChildren<RawImage>(true);
                if (maskImage != null)
                {
                    maskImage.texture = maskTexture;
                    return;
                }
            }

            var canvasGo = new GameObject("Microscope_Eyepiece_Canvas");
            canvasGo.transform.SetParent(transform, false);

            overlayCanvas = canvasGo.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // Fullscreen Mask Container
            var maskGo = new GameObject("Eyepiece_Aperture_Mask");
            maskGo.transform.SetParent(canvasGo.transform, false);

            var rt = maskGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            // Background letterbox borders (fill screen beyond the square aspect)
            var bgLeft = CreateColorPanel("Letterbox_Left", maskGo.transform, new Vector2(0, 0), new Vector2(0.5f, 1), maskColor);
            var bgRight = CreateColorPanel("Letterbox_Right", maskGo.transform, new Vector2(0.5f, 0), new Vector2(1, 1), maskColor);

            // Center Circular Mask
            var centerImageGo = new GameObject("Circular_Iris");
            centerImageGo.transform.SetParent(maskGo.transform, false);
            var centerRt = centerImageGo.AddComponent<RectTransform>();
            centerRt.anchorMin = new Vector2(0.5f, 0.5f);
            centerRt.anchorMax = new Vector2(0.5f, 0.5f);
            centerRt.pivot = new Vector2(0.5f, 0.5f);
            centerRt.sizeDelta = new Vector2(1080, 1080);

            maskImage = centerImageGo.AddComponent<RawImage>();
            maskImage.texture = maskTexture;
            maskImage.color = Color.white;
            maskImage.raycastTarget = false;

            // Adjust letterbox anchors relative to 1080 square
            UpdateLetterbox(bgLeft, bgRight, centerRt);
        }

        private RectTransform CreateColorPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return rt;
        }

        private void UpdateLetterbox(RectTransform left, RectTransform right, RectTransform center)
        {
            left.anchorMin = new Vector2(0, 0);
            left.anchorMax = new Vector2(0.5f, 1);
            left.offsetMax = new Vector2(-530, 0);

            right.anchorMin = new Vector2(0.5f, 0);
            right.anchorMax = new Vector2(1, 1);
            right.offsetMin = new Vector2(530, 0);
        }

        private void OnGUI()
        {
            // Safety fallback if Canvas is not rendered
            if (overlayCanvas == null || !overlayCanvas.gameObject.activeInHierarchy)
            {
                DrawGUIFallback();
            }
        }

        private void DrawGUIFallback()
        {
            if (maskTexture == null) return;

            float screenW = Screen.width;
            float screenH = Screen.height;
            float size = Mathf.Min(screenW, screenH) * 1.05f;
            float x = (screenW - size) * 0.5f;
            float y = (screenH - size) * 0.5f;

            // Black side borders
            GUI.color = maskColor;
            if (x > 0)
            {
                GUI.DrawTexture(new Rect(0, 0, x + 2, screenH), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(screenW - x - 2, 0, x + 2, screenH), Texture2D.whiteTexture);
            }
            if (y > 0)
            {
                GUI.DrawTexture(new Rect(0, 0, screenW, y + 2), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(0, screenH - y - 2, screenW, y + 2), Texture2D.whiteTexture);
            }

            // Circular mask
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(x, y, size, size), maskTexture);
        }
    }
}
