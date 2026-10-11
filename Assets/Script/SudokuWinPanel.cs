using System;
using TMPro;
using UnityEngine;

public class SudokuWinPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private TextMeshProUGUI mistakesText;

    public void Show(float totalSeconds, int mistakes)
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        if (titleText != null)
        {
            titleText.text = "Sudoku opgelost!";
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
}
