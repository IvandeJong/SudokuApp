using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CampaignLevelItem : MonoBehaviour
{
    [Header("Card")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Button cardButton;

    [Header("Number Badge")]
    [SerializeField] private TextMeshProUGUI numberText;

    [Header("Stars")]
    [SerializeField] private Image[] starImages;
    [SerializeField] private Color starFilledColor = new Color(0.984f, 0.75f, 0.141f, 1f); // #FBBF24
    [SerializeField] private Color starEmptyColor = new Color(0.796f, 0.835f, 0.882f, 1f);  // #CBD5E1

    [Header("Difficulty Pill")]
    [SerializeField] private Image pillBackground;
    [SerializeField] private TextMeshProUGUI difficultyText;

    [Header("Lock Status")]
    [SerializeField] private GameObject lockIcon;

    private int levelIndex;
    private System.Action<int> onPlayClicked;

    public void Setup(int index, CampaignLevelType levelType, CampaignDifficulty difficulty, CampaignManager.LevelProgress progress, bool isUnlocked, System.Action<int> playAction)
    {
        levelIndex = index;
        onPlayClicked = playAction;

        // 1. Number prefix (1, 2, 3...)
        if (numberText != null)
        {
            numberText.text = (index + 1).ToString();
        }

        // 2. Stars: grayed out and filled when completed
        int earnedStars = progress != null ? progress.stars : 0;
        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].gameObject.SetActive(true);
                    starImages[i].color = (i < earnedStars) ? starFilledColor : starEmptyColor;
                }
            }
        }

        // 3. Difficulty pill prefixed with type (2x2, 2x3, 3x3)
        string sizePrefix = GetGridSizePrefix(levelType);
        if (difficultyText != null)
        {
            difficultyText.text = $"{sizePrefix} {difficulty}";
            ApplyDifficultyColors(difficulty);
        }

        // 4. Lock state
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }

        if (cardButton != null)
        {
            cardButton.interactable = isUnlocked;
            cardButton.onClick.RemoveAllListeners();
            if (isUnlocked)
            {
                cardButton.onClick.AddListener(() => onPlayClicked?.Invoke(levelIndex));
            }
        }

        if (cardBackground != null)
        {
            cardBackground.color = isUnlocked ? Color.white : new Color(0.97f, 0.98f, 0.99f, 0.95f);
        }
    }

    public void Setup(int index, CampaignLevelType levelType, CampaignDifficulty difficulty, CampaignManager.LevelProgress progress, System.Action<int> playAction)
    {
        bool unlocked = CampaignManager.Instance != null && CampaignManager.Instance.IsLevelUnlocked(index);
        Setup(index, levelType, difficulty, progress, unlocked, playAction);
    }

    public static string GetGridSizePrefix(CampaignLevelType levelType)
    {
        switch (levelType)
        {
            case CampaignLevelType.Nano:
                return "2x2";
            case CampaignLevelType.Mini:
                return "2x3";
            default:
                return "3x3";
        }
    }

    private void ApplyDifficultyColors(CampaignDifficulty difficulty)
    {
        Color bgColor;
        Color textColor;

        switch (difficulty)
        {
            case CampaignDifficulty.Easy:
                bgColor = new Color(0.902f, 0.941f, 0.988f, 1f);  // #E6F0FA
                textColor = new Color(0.145f, 0.388f, 0.922f, 1f); // #2563EB
                break;
            case CampaignDifficulty.Medium:
                bgColor = new Color(0.878f, 0.949f, 0.996f, 1f);  // #E0F2FE
                textColor = new Color(0.008f, 0.518f, 0.780f, 1f); // #0284C7
                break;
            case CampaignDifficulty.Hard:
                bgColor = new Color(0.953f, 0.910f, 1f, 1f);      // #F3E8FF
                textColor = new Color(0.486f, 0.227f, 0.929f, 1f); // #7C3AED
                break;
            default:
                bgColor = new Color(0.902f, 0.941f, 0.988f, 1f);
                textColor = new Color(0.145f, 0.388f, 0.922f, 1f);
                break;
        }

        if (pillBackground != null) pillBackground.color = bgColor;
        if (difficultyText != null) difficultyText.color = textColor;
    }
}

