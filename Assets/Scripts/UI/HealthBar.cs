using UnityEngine;
using UnityEngine.UI;

namespace RobotTD.UI
{
    /// <summary>
    /// World-space health bar for enemies and towers.
    /// Follows target and faces the camera.
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider slider;
        [SerializeField] private Image fillImage;
        [SerializeField] private GameObject barContainer;

        [Header("Visual Settings")]
        [SerializeField] private Gradient healthGradient;
        [SerializeField] private bool lookAtCamera = true;
        [SerializeField] private Vector3 offset = Vector3.up * 2f;
        [SerializeField] private bool hideWhenFull = true;

        private Transform target;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
            target = transform.parent;

            if (hideWhenFull && barContainer != null)
            {
                barContainer.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            // Follow target
            if (target != null)
            {
                transform.position = target.position + offset;
            }

            // Face camera
            if (lookAtCamera && mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.forward);
            }
        }

        /// <summary>
        /// Update health bar value
        /// </summary>
        public void SetHealth(float current, float max)
        {
            float percent = Mathf.Clamp01(current / max);

            // Update slider
            if (slider != null)
            {
                slider.value = percent;
            }

            // Update color gradient
            if (fillImage != null && healthGradient != null)
            {
                fillImage.color = healthGradient.Evaluate(percent);
            }

            // Show/hide bar
            if (hideWhenFull && barContainer != null)
            {
                barContainer.SetActive(percent < 1f);
            }
        }

        /// <summary>
        /// Set the target this health bar should follow
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Set the offset from target position
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }

        /// <summary>
        /// Show or hide the health bar
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (barContainer != null)
            {
                barContainer.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }
    }
}
