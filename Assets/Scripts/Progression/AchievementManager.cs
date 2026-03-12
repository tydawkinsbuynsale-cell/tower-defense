using UnityEngine;
using System;
using System.Collections.Generic;

namespace RobotTD.Progression
{
    public enum AchievementId
    {
        // Kill milestones
        KillsFirst100,
        Kills500,
        Kills2500,
        Kills10000,

        // Wave milestones
        CompleteWave10,
        CompleteWave30,
        SurviveAllWaves,

        // Level milestones
        ReachLevel5,
        ReachLevel10,
        ReachLevel25,

        // Build milestones
        PlaceFirstTower,
        Use5DifferentTowers,
        UseAllTowers,
        UpgradeTower,
        MaxUpgradeTower,

        // Challenge
        NoDamageTaken,          // Complete a wave without losing life
        SpeedRun,               // Complete a map within time limit
        NoTowerSell,            // Win without selling any tower
        BossKill,               // Kill a Boss enemy

        // Progression
        TechTreeFirstUpgrade,
        CompleteTechBranch,
    }

    [Serializable]
    public class AchievementDef
    {
        public AchievementId id;
        public string title;
        [TextArea(1, 2)] public string description;
        public Sprite icon;
        public int xpReward = 50;
        public int techPointReward = 0;
    }

    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        [SerializeField] private AchievementDef[] definitions;
        [SerializeField] private UI.AchievementToast toastUI; // assigned in inspector

        public event Action<AchievementDef> OnAchievementUnlocked;

        private HashSet<string> unlocked = new HashSet<string>();
        private Dictionary<AchievementId, AchievementDef> defMap
            = new Dictionary<AchievementId, AchievementDef>();

        // Session tracking (not persisted — recalc per session)
        private HashSet<Towers.TowerType> towersUsedThisSession = new HashSet<Towers.TowerType>();
        private bool soldTowerThisGame;
        private bool lostLifeThisWave;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var def in definitions)
                defMap[def.id] = def;
        }

        private void Start()
        {
            // Load persisted achievements
            var saved = Core.SaveManager.Instance?.Data.completedAchievements;
            if (saved != null)
                foreach (var s in saved)
                    unlocked.Add(s);
        }

        // ── Public check methods ─────────────────────────────────────────────

        public void CheckKillCount(int totalKills)
        {
            if (totalKills >= 100)   TryUnlock(AchievementId.KillsFirst100);
            if (totalKills >= 500)   TryUnlock(AchievementId.Kills500);
            if (totalKills >= 2500)  TryUnlock(AchievementId.Kills2500);
            if (totalKills >= 10000) TryUnlock(AchievementId.Kills10000);
        }

        public void CheckLevelUp(int level)
        {
            if (level >= 5)  TryUnlock(AchievementId.ReachLevel5);
            if (level >= 10) TryUnlock(AchievementId.ReachLevel10);
            if (level >= 25) TryUnlock(AchievementId.ReachLevel25);
        }

        public void CheckWaveComplete(int wave, bool livesIntact)
        {
            if (wave >= 10) TryUnlock(AchievementId.CompleteWave10);
            if (wave >= 30) TryUnlock(AchievementId.CompleteWave30);

            if (livesIntact && !lostLifeThisWave)
                TryUnlock(AchievementId.NoDamageTaken);

            lostLifeThisWave = false; // Reset for next wave
        }

        public void CheckVictory(float clearTimeSeconds)
        {
            TryUnlock(AchievementId.SurviveAllWaves);

            if (!soldTowerThisGame)
                TryUnlock(AchievementId.NoTowerSell);

            if (clearTimeSeconds < 600f)  // 10 min speed run threshold
                TryUnlock(AchievementId.SpeedRun);
        }

        public void OnTowerPlaced(Towers.TowerType type)
        {
            TryUnlock(AchievementId.PlaceFirstTower);
            towersUsedThisSession.Add(type);

            if (towersUsedThisSession.Count >= 5)
                TryUnlock(AchievementId.Use5DifferentTowers);

            // All 11 tower types used
            int totalTowerTypes = Enum.GetValues(typeof(Towers.TowerType)).Length;
            if (towersUsedThisSession.Count >= totalTowerTypes)
                TryUnlock(AchievementId.UseAllTowers);
        }

        public void OnTowerUpgraded(int upgradeLevel)
        {
            TryUnlock(AchievementId.UpgradeTower);
            if (upgradeLevel >= 3)
                TryUnlock(AchievementId.MaxUpgradeTower);
        }

        public void OnTowerSold()
        {
            soldTowerThisGame = true;
        }

        public void OnLifeLost()
        {
            lostLifeThisWave = true;
        }

        public void OnBossKilled()
        {
            TryUnlock(AchievementId.BossKill);
        }

        public void OnTechUpgrade()
        {
            TryUnlock(AchievementId.TechTreeFirstUpgrade);
        }

        // ── Core unlock ──────────────────────────────────────────────────────

        private void TryUnlock(AchievementId id)
        {
            string key = id.ToString();
            if (unlocked.Contains(key)) return;

            unlocked.Add(key);

            if (Core.SaveManager.Instance != null &&
                !Core.SaveManager.Instance.Data.completedAchievements.Contains(key))
            {
                Core.SaveManager.Instance.Data.completedAchievements.Add(key);
                Core.SaveManager.Instance.Save();
            }

            if (defMap.TryGetValue(id, out AchievementDef def))
            {
                // Grant rewards
                Core.SaveManager.Instance?.AddXP(def.xpReward);
                if (def.techPointReward > 0 && Core.SaveManager.Instance != null)
                    Core.SaveManager.Instance.Data.techPoints += def.techPointReward;

                // Show toast
                toastUI?.Show(def);

                // Fire event
                OnAchievementUnlocked?.Invoke(def);

                Debug.Log($"[Achievement] Unlocked: {def.title}");
            }
        }

        public bool IsUnlocked(AchievementId id) => unlocked.Contains(id.ToString());

        public List<AchievementDef> GetAll() => new List<AchievementDef>(definitions);
    }
}
