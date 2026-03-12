using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotTD.Online;

namespace RobotTD.UI
{
    /// <summary>
    /// Individual leaderboard entry UI — displays one player's score, rank, and name.
    /// </summary>
    public class LeaderboardEntryUI : MonoBehaviour
    {
        [Header("Text References")]
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text metadataText; // Wave # or challenge info
        
        [Header("Visual References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image rankIcon;
        
        [Header("Rank Icons (Optional)")]
        [SerializeField] private Sprite goldMedalSprite;
        [SerializeField] private Sprite silverMedalSprite;
        [SerializeField] private Sprite bronzeMedalSprite;
        
        [Header("Text Colors")]
        [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f);      // #FFD700
        [SerializeField] private Color silverColor = new Color(0.75f, 0.75f, 0.75f); // #C0C0C0
        [SerializeField] private Color bronzeColor = new Color(0.8f, 0.5f, 0.2f);  // #CD7F32
        [SerializeField] private Color normalColor = Color.white;
        
        private LeaderboardEntry entryData;
        
        // ── Public API ───────────────────────────────────────────────────────
        
        /// <summary>
        /// Set the data for this entry and update UI
        /// </summary>
        public void SetData(LeaderboardEntry entry, Color backgroundColor = default)
        {
            entryData = entry;
            
            UpdateRank();
            UpdateName();
            UpdateScore();
            UpdateMetadata();
            UpdateBackground(backgroundColor);
        }
        
        // ── Private Methods ──────────────────────────────────────────────────
        
        private void UpdateRank()
        {
            if (rankText == null)
                return;
            
            int rank = entryData.rank;
            
            // Format rank text
            if (rank <= 3)
            {
                // Top 3 get special formatting
                string rankStr = "";
                switch (rank)
                {
                    case 1: rankStr = "1st"; break;
                    case 2: rankStr = "2nd"; break;
                    case 3: rankStr = "3rd"; break;
                }
                rankText.text = rankStr;
                
                // Apply special colors
                Color textColor = normalColor;
                switch (rank)
                {
                    case 1: textColor = goldColor; break;
                    case 2: textColor = silverColor; break;
                    case 3: textColor = bronzeColor; break;
                }
                rankText.color = textColor;
                
                // Set rank icon if available
                if (rankIcon != null)
                {
                    rankIcon.gameObject.SetActive(true);
                    Sprite iconSprite = null;
                    switch (rank)
                    {
                        case 1: iconSprite = goldMedalSprite; break;
                        case 2: iconSprite = silverMedalSprite; break;
                        case 3: iconSprite = bronzeMedalSprite; break;
                    }
                    if (iconSprite != null)
                        rankIcon.sprite = iconSprite;
                }
            }
            else
            {
                // Regular ranks
                rankText.text = $"#{rank}";
                rankText.color = normalColor;
                
                if (rankIcon != null)
                    rankIcon.gameObject.SetActive(false);
            }
        }
        
        private void UpdateName()
        {
            if (nameText == null)
                return;
            
            nameText.text = entryData.playerName;
        }
        
        private void UpdateScore()
        {
            if (scoreText == null)
                return;
            
            // Format score with commas for readability
            scoreText.text = entryData.score.ToString("N0");
        }
        
        private void UpdateMetadata()
        {
            if (metadataText == null)
                return;
            
            // Check for wave number in metadata (endless mode)
            if (entryData.metadata != null && entryData.metadata.TryGetValue("wave", out string waveStr))
            {
                metadataText.text = $"Wave {waveStr}";
                metadataText.gameObject.SetActive(true);
            }
            // Check for challenge date
            else if (entryData.metadata != null && entryData.metadata.TryGetValue("challenge_date", out string dateStr))
            {
                metadataText.text = dateStr;
                metadataText.gameObject.SetActive(true);
            }
            else
            {
                metadataText.gameObject.SetActive(false);
            }
        }
        
        private void UpdateBackground(Color backgroundColor)
        {
            if (backgroundImage == null)
                return;
            
            if (backgroundColor == default(Color))
                backgroundColor = Color.clear;
            
            backgroundImage.color = backgroundColor;
        }
        
        // ── Debug ────────────────────────────────────────────────────────────
        
        [ContextMenu("Test Entry (Gold)")]
        private void TestGoldEntry()
        {
            var testEntry = new LeaderboardEntry
            {
                rank = 1,
                playerName = "TestPlayer1",
                score = 100000,
                metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "wave", "50" }
                }
            };
            SetData(testEntry, new Color(1f, 0.92f, 0.016f, 0.3f));
        }
        
        [ContextMenu("Test Entry (Normal)")]
        private void TestNormalEntry()
        {
            var testEntry = new LeaderboardEntry
            {
                rank = 10,
                playerName = "TestPlayer10",
                score = 50000,
                metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "wave", "25" }
                }
            };
            SetData(testEntry);
        }
    }
}
