using UnityEngine;
using RobotTD.Core;
using RobotTD.Enemies;
using RobotTD.Towers;

namespace RobotTD.Projectiles
{
    /// <summary>
    /// Artillery shell projectile with parabolic arc trajectory and splash damage.
    /// Fires toward a position (not tracking) in a realistic ballistic arc.
    /// </summary>
    public class ArtilleryProjectile : MonoBehaviour
    {
        [Header("Artillery Shell Settings")]
        [SerializeField] private float lifetime = 8f;
        [SerializeField] private ParticleSystem trailEffect;
        [SerializeField] private GameObject shellModel;
        [SerializeField] private float rotationSpeed = 360f; // Shell spin during flight
        
        // Impact settings
        private Vector3 targetPosition;
        private float damage;
        private float splashRadius;
        private GameObject impactEffect;
        private AudioClip impactSound;
        
        // Arc trajectory settings
        private Vector3 startPosition;
        private float arcHeight;
        private float flightDuration;
        private float currentFlightTime;
        private bool initialized = false;
        
        /// <summary>
        /// Initialize the artillery shell with target position and arc
        /// </summary>
        public void Initialize(Vector3 target, float damageAmount, float radius, float height, GameObject impact = null, AudioClip sound = null)
        {
            targetPosition = target;
            damage = damageAmount;
            splashRadius = radius;
            arcHeight = height;
            impactEffect = impact;
            impactSound = sound;
            
            startPosition = transform.position;
            currentFlightTime = 0f;
            initialized = true;
            
            // Calculate flight duration based on distance
            float distance = Vector3.Distance(startPosition, targetPosition);
            flightDuration = CalculateFlightDuration(distance);
            
            // Start trail effect
            if (trailEffect != null)
            {
                trailEffect.Play();
            }
        }
        
        private void Update()
        {
            if (!initialized) return;
            
            currentFlightTime += Time.deltaTime;
            
            // Check if projectile exceeded lifetime
            if (currentFlightTime >= lifetime)
            {
                DestroyProjectile();
                return;
            }
            
            // Calculate current position along parabolic arc
            float t = currentFlightTime / flightDuration;
            
            if (t >= 1f)
            {
                // Reached target - detonate
                OnImpact();
                return;
            }
            
            UpdatePosition(t);
            UpdateRotation();
        }
        
        /// <summary>
        /// Calculate realistic flight duration based on distance
        /// </summary>
        private float CalculateFlightDuration(float distance)
        {
            // Longer distances take more time (roughly 0.5-2.5 seconds)
            return Mathf.Lerp(0.5f, 2.5f, distance / 20f);
        }
        
        /// <summary>
        /// Update position along parabolic arc trajectory
        /// </summary>
        private void UpdatePosition(float t)
        {
            // Horizontal movement (linear interpolation)
            Vector3 horizontalPosition = Vector3.Lerp(startPosition, targetPosition, t);
            
            // Vertical arc (parabolic curve)
            // Uses (1 - (2t - 1)^2) formula for smooth arc
            float arcProgress = 1f - Mathf.Pow(2f * t - 1f, 2f);
            float currentHeight = arcProgress * arcHeight;
            
            // Combine horizontal position with vertical arc
            transform.position = new Vector3(
                horizontalPosition.x,
                horizontalPosition.y + currentHeight,
                horizontalPosition.z
            );
        }
        
        /// <summary>
        /// Rotate shell during flight for visual effect
        /// </summary>
        private void UpdateRotation()
        {
            if (shellModel != null)
            {
                shellModel.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.Self);
            }
            else
            {
                // Rotate main object if no separate shell model
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.Self);
            }
            
            // Always point in direction of travel
            Vector3 velocity = (targetPosition - transform.position).normalized;
            if (velocity != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(velocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        
        /// <summary>
        /// Handle impact and deal splash damage
        /// </summary>
        private void OnImpact()
        {
            // Snap to target position for accuracy
            transform.position = targetPosition;
            
            // Deal splash damage to all enemies in radius
            ArtilleryBot.DealSplashDamage(targetPosition, damage, splashRadius);
            
            // Spawn explosion effect
            if (impactEffect != null)
            {
                GameObject effect = Instantiate(impactEffect, targetPosition, Quaternion.identity);
                Destroy(effect, 3f);
            }
            
            // Play impact sound
            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, targetPosition);
            }
            
            // Screen shake for heavy impact feel (optional)
            CameraShake();
            
            DestroyProjectile();
        }
        
        /// <summary>
        /// Optional camera shake for dramatic impact
        /// </summary>
        private void CameraShake()
        {
            // Check if camera controller exists and supports shake
            var cameraController = Camera.main?.GetComponent<Map.CameraController>();
            if (cameraController != null)
            {
                // Assuming CameraController has a Shake method
                // If not, this will fail silently
                cameraController.SendMessage("Shake", 0.3f, SendMessageOptions.DontRequireReceiver);
            }
        }
        
        /// <summary>
        /// Clean up projectile
        /// </summary>
        private void DestroyProjectile()
        {
            // Stop trail effect
            if (trailEffect != null)
            {
                trailEffect.Stop();
                trailEffect.transform.SetParent(null); // Detach so it completes
                Destroy(trailEffect.gameObject, 2f);
            }
            
            // Check for object pooling
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
        
        // ── Gizmos ────────────────────────────────────────────────────────────
        
        private void OnDrawGizmos()
        {
            // Draw trajectory prediction in editor
            if (!Application.isPlaying && initialized)
            {
                DrawArcGizmo();
            }
        }
        
        private void DrawArcGizmo()
        {
            Gizmos.color = Color.yellow;
            
            Vector3 previousPoint = startPosition;
            int segments = 20;
            
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                
                // Calculate point on arc
                Vector3 horizontalPos = Vector3.Lerp(startPosition, targetPosition, t);
                float arcProgress = 1f - Mathf.Pow(2f * t - 1f, 2f);
                float height = arcProgress * arcHeight;
                
                Vector3 point = new Vector3(
                    horizontalPos.x,
                    horizontalPos.y + height,
                    horizontalPos.z
                );
                
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
            
            // Draw splash radius at impact
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            DrawCircle(targetPosition, splashRadius, 16);
        }
        
        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);
            
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0,
                    Mathf.Sin(angle) * radius
                );
                
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}
