using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampaignLevelItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private GameObject[] starIcons;
    [SerializeField] private Button playButton;

    private int levelIndex;
    private System.Action<int> onPlayClicked;

    public void Setup(int index, string size, string difficulty, CampaignManager.LevelProgress progress, System.Action<int> playAction)
    {
        levelIndex = index;
        onPlayClicked = playAction;

        levelNameText.text = $"{size} - Level {index + 1} ({difficulty})";
        
        if (progress != null)
        {
            System.TimeSpan t = System.TimeSpan.FromSeconds(progress.bestTime);
            statsText.text = $"Best: {t:mm\\:ss}";
            for (int i = 0; i < starIcons.Length; i++)
            {
                starIcons[i].SetActive(i < progress.stars);
            }
        }
        else
        {
            statsText.text = "Not played";
            foreach (var star in starIcons) star.SetActive(false);
        }

        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() => onPlayClicked?.Invoke(levelIndex));
    }
}
