using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RobotTD.UI
{
    /// <summary>
    /// Reusable animated progress bar component.
    /// Can show percentage text and has smooth fill animation.
    /// </summary>
    public class ProgressBar : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider slider;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI percentText;
        [SerializeField] private Gradient colorGradient;

        [Header("Animation")]
        [SerializeField] private bool smoothFill = true;
        [SerializeField] private float fillSpeed = 5f;
        [SerializeField] private bool showPercentage = true;
        [SerializeField] private string percentageFormat = "{0:0}%";

        [Header("Pulse Effect")]
        [SerializeField] private bool pulseWhenLow = false;
        [SerializeField] private float lowThreshold = 0.2f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.3f;

        private float targetValue = 1f;
        private float currentValue = 1f;

        private void Update()
        {
            if (smoothFill && slider != null)
            {
                // Smooth lerp to target
                currentValue = Mathf.MoveTowards(currentValue, targetValue, fillSpeed * Time.deltaTime);
                slider.value = currentValue;

                // Update color
                if (fillImage != null && colorGradient != null)
                {
                    Color baseColor = colorGradient.Evaluate(currentValue);
                    
                    // Pulse effect when low
                    if (pulseWhenLow && currentValue < lowThreshold)
                    {
                        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + 1f;
                        fillImage.color = baseColor * pulse;
                    }
                    else
                    {
                        fillImage.color = baseColor;
                    }
                }
            }

            // Update percentage text
            if (showPercentage && percentText != null)
            {
                percentText.text = string.Format(percentageFormat, currentValue * 100f);
            }
        }

        /// <summary>
        /// Set progress value (0-1)
        /// </summary>
        public void SetProgress(float value)
        {
            targetValue = Mathf.Clamp01(value);

            if (!smoothFill && slider != null)
            {
                currentValue = targetValue;
                slider.value = currentValue;

                if (fillImage != null && colorGradient != null)
                {
                    fillImage.color = colorGradient.Evaluate(currentValue);
                }
            }

            if (showPercentage && percentText != null)
            {
                percentText.text = string.Format(percentageFormat, targetValue * 100f);
            }
        }

        /// <summary>
        /// Set progress with min/max values
        /// </summary>
        public void SetProgress(float current, float max)
        {
            if (max <= 0f) max = 1f;
            SetProgress(current / max);
        }

        /// <summary>
        /// Set progress immediately without animation
        /// </summary>
        public void SetProgressImmediate(float value)
        {
            targetValue = Mathf.Clamp01(value);
            currentValue = targetValue;

            if (slider != null)
            {
                slider.value = currentValue;
            }

            if (fillImage != null && colorGradient != null)
            {
                fillImage.color = colorGradient.Evaluate(currentValue);
            }

            if (showPercentage && percentText != null)
            {
                percentText.text = string.Format(percentageFormat, currentValue * 100f);
            }
        }

        /// <summary>
        /// Set the fill color directly
        /// </summary>
        public void SetColor(Color color)
        {
            if (fillImage != null)
            {
                fillImage.color = color;
            }
        }

        /// <summary>
        /// Get current progress value (0-1)
        /// </summary>
        public float GetProgress()
        {
            return currentValue;
        }

        /// <summary>
        /// Check if progress bar is full
        /// </summary>
        public bool IsFull()
        {
            return currentValue >= 0.99f;
        }

        /// <summary>
        /// Check if progress bar is empty
        /// </summary>
        public bool IsEmpty()
        {
            return currentValue <= 0.01f;
        }

        /// <summary>
        /// Reset to empty
        /// </summary>
        public void Reset()
        {
            SetProgressImmediate(0f);
        }

        /// <summary>
        /// Fill to 100%
        /// </summary>
        public void Fill()
        {
            SetProgressImmediate(1f);
        }
    }
}
