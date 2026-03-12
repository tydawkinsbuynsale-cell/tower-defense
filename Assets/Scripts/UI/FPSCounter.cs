using UnityEngine;
using TMPro;

namespace RobotTD.UI
{
    /// <summary>
    /// FPS counter for performance monitoring during development.
    /// Shows FPS with color coding: Green (60+), Yellow (30-60), Red (<30)
    /// </summary>
    public class FPSCounter : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        [SerializeField] private float updateInterval = 0.5f;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private GameObject fpsPanel;

        [Header("Colors")]
        [SerializeField] private Color goodColor = Color.green;
        [SerializeField] private Color okColor = Color.yellow;
        [SerializeField] private Color badColor = Color.red;

        private float deltaTime = 0f;
        private float updateTimer = 0f;
        private int frameCount = 0;
        private float fps = 0f;

        private void Start()
        {
            if (fpsPanel != null)
            {
                fpsPanel.SetActive(showOnStart);
            }
        }

        private void Update()
        {
            // Toggle visibility
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleDisplay();
            }

            if (fpsPanel != null && !fpsPanel.activeSelf)
            {
                return;
            }

            // Calculate FPS
            deltaTime += Time.unscaledDeltaTime;
            frameCount++;
            updateTimer += Time.unscaledDeltaTime;

            if (updateTimer >= updateInterval)
            {
                fps = frameCount / deltaTime;
                deltaTime = 0f;
                frameCount = 0;
                updateTimer = 0f;

                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            if (fpsText == null) return;

            // Display FPS
            fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";

            // Color code based on performance
            if (fps >= 60f)
            {
                fpsText.color = goodColor;
            }
            else if (fps >= 30f)
            {
                fpsText.color = okColor;
            }
            else
            {
                fpsText.color = badColor;
            }
        }

        public void ToggleDisplay()
        {
            if (fpsPanel != null)
            {
                fpsPanel.SetActive(!fpsPanel.activeSelf);
            }
        }

        public void Show()
        {
            if (fpsPanel != null)
            {
                fpsPanel.SetActive(true);
            }
        }

        public void Hide()
        {
            if (fpsPanel != null)
            {
                fpsPanel.SetActive(false);
            }
        }
    }
}
