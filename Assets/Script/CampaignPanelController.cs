using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CampaignPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform listContainer;
    [SerializeField] private GameObject levelItemPrefab;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        closeButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        panelRoot.SetActive(true);
        RefreshList();
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }

    private void RefreshList()
    {
        // Clear existing
        foreach (Transform child in listContainer)
        {
            Destroy(child.gameObject);
        }

        var levels = CampaignManager.Instance.GetAllLevels();
        foreach (var level in levels)
        {
            GameObject itemObj = Instantiate(levelItemPrefab, listContainer);
            itemObj.SetActive(true);
            var item = itemObj.GetComponent<CampaignLevelItem>();
            var progress = CampaignManager.Instance.GetProgress(level.index);
            bool isUnlocked = CampaignManager.Instance.IsLevelUnlocked(level.index);
            item.Setup(level.index, level.gridSize, level.difficulty, progress, isUnlocked, StartCampaignLevel);
        }
    }

    private void StartCampaignLevel(int index)
    {
        if (!CampaignManager.Instance.IsLevelUnlocked(index)) return;

        var level = CampaignManager.Instance.GetLevel(index);
        if (level == null) return;

        PlayerPrefs.SetString(MainMenuController.GameModeKey, "Campaign");
        PlayerPrefs.SetInt("CampaignLevelIndex", index);
        PlayerPrefs.Save();

        string sceneName = "SudokuClassic";
        if (level.gridSize == "Nano") sceneName = "SudokuNano";
        else if (level.gridSize == "Mini") sceneName = "SudokuMini";

        SceneManager.LoadScene(sceneName);
    }
}
