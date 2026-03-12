using UnityEngine;
using TMPro;
using RobotTD.Core;

namespace RobotTD.UI
{
    /// <summary>
    /// Floating damage numbers that appear when enemies take damage.
    /// Floats upward and fades out over time.
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI text;

        [Header("Animation Settings")]
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private float fadeSpeed = 2f;
        [SerializeField] private AnimationCurve scaleCurve;
        [SerializeField] private Vector3 randomOffset = new Vector3(0.5f, 0.5f, 0.5f);

        private float timer;
        private Vector3 startScale;
        private Color startColor;
        private Camera mainCamera;

        private void Awake()
        {
            startScale = transform.localScale;
            if (text != null)
            {
                startColor = text.color;
            }
            mainCamera = Camera.main;
        }

        /// <summary>
        /// Show damage number at position with specified damage and color
        /// </summary>
        public void Show(float damage, Vector3 position, Color color)
        {
            // Add random offset for multiple hits
            Vector3 randomPos = new Vector3(
                Random.Range(-randomOffset.x, randomOffset.x),
                Random.Range(0, randomOffset.y),
                Random.Range(-randomOffset.z, randomOffset.z)
            );
            
            transform.position = position + randomPos;
            timer = 0f;

            // Set text
            if (text != null)
            {
                text.text = damage >= 1 ? $"{damage:F0}" : $"{damage:F1}";
                text.color = color;
                startColor = color;
            }

            transform.localScale = startScale;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Show critical hit damage (larger and different color)
        /// </summary>
        public void ShowCritical(float damage, Vector3 position)
        {
            Show(damage, position, Color.yellow);
            transform.localScale = startScale * 1.5f;
        }

        private void Update()
        {
            timer += Time.deltaTime;

            // Float upward
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            // Face camera
            if (mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.forward);
            }

            // Scale animation
            if (scaleCurve != null)
            {
                float t = timer / lifetime;
                float scale = scaleCurve.Evaluate(t);
                transform.localScale = startScale * scale;
            }

            // Fade out
            if (text != null)
            {
                float alpha = Mathf.Lerp(startColor.a, 0f, timer / lifetime);
                text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            // Return to pool or destroy when done
            if (timer >= lifetime)
            {
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            var pooled = GetComponent<PooledObject>();
            if (pooled != null)
            {
                pooled.ReturnToPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Reset for pooling
        /// </summary>
        public void OnSpawnFromPool()
        {
            timer = 0f;
            transform.localScale = startScale;
            if (text != null)
            {
                text.color = startColor;
            }
        }
    }
}
