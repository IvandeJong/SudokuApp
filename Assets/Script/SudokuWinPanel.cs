using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SudokuWinPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private TextMeshProUGUI mistakesText;
    [SerializeField] private GameObject nextLevelButton;

    [SerializeField] private GameObject starsContainer;
    [SerializeField] private GameObject[] starIcons; // 3 stars

    public void Show(float totalSeconds, int mistakes, bool showNextLevel)
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        string mode = PlayerPrefs.GetString("SudokuGameMode", "");
        bool isCampaign = mode == "Campaign";

        if (nextLevelButton != null)
        {
            nextLevelButton.SetActive(showNextLevel);
        }

        if (titleText != null)
        {
            titleText.text = isCampaign ? "Level Voltooid!" : "Sudoku opgelost!";
        }

        TimeSpan time = TimeSpan.FromSeconds(totalSeconds);
        if (totalTimeText != null)
        {
            totalTimeText.text = $"Totale tijd: {time:mm\\:ss}";
        }

        if (mistakesText != null)
        {
            mistakesText.text = $"Aantal fouten: {mistakes}";
        }

        if (isCampaign)
        {
            int levelIndex = PlayerPrefs.GetInt("CampaignLevelIndex", 0);
            CampaignManager.Instance.SaveLevelResult(levelIndex, totalSeconds, mistakes);

            var progress = CampaignManager.Instance.GetProgress(levelIndex);
            if (progress != null && starsContainer != null)
            {
                starsContainer.SetActive(true);
                for (int i = 0; i < starIcons.Length; i++)
                {
                    starIcons[i].SetActive(i < progress.stars);
                }
            }
        }
        else
        {
            if (starsContainer != null) starsContainer.SetActive(false);
        }

        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        panelRoot.SetActive(false);
    }

    public void OpenMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void StartNextCampaignLevel()
    {
        int nextIndex = PlayerPrefs.GetInt("CampaignLevelIndex", 0) + 1;
        CampaignManager.CampaignLevel nextLevel = CampaignManager.Instance.GetLevel(nextIndex);
        if (nextLevel == null)
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        PlayerPrefs.SetInt("CampaignLevelIndex", nextIndex);
        PlayerPrefs.SetString(MainMenuController.DifficultyKey, nextLevel.difficulty);
        PlayerPrefs.Save();

        string sceneName = "SudokuClassic";
        if (nextLevel.gridSize == "Nano") sceneName = "SudokuNano";
        else if (nextLevel.gridSize == "Mini") sceneName = "SudokuMini";

        SceneManager.LoadScene(sceneName);
    }
}
