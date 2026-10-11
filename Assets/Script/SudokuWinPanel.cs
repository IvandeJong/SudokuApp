using System;
using TMPro;
using UnityEngine;

public class SudokuWinPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private TextMeshProUGUI mistakesText;

    [SerializeField] private GameObject starsContainer;
    [SerializeField] private GameObject[] starIcons; // 3 stars

    public void Show(float totalSeconds, int mistakes)
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        string mode = PlayerPrefs.GetString("SudokuGameMode", "");
        bool isCampaign = mode == "Campaign";

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
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
