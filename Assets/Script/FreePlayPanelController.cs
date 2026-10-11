using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FreePlayPanelController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private GameObject bestTimesPanel;

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

    private const string FreePlayModeValue = "FreePlay";

    private string selectedGridSize = "Classic";
    private string selectedDifficulty = "Medium";

    private Color selectedColor = new Color(0.75f, 0.82f, 0.90f, 1f);
    private Color normalColor = new Color(0.92f, 0.95f, 0.98f, 1f);

    void Start()
    {
        nanoButton.onClick.AddListener(() => SetGridSize("Nano"));
        miniButton.onClick.AddListener(() => SetGridSize("Mini"));
        classicButton.onClick.AddListener(() => SetGridSize("Classic"));

        easyButton.onClick.AddListener(() => SetDifficulty("Easy"));
        mediumButton.onClick.AddListener(() => SetDifficulty("Medium"));
        hardButton.onClick.AddListener(() => SetDifficulty("Hard"));

        startGameButton.onClick.AddListener(StartGame);
        closeButton.onClick.AddListener(Hide);

        // Set defaults
        SetGridSize("Classic");
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

    private void SetGridSize(string size)
    {
        selectedGridSize = size;
        UpdateButtonVisuals(nanoButton, selectedGridSize == "Nano");
        UpdateButtonVisuals(miniButton, selectedGridSize == "Mini");
        UpdateButtonVisuals(classicButton, selectedGridSize == "Classic");
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

    private float GetBestTime(string gridSize, string difficulty)
    {
        string key = $"BestTime_{gridSize}_{difficulty}";
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
        PlayerPrefs.SetString(MainMenuController.GameModeKey, FreePlayModeValue);
        PlayerPrefs.SetString(MainMenuController.GridSizeKey, selectedGridSize);
        PlayerPrefs.SetString(MainMenuController.DifficultyKey, selectedDifficulty);
        PlayerPrefs.Save();
        
        string sceneName = "SudokuClassic";
        if (selectedGridSize == "Nano") sceneName = "SudokuNano";
        else if (selectedGridSize == "Mini") sceneName = "SudokuMini";

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
