using UnityEngine;
using RobotTD.Core;
using RobotTD.Enemies;

namespace RobotTD.Projectiles
{
    /// <summary>
    /// Base projectile class. Handles movement and impact.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] protected float lifetime = 5f;
        [SerializeField] protected bool useGravity = false;
        [SerializeField] protected float arcHeight = 0f;
        [SerializeField] protected ParticleSystem trailEffect;
        [SerializeField] protected GameObject impactEffectPrefab;
        [SerializeField] protected AudioClip impactSound;

        // Runtime state
        protected Enemy target;
        protected Vector3 targetLastPosition;
        protected float damage;
        protected float speed;
        protected DamageType damageType;
        protected float aliveTime;
        protected Vector3 startPosition;
        protected float journeyLength;

        public virtual void Initialize(Enemy targetEnemy, float damageAmount, float moveSpeed, DamageType type = DamageType.Physical)
        {
            target = targetEnemy;
            damage = damageAmount;
            speed = moveSpeed;
            damageType = type;
            aliveTime = 0f;
            startPosition = transform.position;

            if (target != null)
            {
                targetLastPosition = target.transform.position;
                journeyLength = Vector3.Distance(startPosition, targetLastPosition);
            }

            // Enable trail
            if (trailEffect != null)
            {
                trailEffect.Play();
            }
        }

        protected virtual void Update()
        {
            aliveTime += Time.deltaTime;

            // Self-destruct after lifetime
            if (aliveTime >= lifetime)
            {
                Destroy();
                return;
            }

            MoveTowardsTarget();
        }

        protected virtual void MoveTowardsTarget()
        {
            // Update target position if target is still alive
            if (target != null && !target.IsDead)
            {
                targetLastPosition = target.transform.position;
            }

            // Calculate movement
            Vector3 direction = (targetLastPosition - transform.position).normalized;
            float distanceThisFrame = speed * Time.deltaTime;

            // Apply arc if specified
            if (arcHeight > 0)
            {
                float progress = 1f - (Vector3.Distance(transform.position, targetLastPosition) / journeyLength);
                float arcOffset = Mathf.Sin(progress * Mathf.PI) * arcHeight;
                transform.position = Vector3.MoveTowards(transform.position, targetLastPosition, distanceThisFrame);
                transform.position += Vector3.up * arcOffset * Time.deltaTime * speed;
            }
            else
            {
                transform.position += direction * distanceThisFrame;
            }

            // Rotate towards movement direction
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // Check for impact
            if (Vector3.Distance(transform.position, targetLastPosition) < 0.5f)
            {
                OnImpact();
            }
        }

        protected virtual void OnImpact()
        {
            // Deal damage if target is still valid
            if (target != null && !target.IsDead)
            {
                target.TakeDamage(damage, damageType);
            }

            // Spawn impact effect
            if (impactEffectPrefab != null)
            {
                GameObject effect = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // Play impact sound
            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, transform.position);
            }

            Destroy();
        }

        protected virtual void Destroy()
        {
            // Stop trail
            if (trailEffect != null)
            {
                trailEffect.Stop();
            }

            // Return to pool or destroy
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
    }

    /// <summary>
    /// Rocket projectile with splash damage
    /// </summary>
    public class RocketProjectile : Projectile
    {
        [Header("Rocket Specific")]
        [SerializeField] private float splashRadius = 3f;
        [SerializeField] private float splashDamagePercent = 0.5f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private ParticleSystem rocketTrail;
        [SerializeField] private GameObject explosionPrefab;

        private float actualSplashRadius;
        private float actualSplashDamagePercent;

        public void Initialize(Enemy targetEnemy, float damageAmount, float moveSpeed, 
            float splashRad, float splashDmgPercent, GameObject explosionPref = null)
        {
            base.Initialize(targetEnemy, damageAmount, moveSpeed, DamageType.Physical);
            actualSplashRadius = splashRad;
            actualSplashDamagePercent = splashDmgPercent;
            
            if (explosionPref != null)
            {
                explosionPrefab = explosionPref;
            }
        }

        protected override void OnImpact()
        {
            // Primary target damage
            if (target != null && !target.IsDead)
            {
                target.TakeDamage(damage, damageType);
            }

            // Splash damage to nearby enemies
            Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, actualSplashRadius, enemyLayer);
            
            foreach (var col in nearbyEnemies)
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null && enemy != target && !enemy.IsDead)
                {
                    // Damage falloff based on distance
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    float falloff = 1f - (distance / actualSplashRadius);
                    float splashDamage = damage * actualSplashDamagePercent * falloff;
                    
                    enemy.TakeDamage(splashDamage, damageType);
                }
            }

            // Explosion effect
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 3f);
            }

            // Sound
            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, transform.position, 1f);
            }

            // Screen shake
            CameraShake.Instance?.Shake(0.1f, 0.2f);

            Destroy();
        }
    }

    /// <summary>
    /// Freeze projectile that slows enemies
    /// </summary>
    public class FreezeProjectile : Projectile
    {
        [Header("Freeze Specific")]
        [SerializeField] private ParticleSystem frostEffect;

        private float slowPercent;
        private float slowDuration;

        public void Initialize(Enemy targetEnemy, float damageAmount, float moveSpeed, 
            float slowPct, float slowDur)
        {
            base.Initialize(targetEnemy, damageAmount, moveSpeed, DamageType.Physical);
            slowPercent = slowPct;
            slowDuration = slowDur;
        }

        protected override void OnImpact()
        {
            if (target != null && !target.IsDead)
            {
                target.TakeDamage(damage, damageType);
                target.ApplySlow(slowPercent, slowDuration);
            }

            // Frost effect
            if (frostEffect != null)
            {
                ParticleSystem effect = Instantiate(frostEffect, transform.position, Quaternion.identity);
                Destroy(effect.gameObject, 2f);
            }

            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, transform.position);
            }

            Destroy();
        }
    }

    /// <summary>
    /// Homing missile that tracks target
    /// </summary>
    public class HomingMissile : Projectile
    {
        [Header("Homing Specific")]
        [SerializeField] private float turnSpeed = 5f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float maxSpeed = 20f;

        private float currentSpeed;
        private Vector3 velocity;

        public override void Initialize(Enemy targetEnemy, float damageAmount, float moveSpeed, DamageType type = DamageType.Physical)
        {
            base.Initialize(targetEnemy, damageAmount, moveSpeed, type);
            currentSpeed = moveSpeed * 0.5f;
            velocity = transform.forward * currentSpeed;
        }

        protected override void MoveTowardsTarget()
        {
            // Update target position
            if (target != null && !target.IsDead)
            {
                targetLastPosition = target.transform.position;
            }

            // Calculate direction to target
            Vector3 direction = (targetLastPosition - transform.position).normalized;

            // Smoothly rotate towards target
            velocity = Vector3.RotateTowards(velocity, direction * currentSpeed, turnSpeed * Time.deltaTime, 0f);

            // Accelerate
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
            velocity = velocity.normalized * currentSpeed;

            // Move
            transform.position += velocity * Time.deltaTime;

            // Face movement direction
            if (velocity != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(velocity);
            }

            // Check for impact
            if (Vector3.Distance(transform.position, targetLastPosition) < 1f)
            {
                OnImpact();
            }
        }
    }

    /// <summary>
    /// Utility class for camera shake effect
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private Vector3 originalPosition;
        private float shakeDuration;
        private float shakeIntensity;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (shakeDuration > 0)
            {
                transform.localPosition = originalPosition + Random.insideUnitSphere * shakeIntensity;
                shakeDuration -= Time.deltaTime;

                if (shakeDuration <= 0)
                {
                    transform.localPosition = originalPosition;
                }
            }
        }

        public void Shake(float duration, float intensity)
        {
            if (shakeDuration <= 0)
            {
                originalPosition = transform.localPosition;
            }
            shakeDuration = duration;
            shakeIntensity = intensity;
        }
    }
}
