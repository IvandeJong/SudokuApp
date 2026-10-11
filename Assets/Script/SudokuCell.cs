using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SudokuCell : MonoBehaviour
{
    public int row;
    public int col;
    public int value = 0; // 0 betekent leeg
    public bool isInitial; // Bepaalt of dit een vast startgetal is

    private Button cellButton;
    private TextMeshProUGUI cellText;
    private SudokuGameManager gameManager;
    private bool isSelected;
    private Coroutine feedbackCoroutine;

    void Awake()
    {
        cellButton = GetComponent<Button>();
        cellText = GetComponentInChildren<TextMeshProUGUI>();
        gameManager = Object.FindFirstObjectByType<SudokuGameManager>();
        // Zoek automatisch de Image component op ditzelfde knop-object
        backgroundImage = GetComponent<Image>();

        // Luister naar een klik op deze cel
        cellButton.onClick.AddListener(OnCellClicked);
    }

    void OnCellClicked()
    {
        // Vertel de GameManager dat deze cel is geselecteerd
        gameManager.SelectCell(this);
    }

    [SerializeField] private TextMeshProUGUI numberText; // Of Text als het geen TMP is
    public void SetValue(int newValue, bool initial = false)
    {
        value = newValue;
        isInitial = initial;

        if (cellText == null)
        {
            cellText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (cellText != null)
        {
            cellText.text = value == 0 ? "" : value.ToString();
            // cellText vaste startgetallen optioneel een andere kleur, bijv. vetgedrukt of grijs
            cellText.color = initial ? Color.black : Color.blue;
        }
    }

    [SerializeField] private Image backgroundImage; // Sleep hier de Image component van de knop in
    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;

        if (backgroundImage != null)
        {
            if (feedbackCoroutine == null)
            {
                backgroundImage.color = GetBaseColor();
            }
        }
    }

    public void FlashWrongAnswer(float duration)
    {
        if (backgroundImage == null)
        {
            return;
        }

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine = StartCoroutine(FlashFeedbackRoutine(Color.red, duration));
    }

    public void FlashCorrectAnswer(float duration)
    {
        if (backgroundImage == null)
        {
            return;
        }

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine = StartCoroutine(FlashFeedbackRoutine(Color.green, duration));
    }

    private IEnumerator FlashFeedbackRoutine(Color flashColor, float duration)
    {
        backgroundImage.color = flashColor;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            backgroundImage.color = Color.Lerp(flashColor, GetBaseColor(), t);
            yield return null;
        }

        backgroundImage.color = GetBaseColor();
        feedbackCoroutine = null;
    }

    private Color GetBaseColor()
    {
        return isSelected ? Color.yellow : Color.white;
    }
}