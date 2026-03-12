using UnityEngine;
using RobotTD.Core;
using RobotTD.Projectiles;

namespace RobotTD.Towers
{
    /// <summary>
    /// Laser Turret - Fast firing, accurate, single target damage.
    /// Good all-around tower for consistent DPS.
    /// </summary>
    public class LaserTurret : Tower
    {
        [Header("Laser Specific")]
        [SerializeField] private LineRenderer laserBeam;
        [SerializeField] private float laserDuration = 0.1f;
        [SerializeField] private ParticleSystem hitParticles;

        private float laserTimer;

        protected override void Update()
        {
            base.Update();

            // Fade out laser
            if (laserBeam != null && laserBeam.enabled)
            {
                laserTimer -= Time.deltaTime;
                if (laserTimer <= 0)
                {
                    laserBeam.enabled = false;
                }
            }
        }

        protected override void SpawnProjectile()
        {
            if (currentTarget == null) return;

            // Instant hit laser (not a projectile)
            currentTarget.TakeDamage(CurrentDamage, DamageType.Energy);

            // Visual laser beam
            if (laserBeam != null)
            {
                laserBeam.enabled = true;
                laserBeam.SetPosition(0, firePoint.position);
                laserBeam.SetPosition(1, currentTarget.transform.position);
                laserTimer = laserDuration;
            }

            // Hit particles
            if (hitParticles != null)
            {
                hitParticles.transform.position = currentTarget.transform.position;
                hitParticles.Play();
            }
        }
    }

    /// <summary>
    /// Plasma Cannon - Slow firing, high damage projectiles.
    /// Great against tanks and bosses.
    /// </summary>
    public class PlasmaCannon : Tower
    {
        [Header("Plasma Specific")]
        [SerializeField] private float projectileSpeed = 15f;
        [SerializeField] private ParticleSystem chargeEffect;

        protected override void SpawnProjectile()
        {
            if (currentTarget == null) return;

            GameObject proj = ObjectPooler.Instance?.GetPooledObject("Projectile_Plasma");
            if (proj == null && towerData.projectilePrefab != null)
            {
                proj = Instantiate(towerData.projectilePrefab);
            }

            if (proj != null)
            {
                proj.transform.position = firePoint.position;
                proj.transform.rotation = firePoint.rotation;

                var projectile = proj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Initialize(currentTarget, CurrentDamage, projectileSpeed, DamageType.Plasma);
                }
            }
        }

        protected override void Fire()
        {
            // Play charge effect before firing
            if (chargeEffect != null)
            {
                chargeEffect.Play();
            }
            
            base.Fire();
        }
    }

    /// <summary>
    /// Rocket Launcher - Splash damage, great for groups.
    /// Slow but devastating against clustered enemies.
    /// </summary>
    public class RocketLauncher : Tower
    {
        [Header("Rocket Specific")]
        [SerializeField] private float rocketSpeed = 8f;
        [SerializeField] private GameObject explosionPrefab;

        protected override void SpawnProjectile()
        {
            if (currentTarget == null) return;

            GameObject proj = ObjectPooler.Instance?.GetPooledObject("Projectile_Rocket");
            if (proj == null && towerData.projectilePrefab != null)
            {
                proj = Instantiate(towerData.projectilePrefab);
            }

            if (proj != null)
            {
                proj.transform.position = firePoint.position;
                proj.transform.LookAt(currentTarget.transform);

                var rocket = proj.GetComponent<RocketProjectile>();
                if (rocket != null)
                {
                    rocket.Initialize(
                        currentTarget, 
                        CurrentDamage, 
                        rocketSpeed, 
                        towerData.splashRadius,
                        towerData.splashDamagePercent,
                        explosionPrefab
                    );
                }
            }
        }
    }

    /// <summary>
    /// Freeze Turret - Slows enemies with cryo projectiles.
    /// Essential for support and controlling fast enemies.
    /// </summary>
    public class FreezeTurret : Tower
    {
        [Header("Freeze Specific")]
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private ParticleSystem frostAura;

        protected override void Start()
        {
            base.Start();
            if (frostAura != null)
            {
                frostAura.Play();
            }
        }

        protected override void SpawnProjectile()
        {
            if (currentTarget == null) return;

            GameObject proj = ObjectPooler.Instance?.GetPooledObject("Projectile_Freeze");
            if (proj == null && towerData.projectilePrefab != null)
            {
                proj = Instantiate(towerData.projectilePrefab);
            }

            if (proj != null)
            {
                proj.transform.position = firePoint.position;
                proj.transform.rotation = firePoint.rotation;

                var projectile = proj.GetComponent<FreezeProjectile>();
                if (projectile != null)
                {
                    projectile.Initialize(
                        currentTarget, 
                        CurrentDamage, 
                        projectileSpeed, 
                        towerData.slowPercent,
                        towerData.slowDuration
                    );
                }
            }
        }
    }

    /// <summary>
    /// Shock Tower - Chain lightning that jumps between enemies.
    /// Excellent crowd control for grouped enemies.
    /// </summary>
    public class ShockTower : Tower
    {
        [Header("Shock Specific")]
        [SerializeField] private LineRenderer lightningEffect;
        [SerializeField] private float chainDelay = 0.05f;
        [SerializeField] private ParticleSystem sparkEffect;

        protected override void SpawnProjectile()
        {
            if (currentTarget == null) return;

            // Start chain lightning
            StartCoroutine(ChainLightning());
        }

        private System.Collections.IEnumerator ChainLightning()
        {
            System.Collections.Generic.List<Enemies.Enemy> hitEnemies = new System.Collections.Generic.List<Enemies.Enemy>();
            Enemies.Enemy target = currentTarget;
            Vector3 lastPosition = firePoint.position;
            float damage = CurrentDamage;

            for (int i = 0; i <= towerData.chainCount && target != null; i++)
            {
                // Draw lightning
                if (lightningEffect != null)
                {
                    lightningEffect.enabled = true;
                    lightningEffect.positionCount = 2;
                    lightningEffect.SetPosition(0, lastPosition);
                    lightningEffect.SetPosition(1, target.transform.position);
                }

                // Deal damage
                target.TakeDamage(damage, DamageType.Electric);
                hitEnemies.Add(target);

                // Spark effect
                if (sparkEffect != null)
                {
                    sparkEffect.transform.position = target.transform.position;
                    sparkEffect.Play();
                }

                lastPosition = target.transform.position;
                damage *= 0.7f; // 30% damage reduction per chain

                yield return new WaitForSeconds(chainDelay);

                // Find next target
                target = FindNextChainTarget(lastPosition, hitEnemies);
            }

            // Disable lightning effect
            yield return new WaitForSeconds(0.1f);
            if (lightningEffect != null)
            {
                lightningEffect.enabled = false;
            }
        }

        private Enemies.Enemy FindNextChainTarget(Vector3 fromPosition, System.Collections.Generic.List<Enemies.Enemy> exclude)
        {
            Enemies.Enemy nearest = null;
            float nearestDist = towerData.chainRange;

            foreach (var enemy in enemiesInRange)
            {
                if (enemy == null || enemy.IsDead || exclude.Contains(enemy)) continue;

                float dist = Vector3.Distance(fromPosition, enemy.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }

            return nearest;
        }
    }

    /// <summary>
    /// Sniper Bot - Extreme range, very high damage, slow fire rate.
    /// Perfect for taking out high-value targets.
    /// </summary>
    public class SniperBot : Tower
    {
        [Header("Sniper Specific")]
        [SerializeField] private LineRenderer sniperTrail;
        [SerializeField] private float trailDuration = 0.2f;
        [SerializeField] private float critChance = 0.2f;
        [SerializeField] private float critMultiplier = 2f;

        protected override void SpawnProjectile()
        {
            if (currentTarget == null) return;

            // Check for critical hit
            float damage = CurrentDamage;
            bool isCrit = Random.value < critChance;
            if (isCrit)
            {
                damage *= critMultiplier;
                // TODO: Show crit visual
            }

            // Instant hit
            currentTarget.TakeDamage(damage, DamageType.Physical);

            // Sniper trail visual
            if (sniperTrail != null)
            {
                sniperTrail.enabled = true;
                sniperTrail.SetPosition(0, firePoint.position);
                sniperTrail.SetPosition(1, currentTarget.transform.position);
                StartCoroutine(FadeTrail());
            }
        }

        private System.Collections.IEnumerator FadeTrail()
        {
            yield return new WaitForSeconds(trailDuration);
            if (sniperTrail != null)
            {
                sniperTrail.enabled = false;
            }
        }
    }

    /// <summary>
    /// Flamethrower - Continuous damage over time in a cone.
    /// Melts through waves of weak enemies.
    /// </summary>
    public class Flamethrower : Tower
    {
        [Header("Flame Specific")]
        [SerializeField] private ParticleSystem flameEffect;
        [SerializeField] private float coneAngle = 45f;
        [SerializeField] private float burnTickRate = 0.25f;

        private float burnTimer;
        private bool isFiring;

        protected override void Update()
        {
            base.Update();

            // Continuous damage while firing
            if (isFiring && currentTarget != null)
            {
                burnTimer -= Time.deltaTime;
                if (burnTimer <= 0)
                {
                    DealConeDamage();
                    burnTimer = burnTickRate;
                }
            }
        }

        protected override void Fire()
        {
            isFiring = true;
            if (flameEffect != null && !flameEffect.isPlaying)
            {
                flameEffect.Play();
            }
            DealConeDamage();
        }

        private void DealConeDamage()
        {
            foreach (var enemy in enemiesInRange)
            {
                if (enemy == null || enemy.IsDead) continue;

                // Check if enemy is within cone
                Vector3 toEnemy = enemy.transform.position - firePoint.position;
                float angle = Vector3.Angle(firePoint.forward, toEnemy);

                if (angle <= coneAngle / 2f)
                {
                    // Apply damage and DOT
                    enemy.TakeDamage(CurrentDamage * burnTickRate, DamageType.Fire);
                    enemy.ApplyBurn(towerData.dotDamage, towerData.dotDuration);
                }
            }
        }

        protected override void SpawnProjectile()
        {
            // Flamethrower doesn't use projectiles
        }
    }

    /// <summary>
    /// Tesla Coil - Auto-targets multiple enemies within range.
    /// Great for area denial.
    /// </summary>
    public class TeslaCoil : Tower
    {
        [Header("Tesla Specific")]
        [SerializeField] private int maxTargets = 4;
        [SerializeField] private LineRenderer[] arcRenderers;
        [SerializeField] private float arcDuration = 0.15f;
        [SerializeField] private Transform coilTop;

        protected override void SpawnProjectile()
        {
            int targetCount = Mathf.Min(maxTargets, enemiesInRange.Count);
            float damagePerTarget = CurrentDamage / targetCount;

            for (int i = 0; i < targetCount && i < arcRenderers.Length; i++)
            {
                if (enemiesInRange[i] != null && !enemiesInRange[i].IsDead)
                {
                    // Deal damage
                    enemiesInRange[i].TakeDamage(damagePerTarget, DamageType.Electric);

                    // Show arc
                    arcRenderers[i].enabled = true;
                    arcRenderers[i].SetPosition(0, coilTop.position);
                    arcRenderers[i].SetPosition(1, enemiesInRange[i].transform.position);
                }
            }

            StartCoroutine(DisableArcs());
        }

        private System.Collections.IEnumerator DisableArcs()
        {
            yield return new WaitForSeconds(arcDuration);
            foreach (var arc in arcRenderers)
            {
                arc.enabled = false;
            }
        }

        // Tesla coil doesn't need to rotate
        protected override void RotateTowardsTarget() { }
    }
}
