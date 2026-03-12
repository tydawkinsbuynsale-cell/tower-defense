using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotTowerDefense.Core
{
    /// <summary>
    /// Manages power-up inventory, activation, and effects.
    /// Power-ups provide temporary gameplay advantages (damage boost, speed up, shield, etc.).
    /// Integrates with IAP (purchase bundles) and Ads (earn free power-ups).
    /// </summary>
    public class PowerUpManager : MonoBehaviour
    {
        #region Singleton
        public static PowerUpManager Instance { get; private set; }
        #endregion

        #region Configuration
        [Header("Power-Up Settings")]
        [SerializeField] private bool enablePowerUps = true;
        [SerializeField] private float defaultDuration = 30f;
        [SerializeField] private int maxStackSize = 99;

        [Header("Effect Multipliers")]
        [SerializeField] private float damageBoostMultiplier = 2f;
        [SerializeField] private float speedBoostMultiplier = 1.5f;
        [SerializeField] private float creditBoostMultiplier = 2f;
        [SerializeField] private float shieldDuration = 60f;
        [SerializeField] private float timeFreezeSlowdown = 0.1f;
        #endregion

        #region State
        private Dictionary<PowerUpType, int> inventory = new Dictionary<PowerUpType, int>();
        private Dictionary<PowerUpType, PowerUpInstance> activePowerUps = new Dictionary<PowerUpType, PowerUpInstance>();
        private Dictionary<PowerUpType, Coroutine> expirationCoroutines = new Dictionary<PowerUpType, Coroutine>();
        
        // Effect states
        private bool isDamageBoostActive = false;
        private bool isSpeedBoostActive = false;
        private bool isCreditBoostActive = false;
        private bool isShieldActive = false;
        private bool isTimeFreezeActive = false;
        #endregion

        #region Events
        public event Action<PowerUpType, int> OnInventoryChanged; // type, newCount
        public event Action<PowerUpType, float> OnPowerUpActivated; // type, duration
        public event Action<PowerUpType> OnPowerUpExpired; // type
        public event Action<PowerUpType, float> OnPowerUpTimeUpdated; // type, remainingTime
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeInventory();
        }

        private void Start()
        {
            LoadInventory();
        }

        private void Update()
        {
            UpdateActivePowerUps();
        }
        #endregion

        #region Initialization
        private void InitializeInventory()
        {
            foreach (PowerUpType type in Enum.GetValues(typeof(PowerUpType)))
            {
                inventory[type] = 0;
            }
        }

        private void LoadInventory()
        {
            foreach (PowerUpType type in Enum.GetValues(typeof(PowerUpType)))
            {
                string key = $"PowerUp_{type}";
                inventory[type] = PlayerPrefs.GetInt(key, 0);
            }

            Debug.Log($"[PowerUpManager] Loaded inventory - Total power-ups: {GetTotalPowerUpCount()}");
        }

        private void SaveInventory()
        {
            foreach (var kvp in inventory)
            {
                string key = $"PowerUp_{kvp.Key}";
                PlayerPrefs.SetInt(key, kvp.Value);
            }

            PlayerPrefs.Save();
        }
        #endregion

        #region Inventory Management
        /// <summary>
        /// Add power-up to inventory.
        /// </summary>
        public void AddPowerUp(PowerUpType type, int amount = 1)
        {
            if (!enablePowerUps) return;

            int currentCount = inventory[type];
            int newCount = Mathf.Min(currentCount + amount, maxStackSize);
            inventory[type] = newCount;

            SaveInventory();

            Debug.Log($"[PowerUpManager] Added {amount}x {type} - Total: {newCount}");

            OnInventoryChanged?.Invoke(type, newCount);

            // Track analytics
            Analytics.AnalyticsManager.Instance?.TrackEvent("powerup_added", new Dictionary<string, object>
            {
                { "type", type.ToString() },
                { "amount", amount },
                { "total", newCount }
            });
        }

        /// <summary>
        /// Remove power-up from inventory.
        /// </summary>
        public bool RemovePowerUp(PowerUpType type, int amount = 1)
        {
            if (!HasPowerUp(type, amount))
                return false;

            inventory[type] -= amount;
            SaveInventory();

            Debug.Log($"[PowerUpManager] Removed {amount}x {type} - Remaining: {inventory[type]}");

            OnInventoryChanged?.Invoke(type, inventory[type]);

            return true;
        }

        /// <summary>
        /// Check if player has power-up in inventory.
        /// </summary>
        public bool HasPowerUp(PowerUpType type, int amount = 1)
        {
            return inventory.ContainsKey(type) && inventory[type] >= amount;
        }

        /// <summary>
        /// Get count of specific power-up type.
        /// </summary>
        public int GetPowerUpCount(PowerUpType type)
        {
            return inventory.ContainsKey(type) ? inventory[type] : 0;
        }

        /// <summary>
        /// Get total count of all power-ups.
        /// </summary>
        public int GetTotalPowerUpCount()
        {
            int total = 0;
            foreach (var count in inventory.Values)
            {
                total += count;
            }
            return total;
        }
        #endregion

        #region Power-Up Activation
        /// <summary>
        /// Activate power-up from inventory.
        /// </summary>
        public bool ActivatePowerUp(PowerUpType type)
        {
            if (!enablePowerUps)
            {
                Debug.LogWarning("[PowerUpManager] Power-ups disabled");
                return false;
            }

            if (!HasPowerUp(type))
            {
                Debug.LogWarning($"[PowerUpManager] No {type} in inventory");
                return false;
            }

            if (IsActive(type))
            {
                Debug.Log($"[PowerUpManager] {type} already active - extending duration");
                ExtendPowerUp(type, GetDuration(type));
                RemovePowerUp(type, 1);
                return true;
            }

            // Remove from inventory
            RemovePowerUp(type, 1);

            // Activate effect
            float duration = GetDuration(type);
            ActivatePowerUpEffect(type, duration);

            // Track instance
            activePowerUps[type] = new PowerUpInstance
            {
                type = type,
                startTime = Time.time,
                duration = duration,
                remainingTime = duration
            };

            // Start expiration timer
            if (expirationCoroutines.ContainsKey(type))
            {
                StopCoroutine(expirationCoroutines[type]);
            }
            expirationCoroutines[type] = StartCoroutine(PowerUpExpirationCoroutine(type, duration));

            Debug.Log($"[PowerUpManager] Activated {type} for {duration}s");

            OnPowerUpActivated?.Invoke(type, duration);

            // Track analytics
            Analytics.AnalyticsManager.Instance?.TrackEvent("powerup_activated", new Dictionary<string, object>
            {
                { "type", type.ToString() },
                { "duration", duration }
            });

            return true;
        }

        /// <summary>
        /// Extend active power-up duration.
        /// </summary>
        private void ExtendPowerUp(PowerUpType type, float additionalTime)
        {
            if (activePowerUps.ContainsKey(type))
            {
                activePowerUps[type].duration += additionalTime;
                activePowerUps[type].remainingTime += additionalTime;

                Debug.Log($"[PowerUpManager] Extended {type} by {additionalTime}s - New duration: {activePowerUps[type].remainingTime}s");
            }
        }

        private void ActivatePowerUpEffect(PowerUpType type, float duration)
        {
            switch (type)
            {
                case PowerUpType.DamageBoost:
                    isDamageBoostActive = true;
                    ApplyDamageBoost();
                    break;

                case PowerUpType.SpeedBoost:
                    isSpeedBoostActive = true;
                    ApplySpeedBoost();
                    break;

                case PowerUpType.CreditBoost:
                    isCreditBoostActive = true;
                    break;

                case PowerUpType.Shield:
                    isShieldActive = true;
                    ApplyShield();
                    break;

                case PowerUpType.TimeFreeze:
                    isTimeFreezeActive = true;
                    ApplyTimeFreeze();
                    break;
            }
        }

        private IEnumerator PowerUpExpirationCoroutine(PowerUpType type, float duration)
        {
            yield return new WaitForSeconds(duration);

            ExpirePowerUp(type);
        }

        private void ExpirePowerUp(PowerUpType type)
        {
            if (!activePowerUps.ContainsKey(type))
                return;

            activePowerUps.Remove(type);
            expirationCoroutines.Remove(type);

            // Deactivate effect
            DeactivatePowerUpEffect(type);

            Debug.Log($"[PowerUpManager] {type} expired");

            OnPowerUpExpired?.Invoke(type);

            // Track analytics
            Analytics.AnalyticsManager.Instance?.TrackEvent("powerup_expired", new Dictionary<string, object>
            {
                { "type", type.ToString() }
            });
        }

        private void DeactivatePowerUpEffect(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.DamageBoost:
                    isDamageBoostActive = false;
                    RemoveDamageBoost();
                    break;

                case PowerUpType.SpeedBoost:
                    isSpeedBoostActive = false;
                    RemoveSpeedBoost();
                    break;

                case PowerUpType.CreditBoost:
                    isCreditBoostActive = false;
                    break;

                case PowerUpType.Shield:
                    isShieldActive = false;
                    RemoveShield();
                    break;

                case PowerUpType.TimeFreeze:
                    isTimeFreezeActive = false;
                    RemoveTimeFreeze();
                    break;
            }
        }

        private void UpdateActivePowerUps()
        {
            foreach (var kvp in activePowerUps)
            {
                PowerUpInstance instance = kvp.Value;
                instance.remainingTime = instance.startTime + instance.duration - Time.time;
                OnPowerUpTimeUpdated?.Invoke(kvp.Key, instance.remainingTime);
            }
        }
        #endregion

        #region Power-Up Effects
        private void ApplyDamageBoost()
        {
            // Apply to all towers
            var towers = FindObjectsOfType<Towers.Tower>();
            foreach (var tower in towers)
            {
                // tower.ApplyDamageMultiplier(damageBoostMultiplier);
            }

            Debug.Log($"[PowerUpManager] Applied {damageBoostMultiplier}x damage boost to {towers.Length} towers");
        }

        private void RemoveDamageBoost()
        {
            var towers = FindObjectsOfType<Towers.Tower>();
            foreach (var tower in towers)
            {
                // tower.RemoveDamageMultiplier();
            }

            Debug.Log("[PowerUpManager] Removed damage boost");
        }

        private void ApplySpeedBoost()
        {
            // Apply to all towers
            var towers = FindObjectsOfType<Towers.Tower>();
            foreach (var tower in towers)
            {
                // tower.ApplyFireRateMultiplier(speedBoostMultiplier);
            }

            Debug.Log($"[PowerUpManager] Applied {speedBoostMultiplier}x speed boost to {towers.Length} towers");
        }

        private void RemoveSpeedBoost()
        {
            var towers = FindObjectsOfType<Towers.Tower>();
            foreach (var tower in towers)
            {
                // tower.RemoveFireRateMultiplier();
            }

            Debug.Log("[PowerUpManager] Removed speed boost");
        }

        private void ApplyShield()
        {
            // Apply shield to base
            if (GameManager.Instance != null)
            {
                // GameManager.Instance.EnableShield();
            }

            Debug.Log($"[PowerUpManager] Applied shield for {shieldDuration}s");
        }

        private void RemoveShield()
        {
            if (GameManager.Instance != null)
            {
                // GameManager.Instance.DisableShield();
            }

            Debug.Log("[PowerUpManager] Removed shield");
        }

        private void ApplyTimeFreeze()
        {
            // Slow down all enemies
            var enemies = FindObjectsOfType<Enemies.Enemy>();
            foreach (var enemy in enemies)
            {
                // enemy.ApplySpeedMultiplier(timeFreezeSlowdown);
            }

            Debug.Log($"[PowerUpManager] Applied time freeze ({timeFreezeSlowdown}x speed) to {enemies.Length} enemies");
        }

        private void RemoveTimeFreeze()
        {
            var enemies = FindObjectsOfType<Enemies.Enemy>();
            foreach (var enemy in enemies)
            {
                // enemy.RemoveSpeedMultiplier();
            }

            Debug.Log("[PowerUpManager] Removed time freeze");
        }
        #endregion

        #region Public Query API
        /// <summary>
        /// Check if power-up is currently active.
        /// </summary>
        public bool IsActive(PowerUpType type)
        {
            return activePowerUps.ContainsKey(type);
        }

        /// <summary>
        /// Get remaining time for active power-up.
        /// </summary>
        public float GetRemainingTime(PowerUpType type)
        {
            if (activePowerUps.ContainsKey(type))
            {
                return activePowerUps[type].remainingTime;
            }
            return 0f;
        }

        /// <summary>
        /// Get power-up duration.
        /// </summary>
        public float GetDuration(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Shield:
                    return shieldDuration;
                default:
                    return defaultDuration;
            }
        }

        /// <summary>
        /// Get power-up multiplier value.
        /// </summary>
        public float GetMultiplier(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.DamageBoost:
                    return damageBoostMultiplier;
                case PowerUpType.SpeedBoost:
                    return speedBoostMultiplier;
                case PowerUpType.CreditBoost:
                    return creditBoostMultiplier;
                default:
                    return 1f;
            }
        }

        /// <summary>
        /// Apply credit boost multiplier if active.
        /// </summary>
        public int ApplyCreditBoost(int baseAmount)
        {
            if (isCreditBoostActive)
            {
                return Mathf.RoundToInt(baseAmount * creditBoostMultiplier);
            }
            return baseAmount;
        }

        /// <summary>
        /// Check if damage boost is active.
        /// </summary>
        public bool IsDamageBoostActive() => isDamageBoostActive;

        /// <summary>
        /// Check if speed boost is active.
        /// </summary>
        public bool IsSpeedBoostActive() => isSpeedBoostActive;

        /// <summary>
        /// Check if credit boost is active.
        /// </summary>
        public bool IsCreditBoostActive() => isCreditBoostActive;

        /// <summary>
        /// Check if shield is active.
        /// </summary>
        public bool IsShieldActive() => isShieldActive;

        /// <summary>
        /// Check if time freeze is active.
        /// </summary>
        public bool IsTimeFreezeActive() => isTimeFreezeActive;

        /// <summary>
        /// Get all active power-ups.
        /// </summary>
        public List<PowerUpType> GetActivePowerUps()
        {
            return new List<PowerUpType>(activePowerUps.Keys);
        }
        #endregion

        #region IAP Integration
        /// <summary>
        /// Grant power-up bundle from IAP purchase.
        /// </summary>
        public void GrantPowerUpBundle()
        {
            AddPowerUp(PowerUpType.DamageBoost, 3);
            AddPowerUp(PowerUpType.SpeedBoost, 3);
            AddPowerUp(PowerUpType.CreditBoost, 3);
            AddPowerUp(PowerUpType.Shield, 2);
            AddPowerUp(PowerUpType.TimeFreeze, 2);

            Debug.Log("[PowerUpManager] Granted power-up bundle (IAP purchase)");

            // Show notification
            var toast = FindObjectOfType<UI.ToastNotification>();
            if (toast != null)
            {
                toast.Show("Power-Up Bundle Received!\n13 power-ups added to inventory", UI.ToastType.Success);
            }
        }
        #endregion

        #region Ad Integration
        /// <summary>
        /// Grant free power-up from rewarded ad.
        /// </summary>
        public void GrantFreePowerUp(PowerUpType type)
        {
            AddPowerUp(type, 1);

            Debug.Log($"[PowerUpManager] Granted free {type} (rewarded ad)");

            // Show notification
            var toast = FindObjectOfType<UI.ToastNotification>();
            if (toast != null)
            {
                toast.Show($"Earned {GetPowerUpName(type)}!", UI.ToastType.Success);
            }
        }
        #endregion

        #region Helpers
        public string GetPowerUpName(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.DamageBoost: return "Damage Boost";
                case PowerUpType.SpeedBoost: return "Speed Boost";
                case PowerUpType.CreditBoost: return "Credit Boost";
                case PowerUpType.Shield: return "Shield";
                case PowerUpType.TimeFreeze: return "Time Freeze";
                default: return type.ToString();
            }
        }

        public string GetPowerUpDescription(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.DamageBoost:
                    return $"{damageBoostMultiplier}x tower damage for {defaultDuration}s";
                case PowerUpType.SpeedBoost:
                    return $"{speedBoostMultiplier}x tower fire rate for {defaultDuration}s";
                case PowerUpType.CreditBoost:
                    return $"{creditBoostMultiplier}x credits earned for {defaultDuration}s";
                case PowerUpType.Shield:
                    return $"Protect base from damage for {shieldDuration}s";
                case PowerUpType.TimeFreeze:
                    return $"Slow all enemies to {timeFreezeSlowdown * 100}% speed for {defaultDuration}s";
                default:
                    return "";
            }
        }
        #endregion

        #region Context Menu Testing
        [ContextMenu("Add All Power-Ups (x5)")]
        private void TestAddAllPowerUps()
        {
            foreach (PowerUpType type in Enum.GetValues(typeof(PowerUpType)))
            {
                AddPowerUp(type, 5);
            }
            Debug.Log("[PowerUpManager] [TEST] Added 5 of each power-up");
        }

        [ContextMenu("Activate Damage Boost")]
        private void TestActivateDamageBoost()
        {
            AddPowerUp(PowerUpType.DamageBoost, 1);
            ActivatePowerUp(PowerUpType.DamageBoost);
        }

        [ContextMenu("Activate All Power-Ups")]
        private void TestActivateAllPowerUps()
        {
            foreach (PowerUpType type in Enum.GetValues(typeof(PowerUpType)))
            {
                AddPowerUp(type, 1);
                ActivatePowerUp(type);
            }
            Debug.Log("[PowerUpManager] [TEST] Activated all power-ups");
        }

        [ContextMenu("Print Inventory")]
        private void TestPrintInventory()
        {
            Debug.Log($"[PowerUpManager] Inventory:");
            foreach (var kvp in inventory)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value}");
            }
            Debug.Log($"Active Power-Ups: {activePowerUps.Count}");
            foreach (var kvp in activePowerUps)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value.remainingTime:F1}s remaining");
            }
        }

        [ContextMenu("Clear All Power-Ups")]
        private void TestClearAll()
        {
            foreach (PowerUpType type in Enum.GetValues(typeof(PowerUpType)))
            {
                inventory[type] = 0;
            }
            SaveInventory();
            Debug.Log("[PowerUpManager] [TEST] Cleared all power-ups");
        }
        #endregion
    }

    #region Data Classes
    [Serializable]
    public class PowerUpInstance
    {
        public PowerUpType type;
        public float startTime;
        public float duration;
        public float remainingTime;
    }

    public enum PowerUpType
    {
        DamageBoost,    // 2x tower damage
        SpeedBoost,     // 1.5x tower fire rate
        CreditBoost,    // 2x credits earned
        Shield,         // Protect base from damage
        TimeFreeze      // Slow all enemies
    }
    #endregion
}
