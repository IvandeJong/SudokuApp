using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public static class SceneNames
{
    public const string MainMenu = "MainMenu";
    public const string SudokuNano = "SudokuNano";
    public const string SudokuMini = "SudokuMini";
    public const string SudokuClassic = "SudokuClassic";
}

public static class GameModes
{
    public const string FreePlay = "FreePlay";
    public const string Campaign = "Campaign";
    public const string Daily = "Daily";
}

public enum PlayerPrefsKey
{
    SudokuGameMode,
    SudokuDailyDate,
    CurrentGridSize,
    CurrentDifficulty,
    CampaignLevelIndex,
    CampaignProgress_,
    BestTime_,
    DailySudokuState
}

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private FreePlayPanelController freePlayPanel;
    [SerializeField] private CampaignPanelController campaignPanel;

    public void OpenDailySudoku()
    {
        PlayerPrefs.SetString(PlayerPrefsKey.SudokuGameMode.ToString(), GameModes.Daily);
        PlayerPrefs.SetString(PlayerPrefsKey.SudokuDailyDate.ToString(), DateTime.Now.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneNames.SudokuClassic);
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


