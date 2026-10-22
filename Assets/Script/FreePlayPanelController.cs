using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FreePlayPanelController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject panelRoot;

    [Header("Grid Size Buttons")]
    [SerializeField] private Button nanoButton;
    [SerializeField] private Button miniButton;
    [SerializeField] private Button classicButton;

    [Header("Difficulty Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;

    [Header("Best Times Text")]
    [SerializeField] private TextMeshProUGUI easyTimeText;
    [SerializeField] private TextMeshProUGUI mediumTimeText;
    [SerializeField] private TextMeshProUGUI hardTimeText;

    [Header("Action Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button closeButton;

    private CampaignLevelType selectedGridSize = CampaignLevelType.Classic;
    private string selectedDifficulty = "Medium";

    private Color selectedColor = new Color(0.75f, 0.82f, 0.90f, 1f);
    private Color normalColor = new Color(0.92f, 0.95f, 0.98f, 1f);

    private void Start()
    {
        nanoButton.onClick.AddListener(() => SetGridSize(CampaignLevelType.Nano));
        miniButton.onClick.AddListener(() => SetGridSize(CampaignLevelType.Mini));
        classicButton.onClick.AddListener(() => SetGridSize(CampaignLevelType.Classic));

        easyButton.onClick.AddListener(() => SetDifficulty("Easy"));
        mediumButton.onClick.AddListener(() => SetDifficulty("Medium"));
        hardButton.onClick.AddListener(() => SetDifficulty("Hard"));

        startGameButton.onClick.AddListener(StartGame);
        closeButton.onClick.AddListener(Hide);

        // Set defaults
        SetGridSize(CampaignLevelType.Classic);
        SetDifficulty("Medium");
    }

    public void Show()
    {
        panelRoot.SetActive(true);
        UpdateBestTimes();
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }

    private void SetGridSize(CampaignLevelType size)
    {
        selectedGridSize = size;
        UpdateButtonVisuals(nanoButton, selectedGridSize == CampaignLevelType.Nano);
        UpdateButtonVisuals(miniButton, selectedGridSize == CampaignLevelType.Mini);
        UpdateButtonVisuals(classicButton, selectedGridSize == CampaignLevelType.Classic);
        UpdateBestTimes();
    }

    private void SetDifficulty(string difficulty)
    {
        selectedDifficulty = difficulty;
        UpdateButtonVisuals(easyButton, selectedDifficulty == "Easy");
        UpdateButtonVisuals(mediumButton, selectedDifficulty == "Medium");
        UpdateButtonVisuals(hardButton, selectedDifficulty == "Hard");
    }

    private void UpdateButtonVisuals(Button button, bool isSelected)
    {
        var colors = button.colors;
        colors.normalColor = isSelected ? selectedColor : normalColor;
        colors.selectedColor = isSelected ? selectedColor : normalColor;
        button.colors = colors;
    }

    private void UpdateBestTimes()
    {
        easyTimeText.text = $"Easy\n{FormatTime(GetBestTime(selectedGridSize, "Easy"))}";
        mediumTimeText.text = $"Medium\n{FormatTime(GetBestTime(selectedGridSize, "Medium"))}";
        hardTimeText.text = $"Hard\n{FormatTime(GetBestTime(selectedGridSize, "Hard"))}";
    }

    private float GetBestTime(CampaignLevelType gridSize, string difficulty)
    {
        string key = $"{PlayerPrefsKey.BestTime_}{gridSize}_{difficulty}";
        return PlayerPrefs.GetFloat(key, 0f);
    }

    private string FormatTime(float time)
    {
        if (time <= 0) return "--/--";
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void StartGame()
    {
        Debug.Log($"Starting Free Play: {selectedGridSize} - {selectedDifficulty}");
        PlayerPrefs.SetString(PlayerPrefsKey.SudokuGameMode.ToString(), GameModes.FreePlay);
        PlayerPrefs.SetString(PlayerPrefsKey.CurrentGridSize.ToString(), selectedGridSize.ToString());
        PlayerPrefs.SetString(PlayerPrefsKey.CurrentDifficulty.ToString(), selectedDifficulty);
        PlayerPrefs.Save();

        string sceneName = selectedGridSize switch
        {
            CampaignLevelType.Nano => SceneNames.SudokuNano,
            CampaignLevelType.Mini => SceneNames.SudokuMini,
            _ => SceneNames.SudokuClassic
        };

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
