using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotTD.Online;

namespace RobotTD.UI
{
    /// <summary>
    /// Player name input dialog — shown on first launch or when changing name.
    /// Validates and submits name to LeaderboardManager.
    /// </summary>
    public class PlayerNameDialog : MonoBehaviour
    {
        public static PlayerNameDialog Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private TMP_Text characterCountText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button randomNameButton;
        
        [Header("Validation")]
        [SerializeField] private int minNameLength = 3;
        [SerializeField] private int maxNameLength = 20;
        [SerializeField] private bool allowSkip = false;
        
        [Header("Random Name Lists")]
        [SerializeField] private string[] randomPrefixes = new string[]
        {
            "Captain", "General", "Commander", "Major", "Admiral", "Chief",
            "Mega", "Super", "Ultra", "Hyper", "Cyber", "Neo",
            "Iron", "Steel", "Titanium", "Copper", "Bronze", "Silver"
        };
        [SerializeField] private string[] randomSuffixes = new string[]
        {
            "Bot", "Mech", "Drone", "Tank", "Turret", "Defender",
            "Guardian", "Warrior", "Hero", "Striker", "Hunter", "Ranger"
        };
        
        private System.Action<string> onNameConfirmed;
        private bool isFirstLaunch;
        
        // ── Unity ────────────────────────────────────────────────────────────
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);
            
            if (randomNameButton != null)
                randomNameButton.onClick.AddListener(OnRandomNameClicked);
            
            if (nameInputField != null)
                nameInputField.onValueChanged.AddListener(OnNameInputChanged);
            
            if (dialogPanel != null)
                dialogPanel.SetActive(false);
        }
        
        // ── Public API ───────────────────────────────────────────────────────
        
        /// <summary>
        /// Show the name input dialog
        /// </summary>
        /// <param name="isFirstLaunch">If true, cancel button is hidden (required input)</param>
        /// <param name="currentName">Pre-fill with current name (for editing)</param>
        /// <param name="onConfirm">Callback when name is confirmed</param>
        public void Show(bool isFirstLaunch = false, string currentName = "", System.Action<string> onConfirm = null)
        {
            this.isFirstLaunch = isFirstLaunch;
            this.onNameConfirmed = onConfirm;
            
            if (dialogPanel != null)
                dialogPanel.SetActive(true);
            
            // Pre-fill with current name or generate random
            if (!string.IsNullOrEmpty(currentName))
            {
                if (nameInputField != null)
                    nameInputField.text = currentName;
            }
            else if (isFirstLaunch)
            {
                // Generate random name for first launch
                OnRandomNameClicked();
            }
            
            // Hide cancel button on first launch
            if (cancelButton != null)
                cancelButton.gameObject.SetActive(!isFirstLaunch || allowSkip);
            
            // Clear error
            HideError();
            
            // Update character count
            UpdateCharacterCount();
            
            // Focus input field
            if (nameInputField != null)
                nameInputField.Select();
        }
        
        /// <summary>
        /// Hide the dialog
        /// </summary>
        public void Hide()
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(false);
        }
        
        // ── Private Methods ──────────────────────────────────────────────────
        
        private void OnConfirmClicked()
        {
            if (nameInputField == null)
                return;
            
            string inputName = nameInputField.text.Trim();
            
            // Validate name
            if (!ValidateName(inputName, out string errorMessage))
            {
                ShowError(errorMessage);
                return;
            }
            
            // Submit to LeaderboardManager
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.SetPlayerName(inputName);
            }
            
            // Invoke callback
            onNameConfirmed?.Invoke(inputName);
            
            // Hide dialog
            Hide();
            
            Debug.Log($"[PlayerNameDialog] Name set to: {inputName}");
        }
        
        private void OnCancelClicked()
        {
            Hide();
        }
        
        private void OnRandomNameClicked()
        {
            string randomName = GenerateRandomName();
            if (nameInputField != null)
                nameInputField.text = randomName;
        }
        
        private void OnNameInputChanged(string newName)
        {
            UpdateCharacterCount();
            HideError();
        }
        
        private bool ValidateName(string name, out string errorMessage)
        {
            errorMessage = "";
            
            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = "Name cannot be empty";
                return false;
            }
            
            if (name.Length < minNameLength)
            {
                errorMessage = $"Name must be at least {minNameLength} characters";
                return false;
            }
            
            if (name.Length > maxNameLength)
            {
                errorMessage = $"Name must be no more than {maxNameLength} characters";
                return false;
            }
            
            // Check for invalid characters (LeaderboardManager.SanitizeName will clean, but warn here)
            if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9_\s]+$"))
            {
                errorMessage = "Name can only contain letters, numbers, spaces, and underscores";
                return false;
            }
            
            return true;
        }
        
        private string GenerateRandomName()
        {
            if (randomPrefixes.Length == 0 || randomSuffixes.Length == 0)
                return "Player" + Random.Range(1000, 9999);
            
            string prefix = randomPrefixes[Random.Range(0, randomPrefixes.Length)];
            string suffix = randomSuffixes[Random.Range(0, randomSuffixes.Length)];
            string number = Random.Range(10, 99).ToString();
            
            return $"{prefix}{suffix}{number}";
        }
        
        private void UpdateCharacterCount()
        {
            if (characterCountText == null || nameInputField == null)
                return;
            
            int currentLength = nameInputField.text.Length;
            characterCountText.text = $"{currentLength} / {maxNameLength}";
            
            // Color based on validity
            if (currentLength < minNameLength)
                characterCountText.color = Color.red;
            else if (currentLength > maxNameLength)
                characterCountText.color = Color.red;
            else
                characterCountText.color = Color.green;
        }
        
        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }
        }
        
        private void HideError()
        {
            if (errorText != null)
                errorText.gameObject.SetActive(false);
        }
        
        // ── Auto-Show on First Launch ────────────────────────────────────────
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CheckFirstLaunch()
        {
            // Check if player name exists, if not show dialog on first launch
            if (LeaderboardManager.Instance != null)
            {
                string playerName = LeaderboardManager.Instance.GetPlayerName();
                
                // Auto-generated names start with "Player" - consider those as not set
                if (playerName.StartsWith("Player") && playerName.Length < 10)
                {
                    // Wait a frame to ensure UI is ready
                    if (Instance != null)
                    {
                        Instance.Invoke(nameof(ShowFirstLaunchDialog), 0.5f);
                    }
                }
            }
        }
        
        private void ShowFirstLaunchDialog()
        {
            Show(isFirstLaunch: true, currentName: "", onConfirm: null);
        }
        
        // ── Debug ────────────────────────────────────────────────────────────
        
        [ContextMenu("Show Dialog (First Launch)")]
        private void ShowFirstLaunchDebug()
        {
            Show(isFirstLaunch: true);
        }
        
        [ContextMenu("Show Dialog (Edit Name)")]
        private void ShowEditNameDebug()
        {
            string currentName = LeaderboardManager.Instance?.GetPlayerName() ?? "TestPlayer";
            Show(isFirstLaunch: false, currentName: currentName);
        }
        
        [ContextMenu("Generate Random Name")]
        private void GenerateRandomNameDebug()
        {
            Debug.Log($"Random name: {GenerateRandomName()}");
        }
    }
}
