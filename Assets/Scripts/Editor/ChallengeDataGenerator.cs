using UnityEngine;
using UnityEditor;
using RobotTD.Core;
using System.IO;

namespace RobotTD.Editor
{
    /// <summary>
    /// Editor utility to generate example challenge data assets.
    /// </summary>
    public static class ChallengeDataGenerator
    {
        private const string CHALLENGE_PATH = "Assets/Resources/Data/Challenges";
        
        [MenuItem("Robot TD/Generate Example Challenges")]
        public static void GenerateExampleChallenges()
        {
            // Ensure directory exists
            if (!Directory.Exists(CHALLENGE_PATH))
            {
                Directory.CreateDirectory(CHALLENGE_PATH);
            }
            
            int count = 0;
            
            // Daily Challenges (Easy-Medium)
            count += CreateDailyChallenge("challenge_daily_01", "Speed Rush", 
                "Enemies move 50% faster. Quick reflexes required!",
                new[] { ChallengeModifier.SpeedRush },
                DifficultyTier.Easy, 300, 5, 0);
                
            count += CreateDailyChallenge("challenge_daily_02", "Budget Warriors",
                "Start with 50% credits. Economy management is key!",
                new[] { ChallengeModifier.StartingDebt, ChallengeModifier.FastForward },
                DifficultyTier.Medium, 400, 8, 1);
                
            count += CreateDailyChallenge("challenge_daily_03", "Rapid Fire",
                "No breaks between waves. Constant pressure!",
                new[] { ChallengeModifier.NoBreaks },
                DifficultyTier.Easy, 350, 6, 2);
                
            count += CreateDailyChallenge("challenge_daily_04", "Armored Horde",
                "Enemies have double HP. Build for sustained damage!",
                new[] { ChallengeModifier.ArmoredAssault },
                DifficultyTier.Medium, 450, 10, 3);
            
            // Weekly Challenges (Medium-Hard)
            count += CreateWeeklyChallenge("challenge_weekly_01", "Limited Arsenal",
                "Only 3 tower types available. Adapt your strategy!",
                new[] { ChallengeModifier.LimitedArsenal, ChallengeModifier.SwarmMode },
                DifficultyTier.Hard, 800, 20, 0);
                
            count += CreateWeeklyChallenge("challenge_weekly_02", "Economic Crisis",
                "Half credits per kill and towers cost more. Every decision counts!",
                new[] { ChallengeModifier.EconomicHardship, ChallengeModifier.BudgetCrisis },
                DifficultyTier.Hard, 900, 25, 1);
                
            count += CreateWeeklyChallenge("challenge_weekly_03", "Tower Limit",
                "Maximum 10 towers. Placement is crucial!",
                new[] { ChallengeModifier.TowerLimit, ChallengeModifier.SpeedRush },
                DifficultyTier.Hard, 850, 22, 2);
                
            count += CreateWeeklyChallenge("challenge_weekly_04", "Weakened Defense",
                "Towers deal 30% less damage. Efficiency is key!",
                new[] { ChallengeModifier.WeakenedTowers, ChallengeModifier.ArmoredAssault },
                DifficultyTier.Hard, 900, 25, 3);
            
            // Permanent Challenges (All Difficulties)
            count += CreatePermanentChallenge("challenge_perm_01", "Perfect Defense",
                "One life only. A single mistake ends the run!",
                new[] { ChallengeModifier.PerfectDefense },
                DifficultyTier.Extreme, 2000, 50, "ach_perfect_defense");
                
            count += CreatePermanentChallenge("challenge_perm_02", "Speed Master",
                "Fast enemies with no wave breaks. Ultimate test of reflexes!",
                new[] { ChallengeModifier.SpeedRush, ChallengeModifier.NoBreaks, ChallengeModifier.SwarmMode },
                DifficultyTier.Extreme, 1800, 45, "ach_speed_master");
                
            count += CreatePermanentChallenge("challenge_perm_03", "Minimalist",
                "Only 10 towers, limited arsenal, weak towers. Maximum strategy!",
                new[] { ChallengeModifier.TowerLimit, ChallengeModifier.LimitedArsenal, ChallengeModifier.WeakenedTowers },
                DifficultyTier.Extreme, 2200, 55, "ach_minimalist");
                
            count += CreatePermanentChallenge("challenge_perm_04", "Boss Rush Easy",
                "Face bosses more frequently. Great for practice!",
                new[] { ChallengeModifier.BossRush },
                DifficultyTier.Easy, 400, 8, "");
                
            count += CreatePermanentChallenge("challenge_perm_05", "Economy Master",
                "All economy modifiers active. Can you manage?",
                new[] { ChallengeModifier.EconomicHardship, ChallengeModifier.StartingDebt, ChallengeModifier.BudgetCrisis },
                DifficultyTier.Hard, 1200, 35, "ach_economy_master");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[ChallengeDataGenerator] Created {count} example challenges in {CHALLENGE_PATH}");
            EditorUtility.DisplayDialog("Success", 
                $"Generated {count} example challenge assets!\n\nLocation: {CHALLENGE_PATH}", 
                "OK");
        }
        
        private static int CreateDailyChallenge(string id, string name, string description,
            ChallengeModifier[] modifiers, DifficultyTier difficulty, 
            int credits, int techPoints, int rotationIndex)
        {
            return CreateChallenge(id, name, description, modifiers, difficulty,
                credits, techPoints, "", ChallengeRotationType.Daily, rotationIndex);
        }
        
        private static int CreateWeeklyChallenge(string id, string name, string description,
            ChallengeModifier[] modifiers, DifficultyTier difficulty,
            int credits, int techPoints, int rotationIndex)
        {
            return CreateChallenge(id, name, description, modifiers, difficulty,
                credits, techPoints, "", ChallengeRotationType.Weekly, rotationIndex);
        }
        
        private static int CreatePermanentChallenge(string id, string name, string description,
            ChallengeModifier[] modifiers, DifficultyTier difficulty,
            int credits, int techPoints, string achievementId)
        {
            return CreateChallenge(id, name, description, modifiers, difficulty,
                credits, techPoints, achievementId, ChallengeRotationType.Permanent, 0);
        }
        
        private static int CreateChallenge(string id, string name, string description,
            ChallengeModifier[] modifiers, DifficultyTier difficulty,
            int credits, int techPoints, string achievementId,
            ChallengeRotationType rotationType, int rotationIndex)
        {
            string fileName = $"{id}.asset";
            string fullPath = Path.Combine(CHALLENGE_PATH, fileName);
            
            // Check if already exists
            ChallengeData existing = AssetDatabase.LoadAssetAtPath<ChallengeData>(fullPath);
            if (existing != null)
            {
                Debug.Log($"[ChallengeDataGenerator] Skipping {id} - already exists");
                return 0;
            }
            
            // Create new challenge
            ChallengeData challenge = ScriptableObject.CreateInstance<ChallengeData>();
            
            // Use reflection to set private fields (since they're SerializeField)
            var type = typeof(ChallengeData);
            
            type.GetField("challengeId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(challenge, id);
            type.GetField("challengeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(challenge, name);
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(challenge, description);
            type.GetField("mapId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(challenge, "map_factory"); // Default map
            type.GetField("modifiers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(challenge, modifiers);
            type.GetField("difficulty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(challenge, difficulty);
            type.GetField("creditReward", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(challenge, credits);
            type.GetField("techPointReward", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(challenge, techPoints);
            type.GetField("achievementId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(challenge, achievementId);
            type.GetField("rotationType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(challenge, rotationType);
            type.GetField("rotationIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(challenge, rotationIndex);
            
            // Create asset
            AssetDatabase.CreateAsset(challenge, fullPath);
            
            Debug.Log($"[ChallengeDataGenerator] Created {name} ({id})");
            return 1;
        }
        
        [MenuItem("Robot TD/Clear All Challenges")]
        public static void ClearAllChallenges()
        {
            if (!Directory.Exists(CHALLENGE_PATH))
            {
                Debug.Log("[ChallengeDataGenerator] No challenges directory found");
                return;
            }
            
            bool confirm = EditorUtility.DisplayDialog("Confirm Delete",
                "Delete all challenge assets? This cannot be undone!",
                "Delete All", "Cancel");
                
            if (!confirm) return;
            
            string[] assets = AssetDatabase.FindAssets("t:ChallengeData", new[] { CHALLENGE_PATH });
            int count = 0;
            
            foreach (string guid in assets)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.DeleteAsset(path);
                count++;
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[ChallengeDataGenerator] Deleted {count} challenge assets");
        }
    }
}
