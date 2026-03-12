using UnityEngine;
using System.Collections;

namespace RobotTD.Enemies
{
    /// <summary>
    /// Scout Bot - Fast moving, low health.
    /// The basic enemy type, good for early waves.
    /// </summary>
    public class ScoutEnemy : Enemy
    {
        [Header("Scout Specific")]
        [SerializeField] private float dodgeChance = 0.1f;
        [SerializeField] private ParticleSystem speedTrail;

        protected override void Start()
        {
            base.Start();
            if (speedTrail != null) speedTrail.Play();
        }

        public override void TakeDamage(float damage, DamageType damageType)
        {
            // Small chance to dodge
            if (Random.value < dodgeChance)
            {
                // Show dodge effect
                Debug.Log("Scout dodged!");
                return;
            }
            base.TakeDamage(damage, damageType);
        }
    }

    /// <summary>
    /// Soldier Bot - Balanced stats, reliable.
    /// The standard enemy type.
    /// </summary>
    public class SoldierEnemy : Enemy
    {
        [Header("Soldier Specific")]
        [SerializeField] private float armorBonus = 0.1f; // 10% damage reduction

        public override void TakeDamage(float damage, DamageType damageType)
        {
            // Apply armor
            float reducedDamage = damage * (1f - armorBonus);
            base.TakeDamage(reducedDamage, damageType);
        }
    }

    /// <summary>
    /// Tank Bot - Slow, very high health, deals more damage to base.
    /// Requires focused fire to take down.
    /// </summary>
    public class TankEnemy : Enemy
    {
        [Header("Tank Specific")]
        [SerializeField] private float damageReductionPercent = 0.2f;
        [SerializeField] private float heavyArmorThreshold = 30f; // Ignore damage below this
        [SerializeField] private ParticleSystem tankTreads;

        public override void TakeDamage(float damage, DamageType damageType)
        {
            // Heavy armor: small hits are less effective
            if (damage < heavyArmorThreshold)
            {
                damage *= 0.5f; // Half damage for weak attacks
            }

            // General damage reduction
            float reducedDamage = damage * (1f - damageReductionPercent);
            base.TakeDamage(reducedDamage, damageType);
        }

        protected override void MoveAlongPath()
        {
            // Slightly slower but steady
            base.MoveAlongPath();
            
            // Shake screen slightly when moving (for impact)
        }
    }

    /// <summary>
    /// Elite Bot - Fast, high health, has shield ability.
    /// Dangerous late-game enemy.
    /// </summary>
    public class EliteEnemy : Enemy
    {
        [Header("Elite Specific")]
        [SerializeField] private float shieldHealth = 100f;
        [SerializeField] private float shieldRechargeTime = 5f;
        [SerializeField] private GameObject shieldEffect;
        [SerializeField] private AudioClip shieldActivateSound;

        private float currentShieldHealth;
        private bool shieldActive = true;
        private float shieldRechargeTimer;

        public override void Initialize(Transform[] path, float healthMultiplier = 1f, float speedMultiplier = 1f)
        {
            base.Initialize(path, healthMultiplier, speedMultiplier);
            currentShieldHealth = shieldHealth * healthMultiplier;
            shieldActive = true;
            UpdateShieldVisual();
        }

        protected override void Update()
        {
            base.Update();

            // Shield recharge
            if (!shieldActive && currentShieldHealth <= 0)
            {
                shieldRechargeTimer -= Time.deltaTime;
                if (shieldRechargeTimer <= 0)
                {
                    ActivateShield();
                }
            }
        }

        public override void TakeDamage(float damage, DamageType damageType)
        {
            if (shieldActive)
            {
                // Shield absorbs damage first
                currentShieldHealth -= damage;
                
                if (currentShieldHealth <= 0)
                {
                    // Shield broken
                    shieldActive = false;
                    shieldRechargeTimer = shieldRechargeTime;
                    UpdateShieldVisual();
                    
                    // Remaining damage goes through
                    float overflow = -currentShieldHealth;
                    if (overflow > 0)
                    {
                        base.TakeDamage(overflow, damageType);
                    }
                }
            }
            else
            {
                base.TakeDamage(damage, damageType);
            }
        }

        private void ActivateShield()
        {
            currentShieldHealth = shieldHealth;
            shieldActive = true;
            UpdateShieldVisual();

            if (shieldActivateSound != null)
            {
                AudioSource.PlayClipAtPoint(shieldActivateSound, transform.position);
            }
        }

        private void UpdateShieldVisual()
        {
            if (shieldEffect != null)
            {
                shieldEffect.SetActive(shieldActive);
            }
        }
    }

    /// <summary>
    /// Boss Bot - Massive health, special attacks, ultimate threat.
    /// Appears every 5 waves.
    /// </summary>
    public class BossEnemy : Enemy
    {
        [Header("Boss Specific")]
        [SerializeField] private float healthRegenPercent = 0.01f; // 1% per second
        [SerializeField] private float enrageHealthPercent = 0.25f;
        [SerializeField] private float enrageSpeedBoost = 0.5f;
        [SerializeField] private ParticleSystem enrageEffect;
        [SerializeField] private AudioClip roarSound;

        private bool isEnraged = false;

        public override void Initialize(Transform[] path, float healthMultiplier = 1f, float speedMultiplier = 1f)
        {
            base.Initialize(path, healthMultiplier, speedMultiplier);
            isEnraged = false;
        }

        protected override void Update()
        {
            base.Update();

            // Health regeneration
            if (!IsDead)
            {
                float regenAmount = maxHealth * healthRegenPercent * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth + regenAmount, maxHealth);
                UpdateHealthBar();
            }

            // Enrage when low health
            if (!isEnraged && HealthPercent <= enrageHealthPercent)
            {
                Enrage();
            }
        }

        private void Enrage()
        {
            isEnraged = true;
            moveSpeed *= (1f + enrageSpeedBoost);

            if (enrageEffect != null)
            {
                enrageEffect.Play();
            }

            if (roarSound != null)
            {
                AudioSource.PlayClipAtPoint(roarSound, transform.position);
            }

            // Visual change
            foreach (var renderer in bodyRenderers)
            {
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
            }
        }
    }

    /// <summary>
    /// Flying Bot - Can only be targeted by certain towers.
    /// Bypasses some ground-based defenses.
    /// </summary>
    public class FlyingEnemy : Enemy
    {
        [Header("Flying Specific")]
        [SerializeField] private float flyHeight = 3f;
        [SerializeField] private float bobAmplitude = 0.3f;
        [SerializeField] private float bobFrequency = 2f;
        [SerializeField] private ParticleSystem thrusterEffect;

        private float bobTimer;

        protected override void MoveAlongPath()
        {
            if (waypoints == null || currentWaypointIndex >= waypoints.Length) return;

            Transform targetWaypoint = waypoints[currentWaypointIndex];
            
            // Get ground target position with height offset
            Vector3 targetPos = targetWaypoint.position;
            targetPos.y += flyHeight;

            // Add bobbing motion
            bobTimer += Time.deltaTime * bobFrequency;
            float bob = Mathf.Sin(bobTimer) * bobAmplitude;
            targetPos.y += bob;

            Vector3 direction = (targetPos - transform.position).normalized;
            float effectiveSpeed = moveSpeed * slowMultiplier * Time.deltaTime;

            transform.position += direction * effectiveSpeed;
            distanceTraveled += effectiveSpeed;

            // Rotate to face movement
            if (direction != Vector3.zero && modelTransform != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                modelTransform.rotation = Quaternion.Slerp(
                    modelTransform.rotation, 
                    targetRotation, 
                    10f * Time.deltaTime
                );
            }

            // Check if reached waypoint (using 2D distance for flying)
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            Vector2 target2D = new Vector2(targetWaypoint.position.x, targetWaypoint.position.z);
            
            if (Vector2.Distance(pos2D, target2D) < 0.5f)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length)
                {
                    ReachEnd();
                }
            }
        }
    }

    /// <summary>
    /// Healer Bot - Heals nearby enemies.
    /// High priority target.
    /// </summary>
    public class HealerEnemy : Enemy
    {
        [Header("Healer Specific")]
        [SerializeField] private float healRadius = 5f;
        [SerializeField] private float healAmount = 10f;
        [SerializeField] private float healInterval = 1f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private ParticleSystem healEffect;
        [SerializeField] private LineRenderer healBeam;

        private float healTimer;

        protected override void Update()
        {
            base.Update();

            // Heal nearby enemies
            healTimer -= Time.deltaTime;
            if (healTimer <= 0)
            {
                HealNearbyEnemies();
                healTimer = healInterval;
            }
        }

        private void HealNearbyEnemies()
        {
            Collider[] nearby = Physics.OverlapSphere(transform.position, healRadius, enemyLayer);
            
            foreach (var col in nearby)
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null && enemy != this && !enemy.IsDead)
                {
                    enemy.Heal(healAmount);

                    // Visual effect
                    if (healEffect != null)
                    {
                        healEffect.transform.position = enemy.transform.position;
                        healEffect.Play();
                    }
                }
            }
        }

        /// <summary>
        /// Heal this enemy
        /// </summary>
        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            UpdateHealthBar();
        }
    }

    /// <summary>
    /// Splitter Bot - Splits into smaller enemies when killed.
    /// Creates more targets.
    /// </summary>
    public class SplitterEnemy : Enemy
    {
        [Header("Splitter Specific")]
        [SerializeField] private int splitCount = 2;
        [SerializeField] private GameObject splitEnemyPrefab;
        [SerializeField] private float splitOffset = 0.5f;

        protected override void Die()
        {
            // Spawn split enemies before dying
            if (splitEnemyPrefab != null)
            {
                for (int i = 0; i < splitCount; i++)
                {
                    Vector3 offset = new Vector3(
                        Random.Range(-splitOffset, splitOffset),
                        0,
                        Random.Range(-splitOffset, splitOffset)
                    );

                    GameObject splitObj = Core.ObjectPooler.Instance?.GetPooledObject("Enemy_Split");
                    if (splitObj == null)
                    {
                        splitObj = Instantiate(splitEnemyPrefab);
                    }

                    splitObj.transform.position = transform.position + offset;
                    var splitEnemy = splitObj.GetComponent<Enemy>();
                    if (splitEnemy != null)
                    {
                        // Pass along remaining path
                        Transform[] remainingPath = new Transform[waypoints.Length - currentWaypointIndex];
                        System.Array.Copy(waypoints, currentWaypointIndex, remainingPath, 0, remainingPath.Length);
                        splitEnemy.Initialize(remainingPath, 0.5f, 1.2f); // Half health, faster
                    }
                }
            }

            // Don't award full rewards for death (splits give rewards)
            IsDead = true;
            OnDied?.Invoke();
            Core.WaveManager.Instance?.OnEnemyRemoved();

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
    }

    /// <summary>
    /// Teleporter Bot - Periodically teleports forward on the path.
    /// Very annoying enemy type.
    /// </summary>
    public class TeleporterEnemy : Enemy
    {
        [Header("Teleporter Specific")]
        [SerializeField] private float teleportCooldown = 3f;
        [SerializeField] private int teleportSkipWaypoints = 2;
        [SerializeField] private ParticleSystem teleportInEffect;
        [SerializeField] private ParticleSystem teleportOutEffect;
        [SerializeField] private AudioClip teleportSound;

        private float teleportTimer;

        protected override void Update()
        {
            base.Update();

            teleportTimer -= Time.deltaTime;
            if (teleportTimer <= 0)
            {
                TryTeleport();
                teleportTimer = teleportCooldown;
            }
        }

        private void TryTeleport()
        {
            int targetIndex = currentWaypointIndex + teleportSkipWaypoints;
            
            if (targetIndex < waypoints.Length)
            {
                // Teleport out effect
                if (teleportOutEffect != null)
                {
                    Instantiate(teleportOutEffect, transform.position, Quaternion.identity);
                }

                // Teleport
                currentWaypointIndex = targetIndex;
                transform.position = waypoints[currentWaypointIndex].position;

                // Teleport in effect
                if (teleportInEffect != null)
                {
                    teleportInEffect.Play();
                }

                // Sound
                if (teleportSound != null)
                {
                    AudioSource.PlayClipAtPoint(teleportSound, transform.position);
                }
            }
        }
    }
}
