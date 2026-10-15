using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MainMenuController : MonoBehaviour
{
    public const string SudokuSceneName = "SudokuClassic";
    public const string GameModeKey = "SudokuGameMode";
    public const string DailyDateKey = "SudokuDailyDate";
    public const string GridSizeKey = "CurrentGridSize";
    public const string DifficultyKey = "CurrentDifficulty";

    [SerializeField] private FreePlayPanelController freePlayPanel;
    [SerializeField] private CampaignPanelController campaignPanel;

    private void Start()
    {
        if (campaignPanel != null && PlayerPrefs.GetString(GameModeKey, "") == "Campaign")
        {
            campaignPanel.Show();
        }
    }

    public void OpenDailySudoku()
    {
        PlayerPrefs.SetString(GameModeKey, "Daily");
        PlayerPrefs.SetString(DailyDateKey, DateTime.Now.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();
        SceneManager.LoadScene(SudokuSceneName);
    }

    public void OpenCampaign()
    {
        campaignPanel.Show();
    }

    public void OpenFreePlay()
    {
        freePlayPanel.Show();
    }
}


