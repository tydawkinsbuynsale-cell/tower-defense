using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotTD.Online;
using System;

namespace RobotTD.UI
{
    /// <summary>
    /// UI card component for displaying a player search result.
    /// </summary>
    public class PlayerSearchResultCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image onlineIndicator;
        [SerializeField] private Button addFriendButton;
        [SerializeField] private TextMeshProUGUI friendStatusText;
        [SerializeField] private Color onlineColor = Color.green;
        [SerializeField] private Color offlineColor = Color.gray;

        private PlayerSearchResult resultInfo;

        // Events
        public event Action<string> OnSendFriendRequest;

        private void Start()
        {
            if (addFriendButton != null)
            {
                addFriendButton.onClick.AddListener(HandleAddFriend);
            }
        }

        public void Setup(PlayerSearchResult result)
        {
            resultInfo = result;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (resultInfo == null) return;

            // Player name
            if (playerNameText != null)
            {
                playerNameText.text = resultInfo.playerName;
            }

            // Online status
            if (statusText != null)
            {
                statusText.text = resultInfo.isOnline ? "Online" : "Offline";
            }

            // Online indicator
            if (onlineIndicator != null)
            {
                onlineIndicator.color = resultInfo.isOnline ? onlineColor : offlineColor;
            }

            // Friend status
            if (resultInfo.isFriend)
            {
                if (addFriendButton != null)
                {
                    addFriendButton.gameObject.SetActive(false);
                }
                if (friendStatusText != null)
                {
                    friendStatusText.gameObject.SetActive(true);
                    friendStatusText.text = "Already Friends";
                }
            }
            else
            {
                if (addFriendButton != null)
                {
                    addFriendButton.gameObject.SetActive(true);
                }
                if (friendStatusText != null)
                {
                    friendStatusText.gameObject.SetActive(false);
                }
            }
        }

        private void HandleAddFriend()
        {
            OnSendFriendRequest?.Invoke(resultInfo.playerId);
            
            // Disable button and show "Request Sent"
            if (addFriendButton != null)
            {
                addFriendButton.interactable = false;
                var buttonText = addFriendButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Request Sent";
                }
            }
        }
    }
}
