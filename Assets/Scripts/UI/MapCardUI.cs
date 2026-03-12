using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// UI card component for displaying a custom map in the library list.
/// </summary>
public class MapCardUI : MonoBehaviour
{
    #region UI References
    [Header("Main Info")]
    [SerializeField] private TextMeshProUGUI mapNameText;
    [SerializeField] private TextMeshProUGUI authorNameText;
    [SerializeField] private RawImage thumbnailImage;
    [SerializeField] private Image placeholderImage;

    [Header("Map Details")]
    [SerializeField] private TextMeshProUGUI gridSizeText;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private Image difficultyIcon;
    [SerializeField] private Color[] difficultyColors = new Color[5]
    {
        new Color(0.5f, 1f, 0.5f), // Tutorial - Green
        new Color(0.7f, 1f, 0.7f), // Easy - Light Green  
        new Color(1f, 1f, 0.5f),   // Normal - Yellow
        new Color(1f, 0.7f, 0.3f), // Hard - Orange
        new Color(1f, 0.3f, 0.3f)  // Expert - Red
    };

    [Header("Statistics")]
    [SerializeField] private TextMeshProUGUI playCountText;
    [SerializeField] private TextMeshProUGUI ratingText;
    [SerializeField] private GameObject starRatingPanel;
    [SerializeField] private Image[] starImages;

    [Header("Metadata")]
    [SerializeField] private TextMeshProUGUI dateModifiedText;
    [SerializeField] private TextMeshProUGUI fileSizeText;

    [Header("Selection")]
    [SerializeField] private Button cardButton;
    [SerializeField] private Image selectionOutline;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [Header("Animation")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float animationDuration = 0.2f;
    #endregion

    #region Private Fields
    private CustomMapMetadata mapData;
    private Action<CustomMapMetadata> onClickCallback;
    private bool isSelected = false;
    private Vector3 originalScale;
    #endregion

    #region Lifecycle
    private void Awake()
    {
        originalScale = transform.localScale;

        if (cardButton != null)
        {
            cardButton.onClick.AddListener(OnCardClicked);
        }

        if (selectionOutline != null)
        {
            selectionOutline.color = normalColor;
        }
    }
    #endregion

    #region Setup
    /// <summary>
    /// Initializes the card with map data.
    /// </summary>
    public void Setup(CustomMapMetadata metadata, Action<CustomMapMetadata> onClick)
    {
        mapData = metadata;
        onClickCallback = onClick;

        UpdateDisplay();
        LoadThumbnail();
    }

    private void UpdateDisplay()
    {
        if (mapData == null) return;

        // Map name
        if (mapNameText != null)
        {
            mapNameText.text = mapData.mapName;
        }

        // Author name
        if (authorNameText != null)
        {
            authorNameText.text = $"by {mapData.authorName}";
        }

        // Grid size
        if (gridSizeText != null)
        {
            gridSizeText.text = $"{mapData.gridWidth}x{mapData.gridHeight}";
        }

        // Difficulty
        UpdateDifficultyDisplay();

        // Play count
        if (playCountText != null)
        {
            playCountText.text = $"{mapData.playCount} plays";
        }

        // Rating
        UpdateRatingDisplay();

        // Date modified
        if (dateModifiedText != null)
        {
            dateModifiedText.text = FormatDate(mapData.lastModifiedDate);
        }

        // File size
        if (fileSizeText != null)
        {
            fileSizeText.text = FormatBytes(mapData.fileSize);
        }
    }

    private void UpdateDifficultyDisplay()
    {
        string difficultyName = GetDifficultyName(mapData.difficulty);
        Color difficultyColor = GetDifficultyColor(mapData.difficulty);

        if (difficultyText != null)
        {
            difficultyText.text = difficultyName;
            difficultyText.color = difficultyColor;
        }

        if (difficultyIcon != null)
        {
            difficultyIcon.color = difficultyColor;
        }
    }

    private void UpdateRatingDisplay()
    {
        if (ratingText != null)
        {
            ratingText.text = $"{mapData.rating:F1}/5.0";
        }

        // Update star rating visual
        if (starRatingPanel != null && starImages != null && starImages.Length > 0)
        {
            starRatingPanel.SetActive(mapData.playCount > 0);

            int fullStars = Mathf.FloorToInt(mapData.rating);
            float partialStar = mapData.rating - fullStars;

            for (int i = 0; i < starImages.Length && i < 5; i++)
            {
                if (starImages[i] != null)
                {
                    if (i < fullStars)
                    {
                        // Full star
                        starImages[i].fillAmount = 1f;
                        starImages[i].color = Color.yellow;
                    }
                    else if (i == fullStars && partialStar > 0)
                    {
                        // Partial star
                        starImages[i].fillAmount = partialStar;
                        starImages[i].color = Color.yellow;
                    }
                    else
                    {
                        // Empty star
                        starImages[i].fillAmount = 0f;
                        starImages[i].color = Color.gray;
                    }
                }
            }
        }
    }

    private void LoadThumbnail()
    {
        if (mapData == null) return;

        if (mapData.hasThumbnail && CustomMapStorage.Instance != null)
        {
            Texture2D thumbnail = CustomMapStorage.Instance.LoadThumbnail(mapData.mapId);

            if (thumbnail != null && thumbnailImage != null)
            {
                thumbnailImage.texture = thumbnail;
                thumbnailImage.gameObject.SetActive(true);

                if (placeholderImage != null)
                {
                    placeholderImage.gameObject.SetActive(false);
                }

                return;
            }
        }

        // Show placeholder if no thumbnail
        if (thumbnailImage != null)
        {
            thumbnailImage.gameObject.SetActive(false);
        }

        if (placeholderImage != null)
        {
            placeholderImage.gameObject.SetActive(true);
        }
    }
    #endregion

    #region Interaction
    private void OnCardClicked()
    {
        onClickCallback?.Invoke(mapData);
        SetSelected(true);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectionOutline != null)
        {
            selectionOutline.color = selected ? selectedColor : normalColor;
        }
    }

    public void OnPointerEnter()
    {
        if (!isSelected)
        {
            LeanTween.scale(gameObject, originalScale * hoverScale, animationDuration)
                .setEase(LeanTweenType.easeOutQuad);
        }
    }

    public void OnPointerExit()
    {
        if (!isSelected)
        {
            LeanTween.scale(gameObject, originalScale, animationDuration)
                .setEase(LeanTweenType.easeOutQuad);
        }
    }
    #endregion

    #region Helpers
    private string GetDifficultyName(int difficulty)
    {
        switch (difficulty)
        {
            case 1: return "Tutorial";
            case 2: return "Easy";
            case 3: return "Normal";
            case 4: return "Hard";
            case 5: return "Expert";
            default: return "Normal";
        }
    }

    private Color GetDifficultyColor(int difficulty)
    {
        int index = Mathf.Clamp(difficulty - 1, 0, difficultyColors.Length - 1);
        return difficultyColors[index];
    }

    private string FormatDate(string dateString)
    {
        if (DateTime.TryParse(dateString, out DateTime date))
        {
            TimeSpan timeSince = DateTime.Now - date;

            if (timeSince.TotalDays < 1)
            {
                if (timeSince.TotalHours < 1)
                {
                    return $"{(int)timeSince.TotalMinutes}m ago";
                }
                return $"{(int)timeSince.TotalHours}h ago";
            }
            else if (timeSince.TotalDays < 7)
            {
                return $"{(int)timeSince.TotalDays}d ago";
            }
            else if (timeSince.TotalDays < 30)
            {
                return $"{(int)(timeSince.TotalDays / 7)}w ago";
            }
            else
            {
                return date.ToString("MMM dd, yyyy");
            }
        }

        return dateString;
    }

    private string FormatBytes(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        else if (bytes < 1024 * 1024)
            return $"{bytes / 1024f:F1} KB";
        else
            return $"{bytes / (1024f * 1024f):F1} MB";
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Gets the map data associated with this card.
    /// </summary>
    public CustomMapMetadata GetMapData()
    {
        return mapData;
    }

    /// <summary>
    /// Refreshes the card display with current data.
    /// </summary>
    public void Refresh()
    {
        UpdateDisplay();
    }
    #endregion
}
