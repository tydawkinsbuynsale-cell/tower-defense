using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotTD.Online;
using System;

namespace RobotTD.UI
{
    /// <summary>
    /// UI card component for displaying a friend in the friends list.
    /// </summary>
    public class FriendCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image onlineIndicator;
        [SerializeField] private Button viewProfileButton;
        [SerializeField] private Button removeFriendButton;
        [SerializeField] private Color onlineColor = Color.green;
        [SerializeField] private Color offlineColor = Color.gray;

        private FriendInfo friendInfo;

        // Events
        public event Action<string> OnViewProfile;
        public event Action<string> OnRemoveFriend;

        private void Start()
        {
            if (viewProfileButton != null)
            {
                viewProfileButton.onClick.AddListener(HandleViewProfile);
            }

            if (removeFriendButton != null)
            {
                removeFriendButton.onClick.AddListener(HandleRemoveFriend);
            }
        }

        public void Setup(FriendInfo friend)
        {
            friendInfo = friend;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (friendInfo == null) return;

            // Player name
            if (playerNameText != null)
            {
                playerNameText.text = friendInfo.playerName;
            }

            // Online status
            if (statusText != null)
            {
                if (friendInfo.isOnline)
                {
                    statusText.text = "Online";
                }
                else
                {
                    TimeSpan timeSince = DateTime.UtcNow - friendInfo.lastSeenOnline;
                    if (timeSince.TotalMinutes < 60)
                    {
                        statusText.text = $"Last seen {(int)timeSince.TotalMinutes}m ago";
                    }
                    else if (timeSince.TotalHours < 24)
                    {
                        statusText.text = $"Last seen {(int)timeSince.TotalHours}h ago";
                    }
                    else
                    {
                        statusText.text = $"Last seen {(int)timeSince.TotalDays}d ago";
                    }
                }
            }

            // Online indicator
            if (onlineIndicator != null)
            {
                onlineIndicator.color = friendInfo.isOnline ? onlineColor : offlineColor;
            }
        }

        private void HandleViewProfile()
        {
            OnViewProfile?.Invoke(friendInfo.playerId);
        }

        private void HandleRemoveFriend()
        {
            // Show confirmation dialog
            if (MessageBox.Instance != null)
            {
                MessageBox.Instance.Show(
                    "Remove Friend",
                    $"Are you sure you want to remove {friendInfo.playerName} from your friends?",
                    MessageBox.MessageBoxButtons.YesNo,
                    (result) => {
                        if (result == MessageBox.MessageBoxResult.Yes)
                        {
                            OnRemoveFriend?.Invoke(friendInfo.playerId);
                        }
                    }
                );
            }
            else
            {
                // No confirmation dialog available, remove directly
                OnRemoveFriend?.Invoke(friendInfo.playerId);
            }
        }
    }
}
