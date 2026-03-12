using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using RobotTD.UI;

namespace RobotTD.Enemies
{
    /// <summary>
    /// Base enemy class. All enemy types inherit from this.
    /// Handles path following, health, and status effects.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] protected EnemyData enemyData;

        [Header("References")]
        [SerializeField] protected Transform modelTransform;
        [SerializeField] protected HealthBar healthBar;
        [SerializeField] protected GameObject deathEffectPrefab;
        [SerializeField] protected Renderer[] bodyRenderers;

        // Runtime stats (can be modified by wave scaling)
        protected float maxHealth;
        protected float currentHealth;
        protected float moveSpeed;
        protected int reward;

        // Path following
        protected Transform[] waypoints;
        protected int currentWaypointIndex;
        protected float pathProgress; // 0-1 progress along entire path
        protected float distanceTraveled;
        protected float totalPathLength;

        // State
        public bool IsDead { get; protected set; }
        public bool HasReachedEnd { get; protected set; }
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => currentHealth / maxHealth;
        public float PathProgress => pathProgress;

        // Status effects
        protected float slowMultiplier = 1f;
        protected float slowDuration;
        protected bool isSlowed => slowMultiplier < 1f;
        protected float burnDamage;
        protected float burnDuration;
        protected bool isBurning => burnDuration > 0;
        protected bool isStunned;
        protected float stunDuration;

        // Events
        public UnityEvent<float> OnDamageTaken;
        public UnityEvent OnDied;
        public UnityEvent OnReachedEnd;

        protected virtual void Awake()
        {
            OnDamageTaken ??= new UnityEvent<float>();
            OnDied ??= new UnityEvent();
            OnReachedEnd ??= new UnityEvent();
        }

        /// <summary>
        /// Initialize the enemy with wave scaling
        /// </summary>
        public virtual void Initialize(Transform[] path, float healthMultiplier = 1f, float speedMultiplier = 1f)
        {
            waypoints = path;
            currentWaypointIndex = 0;
            distanceTraveled = 0f;
            IsDead = false;
            HasReachedEnd = false;

            // Apply stats with scaling
            maxHealth = enemyData.baseHealth * healthMultiplier;
            currentHealth = maxHealth;
            moveSpeed = enemyData.baseMoveSpeed * speedMultiplier;
            reward = enemyData.baseReward;

            // Reset status effects
            slowMultiplier = 1f;
            slowDuration = 0f;
            burnDuration = 0f;
            stunDuration = 0f;
            isStunned = false;

            // Calculate total path length
            CalculatePathLength();

            // Position at start
            if (waypoints != null && waypoints.Length > 0)
            {
                transform.position = waypoints[0].position;
            }

            // Update health bar
            UpdateHealthBar();

            // Reset visuals
            ResetVisuals();
        }

        protected virtual void Update()
        {
            if (IsDead || HasReachedEnd) return;

            // Process status effects
            ProcessStatusEffects();

            // Move along path (if not stunned)
            if (!isStunned)
            {
                MoveAlongPath();
            }

            // Update path progress
            pathProgress = totalPathLength > 0 ? distanceTraveled / totalPathLength : 0f;
        }

        #region Movement

        protected virtual void MoveAlongPath()
        {
            if (waypoints == null || currentWaypointIndex >= waypoints.Length) return;

            Transform targetWaypoint = waypoints[currentWaypointIndex];
            Vector3 direction = (targetWaypoint.position - transform.position).normalized;

            // Apply slow effect
            float effectiveSpeed = moveSpeed * slowMultiplier * Time.deltaTime;

            // Move towards waypoint
            transform.position += direction * effectiveSpeed;
            distanceTraveled += effectiveSpeed;

            // Rotate to face movement direction
            if (direction != Vector3.zero && modelTransform != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                modelTransform.rotation = Quaternion.Slerp(
                    modelTransform.rotation, 
                    targetRotation, 
                    10f * Time.deltaTime
                );
            }

            // Check if reached waypoint
            if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= waypoints.Length)
                {
                    ReachEnd();
                }
            }
        }

        protected void CalculatePathLength()
        {
            totalPathLength = 0f;
            if (waypoints == null || waypoints.Length < 2) return;

            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                totalPathLength += Vector3.Distance(
                    waypoints[i].position, 
                    waypoints[i + 1].position
                );
            }
        }

        #endregion

        #region Health & Damage

        /// <summary>
        /// Take damage from a source
        /// </summary>
        public virtual void TakeDamage(float damage, DamageType damageType = DamageType.Physical)
        {
            if (IsDead) return;

            // Apply resistance
            float resistance = GetResistance(damageType);
            float actualDamage = damage * (1f - resistance);

            currentHealth -= actualDamage;
            OnDamageTaken?.Invoke(actualDamage);

            // Visual feedback
            StartCoroutine(DamageFlash());
            SpawnDamageNumber(actualDamage);

            UpdateHealthBar();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual float GetResistance(DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Physical => enemyData.physicalResistance,
                DamageType.Energy => enemyData.energyResistance,
                DamageType.Fire => enemyData.fireResistance,
                DamageType.Electric => enemyData.electricResistance,
                DamageType.Plasma => enemyData.plasmaResistance,
                _ => 0f
            };
        }

        protected virtual void Die()
        {
            IsDead = true;
            OnDied?.Invoke();

            // Award reward
            Core.GameManager.Instance?.AddCredits(reward);
            Core.GameManager.Instance?.AddScore(enemyData.scoreValue);

            // Track kill in save data
            Core.SaveManager.Instance?.AddKills(1);

            // Check if this is a boss
            if (enemyData.enemyType == EnemyTypes.SwarmMotherBoss || 
                enemyData.enemyType == EnemyTypes.ShieldCommanderBoss ||
                enemyData.enemyType == EnemyTypes.TitanBoss)
            {
                Progression.AchievementManager.Instance?.OnBossKilled();
            }

            // Notify wave manager
            Core.WaveManager.Instance?.OnEnemyRemoved();

            // Spawn death effect
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            // Return to pool or destroy
            var pooled = GetComponent<Core.PooledObject>();
            if (pooled != null)
            {
                pooled.ReturnToPool();
            }
            else
            {
                Destroy(gameObject, 0.1f);
            }
        }

        protected void ReachEnd()
        {
            HasReachedEnd = true;
            OnReachedEnd?.Invoke();

            // Damage player
            Core.GameManager.Instance?.LoseLife(enemyData.liveDamage);

            // Notify wave manager
            Core.WaveManager.Instance?.OnEnemyRemoved();

            // Return to pool or destroy
            var pooled = GetComponent<Core.PooledObject>();
            if (pooled != null)
            {
                pooled.ReturnToPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected void UpdateHealthBar()
        {
            if (healthBar != null)
            {
                healthBar.SetHealth(currentHealth, maxHealth);
            }
        }

        #endregion

        #region Status Effects

        /// <summary>
        /// Apply slow effect
        /// </summary>
        public virtual void ApplySlow(float slowPercent, float duration)
        {
            float newMultiplier = 1f - slowPercent;
            
            // Use strongest slow
            if (newMultiplier < slowMultiplier || duration > slowDuration)
            {
                slowMultiplier = newMultiplier;
                slowDuration = duration;
                
                // Visual feedback
                ShowSlowEffect(true);
            }
        }

        /// <summary>
        /// Apply burn (damage over time)
        /// </summary>
        public virtual void ApplyBurn(float damagePerSecond, float duration)
        {
            burnDamage = damagePerSecond;
            burnDuration = duration;
            
            // Visual feedback
            ShowBurnEffect(true);
        }

        /// <summary>
        /// Apply stun (stops movement)
        /// </summary>
        public virtual void ApplyStun(float duration)
        {
            isStunned = true;
            stunDuration = duration;
            
            // Visual feedback
            ShowStunEffect(true);
        }

        protected virtual void ProcessStatusEffects()
        {
            // Process slow
            if (slowDuration > 0)
            {
                slowDuration -= Time.deltaTime;
                if (slowDuration <= 0)
                {
                    slowMultiplier = 1f;
                    ShowSlowEffect(false);
                }
            }

            // Process burn
            if (burnDuration > 0)
            {
                TakeDamage(burnDamage * Time.deltaTime, DamageType.Fire);
                burnDuration -= Time.deltaTime;
                if (burnDuration <= 0)
                {
                    ShowBurnEffect(false);
                }
            }

            // Process stun
            if (stunDuration > 0)
            {
                stunDuration -= Time.deltaTime;
                if (stunDuration <= 0)
                {
                    isStunned = false;
                    ShowStunEffect(false);
                }
            }
        }

        #endregion

        #region Visual Effects

        protected virtual IEnumerator DamageFlash()
        {
            if (bodyRenderers == null) yield break;

            // Flash white
            foreach (var renderer in bodyRenderers)
            {
                if (renderer != null)
                {
                    renderer.material.SetColor("_EmissionColor", Color.white);
                }
            }

            yield return new WaitForSeconds(0.1f);

            // Return to normal
            foreach (var renderer in bodyRenderers)
            {
                if (renderer != null)
                {
                    renderer.material.SetColor("_EmissionColor", Color.black);
                }
            }
        }

        protected virtual void SpawnDamageNumber(float damage)
        {
            // Spawn floating damage number
            // TODO: Implement damage number pooling
        }

        protected virtual void ShowSlowEffect(bool show)
        {
            // Blue tint or ice particles
        }

        protected virtual void ShowBurnEffect(bool show)
        {
            // Fire particles
        }

        protected virtual void ShowStunEffect(bool show)
        {
            // Stars or sparks above head
        }

        protected virtual void ResetVisuals()
        {
            ShowSlowEffect(false);
            ShowBurnEffect(false);
            ShowStunEffect(false);
        }

        #endregion

        #region Pooling Callbacks

        public void OnSpawnFromPool()
        {
            // Reset enemy state when spawned from pool
            IsDead = false;
            HasReachedEnd = false;
            currentWaypointIndex = 0;
        }

        public void OnReturnToPool()
        {
            // Cleanup when returned to pool
            StopAllCoroutines();
            ResetVisuals();
        }

        #endregion
    }

    /// <summary>
    /// Damage types for resistance system
    /// </summary>
    public enum DamageType
    {
        Physical,   // Bullets, missiles
        Energy,     // Lasers
        Fire,       // Flames, DOT
        Electric,   // Chain lightning, tesla
        Plasma      // High-powered energy
    }
}
