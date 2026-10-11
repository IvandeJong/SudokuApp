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

    public void OpenDailySudoku()
    {
        PlayerPrefs.SetString(GameModeKey, "Daily");
        PlayerPrefs.SetString(DailyDateKey, DateTime.Now.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();
        SceneManager.LoadScene(SudokuSceneName);
    }

    public void OpenCampaign()
    {
        if (campaignPanel != null)
        {
            campaignPanel.Show();
        }
        else
        {
            PlayerPrefs.SetString(GameModeKey, "Campaign");
            PlayerPrefs.Save();
            SceneManager.LoadScene(SudokuSceneName);
        }
    }

    public void OpenFreePlay()
    {
        if (freePlayPanel != null)
        {
            freePlayPanel.Show();
        }
        else
        {
            Debug.Log("Free play mode clicked (no panel linked)");
        }
    }
}


