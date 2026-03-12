using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace RobotTD.Towers
{
    /// <summary>
    /// Base tower class. All tower types inherit from this.
    /// </summary>
    public abstract class Tower : MonoBehaviour
    {
        [Header("Tower Stats")]
        [SerializeField] protected TowerData towerData;
        
        [Header("References")]
        [SerializeField] protected Transform firePoint;
        [SerializeField] protected Transform turretPivot;
        [SerializeField] protected GameObject rangeIndicator;

        // Runtime state
        protected int currentLevel = 1;
        protected float lastFireTime;
        protected Enemies.Enemy currentTarget;
        protected List<Enemies.Enemy> enemiesInRange = new List<Enemies.Enemy>();

        // Properties
        public TowerData Data => towerData;
        public int Level => currentLevel;
        public bool IsMaxLevel => currentLevel >= towerData.maxLevel;
        
        // Current stats (with upgrades)
        public float CurrentDamage => towerData.baseDamage * GetDamageMultiplier();
        public float CurrentRange => towerData.baseRange * GetRangeMultiplier();
        public float CurrentFireRate => towerData.baseFireRate * GetFireRateMultiplier();
        public int UpgradeCost => towerData.GetUpgradeCost(currentLevel);
        public int SellValue => Mathf.FloorToInt(towerData.cost * 0.5f * currentLevel);

        // Events
        public UnityEvent<Enemies.Enemy> OnTargetAcquired;
        public UnityEvent OnFire;
        public UnityEvent<int> OnUpgraded;

        protected virtual void Awake()
        {
            OnTargetAcquired ??= new UnityEvent<Enemies.Enemy>();
            OnFire ??= new UnityEvent();
            OnUpgraded ??= new UnityEvent<int>();
        }

        protected virtual void Start()
        {
            SetupRangeIndicator();
        }

        protected virtual void Update()
        {
            // Remove dead or out-of-range enemies
            CleanupTargets();

            // Find target if we don't have one
            if (currentTarget == null || !IsValidTarget(currentTarget))
            {
                currentTarget = FindBestTarget();
                if (currentTarget != null)
                {
                    OnTargetAcquired?.Invoke(currentTarget);
                }
            }

            // Rotate towards target
            if (currentTarget != null && turretPivot != null)
            {
                RotateTowardsTarget();
            }

            // Fire at target
            if (currentTarget != null && CanFire())
            {
                Fire();
            }
        }

        #region Targeting

        protected virtual void CleanupTargets()
        {
            enemiesInRange.RemoveAll(e => e == null || e.IsDead || !IsInRange(e));
        }

        protected virtual Enemies.Enemy FindBestTarget()
        {
            if (enemiesInRange.Count == 0) return null;

            // Different targeting priorities
            switch (towerData.targetPriority)
            {
                case TargetPriority.First:
                    return GetFirstEnemy();
                case TargetPriority.Last:
                    return GetLastEnemy();
                case TargetPriority.Strongest:
                    return GetStrongestEnemy();
                case TargetPriority.Weakest:
                    return GetWeakestEnemy();
                case TargetPriority.Closest:
                    return GetClosestEnemy();
                default:
                    return enemiesInRange[0];
            }
        }

        protected Enemies.Enemy GetFirstEnemy()
        {
            Enemies.Enemy first = null;
            float maxProgress = -1;

            foreach (var enemy in enemiesInRange)
            {
                if (enemy.PathProgress > maxProgress)
                {
                    maxProgress = enemy.PathProgress;
                    first = enemy;
                }
            }
            return first;
        }

        protected Enemies.Enemy GetLastEnemy()
        {
            Enemies.Enemy last = null;
            float minProgress = float.MaxValue;

            foreach (var enemy in enemiesInRange)
            {
                if (enemy.PathProgress < minProgress)
                {
                    minProgress = enemy.PathProgress;
                    last = enemy;
                }
            }
            return last;
        }

        protected Enemies.Enemy GetStrongestEnemy()
        {
            Enemies.Enemy strongest = null;
            float maxHealth = -1;

            foreach (var enemy in enemiesInRange)
            {
                if (enemy.CurrentHealth > maxHealth)
                {
                    maxHealth = enemy.CurrentHealth;
                    strongest = enemy;
                }
            }
            return strongest;
        }

        protected Enemies.Enemy GetWeakestEnemy()
        {
            Enemies.Enemy weakest = null;
            float minHealth = float.MaxValue;

            foreach (var enemy in enemiesInRange)
            {
                if (enemy.CurrentHealth < minHealth)
                {
                    minHealth = enemy.CurrentHealth;
                    weakest = enemy;
                }
            }
            return weakest;
        }

        protected Enemies.Enemy GetClosestEnemy()
        {
            Enemies.Enemy closest = null;
            float minDist = float.MaxValue;

            foreach (var enemy in enemiesInRange)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = enemy;
                }
            }
            return closest;
        }

        protected bool IsInRange(Enemies.Enemy enemy)
        {
            return Vector3.Distance(transform.position, enemy.transform.position) <= CurrentRange;
        }

        protected bool IsValidTarget(Enemies.Enemy enemy)
        {
            return enemy != null && !enemy.IsDead && IsInRange(enemy);
        }

        #endregion

        #region Combat

        protected virtual void RotateTowardsTarget()
        {
            Vector3 direction = currentTarget.transform.position - turretPivot.position;
            direction.y = 0; // Keep rotation horizontal

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                turretPivot.rotation = Quaternion.Slerp(
                    turretPivot.rotation, 
                    targetRotation, 
                    towerData.rotationSpeed * Time.deltaTime
                );
            }
        }

        protected virtual bool CanFire()
        {
            return Time.time - lastFireTime >= (1f / CurrentFireRate);
        }

        protected virtual void Fire()
        {
            lastFireTime = Time.time;
            OnFire?.Invoke();
            
            // Create projectile
            SpawnProjectile();
            
            // Play sound, particles, etc.
            PlayFireEffects();
        }

        protected abstract void SpawnProjectile();

        protected virtual void PlayFireEffects()
        {
            // Override in derived classes for custom effects
            // Play sound
            if (towerData.fireSound != null)
            {
                AudioSource.PlayClipAtPoint(towerData.fireSound, transform.position);
            }
        }

        #endregion

        #region Upgrades

        public virtual bool CanUpgrade()
        {
            return !IsMaxLevel && Core.GameManager.Instance.CanAfford(UpgradeCost);
        }

        public virtual void Upgrade()
        {
            if (!CanUpgrade()) return;

            if (Core.GameManager.Instance.SpendCredits(UpgradeCost))
            {
                currentLevel++;
                OnUpgraded?.Invoke(currentLevel);
                
                // Track upgrade in save data
                Core.SaveManager.Instance?.RecordTowerUpgraded();

                // Notify achievement manager
                Progression.AchievementManager.Instance?.OnTowerUpgraded(currentLevel);
                
                // Update range indicator
                SetupRangeIndicator();
            }
        }

        public virtual void Sell()
        {
            Core.GameManager.Instance.AddCredits(SellValue);
            
            // Notify achievement manager
            Progression.AchievementManager.Instance?.OnTowerSold();
            
            Destroy(gameObject);
        }

        protected virtual float GetDamageMultiplier()
        {
            float baseMultiplier = 1f + (currentLevel - 1) * towerData.damageUpgradePercent;
            
            // Apply challenge modifier if active
            if (Core.ChallengeManager.Instance != null)
            {
                float challengeMult = Core.ChallengeManager.Instance.GetTowerDamageMultiplier();
                return baseMultiplier * challengeMult;
            }
            
            return baseMultiplier;
        }

        protected virtual float GetRangeMultiplier()
        {
            return 1f + (currentLevel - 1) * towerData.rangeUpgradePercent;
        }

        protected virtual float GetFireRateMultiplier()
        {
            return 1f + (currentLevel - 1) * towerData.fireRateUpgradePercent;
        }

        #endregion

        #region Range Indicator

        protected virtual void SetupRangeIndicator()
        {
            if (rangeIndicator != null)
            {
                float diameter = CurrentRange * 2f;
                rangeIndicator.transform.localScale = new Vector3(diameter, 0.01f, diameter);
                rangeIndicator.SetActive(false);
            }
        }

        public void ShowRange(bool show)
        {
            if (rangeIndicator != null)
            {
                rangeIndicator.SetActive(show);
            }
        }

        #endregion

        #region Collision Detection

        private void OnTriggerEnter(Collider other)
        {
            var enemy = other.GetComponent<Enemies.Enemy>();
            if (enemy != null && !enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Add(enemy);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var enemy = other.GetComponent<Enemies.Enemy>();
            if (enemy != null)
            {
                enemiesInRange.Remove(enemy);
                if (currentTarget == enemy)
                {
                    currentTarget = null;
                }
            }
        }

        #endregion

        #region Public Methods

        public void SetTargetPriority(TargetPriority priority)
        {
            // Create a copy of tower data with new priority
            // (or add a runtime priority field)
        }

        #endregion
    }

    /// <summary>
    /// Target selection priorities
    /// </summary>
    public enum TargetPriority
    {
        First,      // Enemy closest to the end
        Last,       // Enemy closest to the start
        Strongest,  // Highest current health
        Weakest,    // Lowest current health
        Closest     // Closest to the tower
    }
}
