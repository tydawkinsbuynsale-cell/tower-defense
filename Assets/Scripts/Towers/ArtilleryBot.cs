using UnityEngine;
using RobotTD.Core;
using RobotTD.Projectiles;

namespace RobotTD.Towers
{
    /// <summary>
    /// Artillery Bot - Long-range siege tower with arc projectiles.
    /// Fires shells in a high arc that deal splash damage on impact.
    /// High damage, slow fire rate, extreme range.
    /// </summary>
    public class ArtilleryBot : Tower
    {
        [Header("Artillery Specific")]
        [SerializeField] private float splashRadius = 2.5f;
        [SerializeField] private float arcHeight = 5f; // Height of projectile arc
        [SerializeField] private GameObject impactEffect;
        [SerializeField] private float minRange = 3f; // Cannot hit enemies too close
        
        [Header("Visual Effects")]
        [SerializeField] private Transform barrelTransform;
        [SerializeField] private float barrelRotationSpeed = 3f;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip impactSound;
        
        private Vector3 targetPosition;
        
        protected override void Update()
        {
            base.Update();
            
            // Aim barrel at target
            if (currentTarget != null && barrelTransform != null)
            {
                AimBarrelAtTarget();
            }
        }
        
        /// <summary>
        /// Override targeting to respect minimum range
        /// </summary>
        protected override void FindTarget()
        {
            currentTarget = null;
            float closestDist = float.MaxValue;
            
            foreach (var enemy in Enemies.Enemy.AllEnemies)
            {
                if (enemy == null || enemy.IsDead) continue;
                
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                
                // Check if within range AND outside minimum range
                if (dist <= Range && dist >= minRange && dist < closestDist)
                {
                    closestDist = dist;
                    currentTarget = enemy;
                }
            }
        }
        
        protected override void SpawnProjectile()
        {
            if (currentTarget == null) return;
            
            // Store target position at fire time (for arc calculation)
            targetPosition = currentTarget.transform.position;
            
            // Spawn artillery shell projectile
            if (projectilePrefab != null && firePoint != null)
            {
                GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                ArtilleryProjectile artilleryProj = proj.GetComponent<ArtilleryProjectile>();
                
                if (artilleryProj != null)
                {
                    artilleryProj.Initialize(targetPosition, CurrentDamage, splashRadius, arcHeight, impactEffect, impactSound);
                }
                else
                {
                    // Fallback to standard projectile
                    Projectile standardProj = proj.GetComponent<Projectile>();
                    if (standardProj != null)
                    {
                        standardProj.Initialize(currentTarget, CurrentDamage, DamageType.Physical);
                    }
                }
            }
            
            // Play muzzle flash
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }
            
            // Play fire sound
            if (fireSound != null && AudioSource != null)
            {
                AudioSource.PlayOneShot(fireSound);
            }
        }
        
        /// <summary>
        /// Aim the barrel at the current target with elevation for arc
        /// </summary>
        private void AimBarrelAtTarget()
        {
            if (currentTarget == null || barrelTransform == null) return;
            
            Vector3 direction = currentTarget.transform.position - barrelTransform.position;
            float distance = direction.magnitude;
            
            // Calculate elevation angle for arc (simple approximation)
            float angle = CalculateArcAngle(distance);
            
            // Horizontal rotation
            Quaternion horizontalRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            
            // Add elevation
            Quaternion targetRotation = horizontalRotation * Quaternion.Euler(-angle, 0, 0);
            
            // Smooth rotation
            barrelTransform.rotation = Quaternion.Slerp(
                barrelTransform.rotation,
                targetRotation,
                Time.deltaTime * barrelRotationSpeed
            );
        }
        
        /// <summary>
        /// Calculate barrel elevation angle based on distance
        /// </summary>
        private float CalculateArcAngle(float distance)
        {
            // Higher arc for longer distances
            float normalizedDist = Mathf.Clamp01(distance / Range);
            return Mathf.Lerp(30f, 60f, normalizedDist); // 30-60 degree arc
        }
        
        /// <summary>
        /// Override range indicator to show minimum range
        /// </summary>
        protected override void SetupRangeIndicator()
        {
            base.SetupRangeIndicator();
            
            // TODO: Add visual for minimum range (inner circle)
            // This would require modifying the range indicator prefab
        }
        
        // ── Splash Damage Helper ─────────────────────────────────────────────
        
        /// <summary>
        /// Static helper for dealing splash damage at impact point.
        /// Called by ArtilleryProjectile on impact.
        /// </summary>
        public static void DealSplashDamage(Vector3 impactPoint, float damage, float radius)
        {
            if (radius <= 0) return;
            
            Collider[] hitColliders = Physics.OverlapSphere(impactPoint, radius);
            
            foreach (var collider in hitColliders)
            {
                Enemies.Enemy enemy = collider.GetComponent<Enemies.Enemy>();
                if (enemy != null && !enemy.IsDead)
                {
                    // Calculate damage falloff based on distance
                    float distance = Vector3.Distance(impactPoint, enemy.transform.position);
                    float damageFalloff = 1f - (distance / radius);
                    damageFalloff = Mathf.Clamp01(damageFalloff);
                    
                    float finalDamage = damage * damageFalloff;
                    enemy.TakeDamage(finalDamage, DamageType.Physical);
                }
            }
        }
        
        // ── Gizmos ────────────────────────────────────────────────────────────
        
        private void OnDrawGizmosSelected()
        {
            // Draw range circles
            Gizmos.color = Color.red;
            DrawCircle(transform.position, Range, 32);
            
            // Draw minimum range
            Gizmos.color = Color.yellow;
            DrawCircle(transform.position, minRange, 32);
            
            // Draw splash radius at target
            if (currentTarget != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                DrawCircle(currentTarget.transform.position, splashRadius, 16);
            }
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
