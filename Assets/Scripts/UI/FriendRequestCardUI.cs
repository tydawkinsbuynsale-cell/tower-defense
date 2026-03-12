using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotTD.Online;
using System;

namespace RobotTD.UI
{
    /// <summary>
    /// UI card component for displaying a pending friend request.
    /// </summary>
    public class FriendRequestCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button declineButton;

        private FriendRequest requestInfo;

        // Events
        public event Action<string> OnAccept;
        public event Action<string> OnDecline;

        private void Start()
        {
            if (acceptButton != null)
            {
                acceptButton.onClick.AddListener(HandleAccept);
            }

            if (declineButton != null)
            {
                declineButton.onClick.AddListener(HandleDecline);
            }
        }

        public void Setup(FriendRequest request)
        {
            requestInfo = request;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (requestInfo == null) return;

            // Player name
            if (playerNameText != null)
            {
                playerNameText.text = requestInfo.senderPlayerName;
            }

            // Time ago
            if (timeText != null)
            {
                TimeSpan timeSince = DateTime.UtcNow - requestInfo.timestamp;
                if (timeSince.TotalMinutes < 60)
                {
                    timeText.text = $"{(int)timeSince.TotalMinutes}m ago";
                }
                else if (timeSince.TotalHours < 24)
                {
                    timeText.text = $"{(int)timeSince.TotalHours}h ago";
                }
                else
                {
                    timeText.text = $"{(int)timeSince.TotalDays}d ago";
                }
            }
        }

        private void HandleAccept()
        {
            OnAccept?.Invoke(requestInfo.requestId);
            
            // Disable buttons to prevent double-click
            if (acceptButton != null) acceptButton.interactable = false;
            if (declineButton != null) declineButton.interactable = false;
        }

        private void HandleDecline()
        {
            OnDecline?.Invoke(requestInfo.requestId);
            
            // Disable buttons to prevent double-click
            if (acceptButton != null) acceptButton.interactable = false;
            if (declineButton != null) declineButton.interactable = false;
        }
    }
}
