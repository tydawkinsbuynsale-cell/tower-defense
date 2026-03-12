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

    /// <summary>
    /// Swarm Mother Boss - Continuously spawns drone minions.
    /// Focus fire required to stop the swarm.
    /// </summary>
    public class SwarmMotherBoss : BossEnemy
    {
        [Header("Swarm Mother Specific")]
        [SerializeField] private GameObject dronePrefab;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int maxDrones = 8;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private ParticleSystem spawnEffect;

        private float spawnTimer;
        private int currentDroneCount;

        protected override void Update()
        {
            base.Update();

            // Spawn drones periodically
            if (!IsDead && currentDroneCount < maxDrones)
            {
                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0)
                {
                    SpawnDrone();
                    spawnTimer = spawnInterval;
                }
            }
        }

        private void SpawnDrone()
        {
            if (dronePrefab == null) return;

            Transform spawnPoint = spawnPoints != null && spawnPoints.Length > 0 
                ? spawnPoints[Random.Range(0, spawnPoints.Length)]
                : transform;

            GameObject droneObj = Core.ObjectPooler.Instance?.GetPooledObject("Enemy_Drone");
            if (droneObj == null)
            {
                droneObj = Instantiate(dronePrefab);
            }

            droneObj.transform.position = spawnPoint.position;
            var drone = droneObj.GetComponent<Enemy>();
            if (drone != null)
            {
                // Spawn at current position on path
                Transform[] remainingPath = new Transform[waypoints.Length - currentWaypointIndex];
                System.Array.Copy(waypoints, currentWaypointIndex, remainingPath, 0, remainingPath.Length);
                drone.Initialize(remainingPath, 0.3f, 1.5f); // Low health, fast
                
                // Track drone count
                currentDroneCount++;
                drone.OnDied.AddListener(() => currentDroneCount--);
            }

            // Spawn effect
            if (spawnEffect != null)
            {
                Instantiate(spawnEffect, spawnPoint.position, Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// Shield Commander Boss - Provides shields to nearby enemies.
    /// Must be eliminated to weaken enemy formations.
    /// </summary>
    public class ShieldCommanderBoss : BossEnemy
    {
        [Header("Shield Commander Specific")]
        [SerializeField] private float shieldRadius = 8f;
        [SerializeField] private float shieldAmount = 50f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private ParticleSystem shieldAura;
        [SerializeField] private GameObject shieldEffectPrefab;
        [SerializeField] private float shieldRefreshInterval = 5f;

        private System.Collections.Generic.Dictionary<Enemy, GameObject> shieldedEnemies = 
            new System.Collections.Generic.Dictionary<Enemy, GameObject>();
        private float shieldRefreshTimer;

        protected override void Start()
        {
            base.Start();
            if (shieldAura != null)
            {
                shieldAura.Play();
            }
        }

        protected override void Update()
        {
            base.Update();

            // Refresh shields periodically
            shieldRefreshTimer -= Time.deltaTime;
            if (shieldRefreshTimer <= 0)
            {
                UpdateShields();
                shieldRefreshTimer = shieldRefreshInterval;
            }
        }

        private void UpdateShields()
        {
            if (IsDead) return;

            // Find all enemies in range
            Collider[] nearby = Physics.OverlapSphere(transform.position, shieldRadius, enemyLayer);
            System.Collections.Generic.List<Enemy> enemiesInRange = new System.Collections.Generic.List<Enemy>();

            foreach (var col in nearby)
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null && enemy != this && !enemy.IsDead)
                {
                    enemiesInRange.Add(enemy);
                }
            }

            // Remove shields from enemies that left range
            foreach (var kvp in shieldedEnemies.ToArray())
            {
                if (kvp.Key == null || !enemiesInRange.Contains(kvp.Key))
                {
                    RemoveShield(kvp.Key);
                }
            }

            // Apply shields to enemies in range
            foreach (var enemy in enemiesInRange)
            {
                if (!shieldedEnemies.ContainsKey(enemy))
                {
                    ApplyShield(enemy);
                }
            }
        }

        private void ApplyShield(Enemy enemy)
        {
            if (enemy == null) return;

            // Visual shield effect
            if (shieldEffectPrefab != null)
            {
                GameObject shield = Instantiate(shieldEffectPrefab, enemy.transform);
                shield.transform.localPosition = Vector3.zero;
                shield.transform.localScale = Vector3.one * 1.2f;
                shieldedEnemies[enemy] = shield;
            }

            // Note: Actual shield mechanics would need to be implemented in Enemy class
            // This is a visual representation
        }

        private void RemoveShield(Enemy enemy)
        {
            if (shieldedEnemies.ContainsKey(enemy))
            {
                if (shieldedEnemies[enemy] != null)
                {
                    Destroy(shieldedEnemies[enemy]);
                }
                shieldedEnemies.Remove(enemy);
            }
        }

        protected override void Die()
        {
            // Remove all shields when commander dies
            foreach (var enemy in shieldedEnemies.Keys.ToArray())
            {
                RemoveShield(enemy);
            }
            shieldedEnemies.Clear();

            base.Die();
        }

        private void OnDrawGizmosSelected()
        {
            // Draw shield radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, shieldRadius);
        }
    }

    /// <summary>
    /// Cloaker Bot - Stealth unit that can turn invisible.
    /// Becomes visible when taking damage, re-cloaks after time.
    /// Can only be targeted when visible or by towers within detection range.
    /// </summary>
    public class CloakerEnemy : Enemy
    {
        [Header("Cloaker Specific")]
        [SerializeField] private float cloakTransitionDuration = 0.5f; // Time to cloak/uncloak
        [SerializeField] private float uncloakDuration = 2f; // Stay visible for 2s after damage
        [SerializeField] private float recloakCooldown = 3f; // Time before can cloak again
        [SerializeField] private float cloakedAlpha = 0.15f; // How transparent when cloaked
        [SerializeField] private ParticleSystem cloakEffect;
        [SerializeField] private ParticleSystem uncloakEffect;
        [SerializeField] private AudioClip cloakSound;
        [SerializeField] private AudioClip uncloakSound;

        // Cloaking state
        private bool isCloaked;
        private bool isTransitioning;
        private float transitionProgress;
        private float uncloakTimer;
        private float recloakTimer;
        private Color[] originalColors;
        private bool initialized;

        // Detection range - towers within this range can always target cloaked enemies
        public static float DetectionRange = 4f;

        public bool IsCloaked => isCloaked && !isTransitioning;

        public override void Initialize(Transform[] path, float healthMultiplier = 1f, float speedMultiplier = 1f)
        {
            base.Initialize(path, healthMultiplier, speedMultiplier);

            // Store original colors
            if (!initialized)
            {
                StoreOriginalColors();
                initialized = true;
            }

            // Start cloaked
            isCloaked = true;
            isTransitioning = false;
            transitionProgress = 1f;
            uncloakTimer = 0f;
            recloakTimer = 0f;

            ApplyCloakVisuals(cloakedAlpha);
        }

        protected override void Update()
        {
            base.Update();

            if (IsDead) return;

            // Handle cloak transition
            if (isTransitioning)
            {
                UpdateCloakTransition();
            }

            // Handle uncloak timer (re-cloak after duration)
            if (!isCloaked && uncloakTimer > 0)
            {
                uncloakTimer -= Time.deltaTime;
                if (uncloakTimer <= 0 && recloakTimer <= 0)
                {
                    StartCloak();
                }
            }

            // Handle recloak cooldown
            if (recloakTimer > 0)
            {
                recloakTimer -= Time.deltaTime;
            }
        }

        public override void TakeDamage(float damage, DamageType damageType = DamageType.Physical)
        {
            // Uncloak when taking damage
            if (isCloaked || isTransitioning)
            {
                StartUncloak();
            }

            // Reset uncloak timer
            uncloakTimer = uncloakDuration;
            recloakTimer = recloakCooldown;

            base.TakeDamage(damage, damageType);
        }

        private void StartCloak()
        {
            if (isCloaked || isTransitioning) return;

            isTransitioning = true;
            transitionProgress = 0f;
            isCloaked = false; // Will be set to true when transition completes

            // Play cloak effect
            if (cloakEffect != null)
            {
                cloakEffect.Play();
            }

            // Play cloak sound
            if (cloakSound != null)
            {
                AudioSource.PlayClipAtPoint(cloakSound, transform.position, 0.5f);
            }
        }

        private void StartUncloak()
        {
            if (!isCloaked && !isTransitioning) return;

            isTransitioning = true;
            transitionProgress = 0f;
            isCloaked = true; // Will be set to false when transition completes

            // Play uncloak effect
            if (uncloakEffect != null)
            {
                uncloakEffect.Play();
            }

            // Play uncloak sound
            if (uncloakSound != null)
            {
                AudioSource.PlayClipAtPoint(uncloakSound, transform.position, 0.5f);
            }
        }

        private void UpdateCloakTransition()
        {
            transitionProgress += Time.deltaTime / cloakTransitionDuration;

            if (transitionProgress >= 1f)
            {
                // Transition complete
                transitionProgress = 1f;
                isTransitioning = false;
                isCloaked = !isCloaked; // Toggle state
            }

            // Update visual transparency
            float targetAlpha = isCloaked ? cloakedAlpha : 1f;
            float currentAlpha = isCloaked 
                ? Mathf.Lerp(1f, targetAlpha, transitionProgress)
                : Mathf.Lerp(targetAlpha, 1f, transitionProgress);

            ApplyCloakVisuals(currentAlpha);
        }

        private void StoreOriginalColors()
        {
            if (bodyRenderers == null || bodyRenderers.Length == 0) return;

            originalColors = new Color[bodyRenderers.Length];
            for (int i = 0; i < bodyRenderers.Length; i++)
            {
                if (bodyRenderers[i] != null && bodyRenderers[i].material != null)
                {
                    originalColors[i] = bodyRenderers[i].material.color;
                }
            }
        }

        private void ApplyCloakVisuals(float alpha)
        {
            if (bodyRenderers == null || originalColors == null) return;

            for (int i = 0; i < bodyRenderers.Length && i < originalColors.Length; i++)
            {
                if (bodyRenderers[i] != null && bodyRenderers[i].material != null)
                {
                    Color color = originalColors[i];
                    color.a = alpha;
                    bodyRenderers[i].material.color = color;

                    // Enable transparent rendering mode
                    if (alpha < 1f)
                    {
                        SetMaterialTransparent(bodyRenderers[i].material);
                    }
                    else
                    {
                        SetMaterialOpaque(bodyRenderers[i].material);
                    }
                }
            }

            // Hide/show health bar based on cloak state
            if (healthBar != null)
            {
                healthBar.gameObject.SetActive(alpha >= 0.5f);
            }
        }

        private void SetMaterialTransparent(Material mat)
        {
            // Set rendering mode to Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }

        private void SetMaterialOpaque(Material mat)
        {
            // Set rendering mode to Opaque
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
        }

        /// <summary>
        /// Check if tower can detect this cloaked enemy
        /// </summary>
        public bool CanBeDetectedBy(Vector3 towerPosition, float detectionRadius)
        {
            // If not cloaked, always detectable
            if (!IsCloaked) return true;

            // Check if tower is within detection range
            float distance = Vector3.Distance(transform.position, towerPosition);
            return distance <= detectionRadius;
        }

        /// <summary>
        /// Static helper for towers to check if they can target a cloaker
        /// </summary>
        public static bool CanTarget(Enemy enemy, Vector3 towerPosition, float towerDetectionRange = -1f)
        {
            CloakerEnemy cloaker = enemy as CloakerEnemy;
            if (cloaker == null) return true; // Not a cloaker, always targetable

            // Use tower's detection range, or default detection range
            float detection = towerDetectionRange > 0 ? towerDetectionRange : DetectionRange;
            return cloaker.CanBeDetectedBy(towerPosition, detection);
        }

        protected override void Die()
        {
            // Force uncloak on death for visual clarity
            if (isCloaked)
            {
                ApplyCloakVisuals(1f);
            }

            base.Die();
        }

        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = isCloaked ? new Color(0.5f, 0f, 1f, 0.3f) : Color.green;
            Gizmos.DrawWireSphere(transform.position, DetectionRange);
        }
    }
}
