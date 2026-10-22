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

    private CampaignLevelItem sceneLevelItem;

    private void Awake()
    {
        if (listContainer != null && listContainer.childCount > 0)
        {
            sceneLevelItem = listContainer.GetChild(0).GetComponent<CampaignLevelItem>();
        }
    }

    private void OnEnable()
    {
        RefreshList();
    }

    private void Start()
    {
        closeButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        if (panelRoot.activeInHierarchy)
        {
            RefreshList();
        }
        else
        {
            panelRoot.SetActive(true);
        }
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }

    private void RefreshList()
    {
        for (int i = listContainer.childCount - 1; i >= 0; i--)
        {
            GameObject child = listContainer.GetChild(i).gameObject;
            if (sceneLevelItem != null && child == sceneLevelItem.gameObject) continue;
            child.SetActive(false);
            Destroy(child);
        }

        var levels = CampaignManager.Instance.GetAllLevels();
        bool firstItem = true;
        foreach (var level in levels)
        {
            GameObject itemObj = firstItem && sceneLevelItem != null
                ? sceneLevelItem.gameObject
                : Instantiate(sceneLevelItem != null ? sceneLevelItem.gameObject : levelItemPrefab, listContainer);
            itemObj.SetActive(true);
            var item = itemObj.GetComponent<CampaignLevelItem>();
            var progress = CampaignManager.Instance.GetProgress(level.index);
            bool isUnlocked = CampaignManager.Instance.IsLevelUnlocked(level.index);
            item.Setup(level.index, level.levelType, level.difficulty, progress, isUnlocked, StartCampaignLevel);
            firstItem = false;
        }

        if (firstItem && sceneLevelItem != null)
        {
            sceneLevelItem.gameObject.SetActive(false);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(listContainer);

        ScrollRect scrollRect = listContainer.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void StartCampaignLevel(int index)
    {
        if (!CampaignManager.Instance.IsLevelUnlocked(index)) return;

        var level = CampaignManager.Instance.GetLevel(index);
        if (level == null) return;

        PlayerPrefs.SetString(PlayerPrefsKey.SudokuGameMode.ToString(), GameModes.Campaign);
        PlayerPrefs.SetInt(PlayerPrefsKey.CampaignLevelIndex.ToString(), index);
        PlayerPrefs.Save();

        string sceneName = SceneNames.SudokuClassic;
        if (level.levelType == CampaignLevelType.Nano) sceneName = SceneNames.SudokuNano;
        else if (level.levelType == CampaignLevelType.Mini) sceneName = SceneNames.SudokuMini;

        SceneManager.LoadScene(sceneName);
    }
}
