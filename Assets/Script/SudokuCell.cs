using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

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
    [SerializeField] private GameObject notesContainer;
    [SerializeField] private TextMeshProUGUI[] noteTexts; // 9 texts for classic 9x9
    private const int MaxNotes = 4;
    private List<int> currentNotes = new List<int>();

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
            cellText.color = initial ? Color.black : Color.blue;
            cellText.gameObject.SetActive(value != 0);
        }

        if (backgroundImage != null && feedbackCoroutine == null)
        {
            backgroundImage.color = GetBaseColor();
        }

        if (notesContainer != null)
        {
            notesContainer.SetActive(value == 0 && currentNotes.Count > 0);
        }
    }

    public void AddNote(int number)
    {
        if (!currentNotes.Contains(number) && currentNotes.Count < MaxNotes)
        {
            currentNotes.Add(number);
            UpdateNotesDisplay();
        }
    }

    public void RemoveNote(int number)
    {
        if (currentNotes.Contains(number))
        {
            currentNotes.Remove(number);
            UpdateNotesDisplay();
        }
    }

    public bool ToggleNote(int number)
    {
        if (currentNotes.Contains(number))
        {
            currentNotes.Remove(number);
            UpdateNotesDisplay();
            return false;
        }

        if (currentNotes.Count >= MaxNotes)
        {
            return false;
        }

        currentNotes.Add(number);
        UpdateNotesDisplay();
        return true;
    }

    public void ClearNotes()
    {
        currentNotes.Clear();
        UpdateNotesDisplay();
    }

    public List<int> GetNotes() => new List<int>(currentNotes);

    public void SetNotes(List<int> notes)
    {
        currentNotes = new List<int>(notes);
        if (currentNotes.Count > MaxNotes)
        {
            currentNotes.RemoveRange(MaxNotes, currentNotes.Count - MaxNotes);
        }
        UpdateNotesDisplay();
    }

    private void UpdateNotesDisplay()
    {
        if (notesContainer == null) return;
        
        notesContainer.SetActive(value == 0 && currentNotes.Count > 0);
        
        for (int i = 0; i < noteTexts.Length; i++)
        {
            if (noteTexts[i] != null)
            {
                noteTexts[i].text = i < currentNotes.Count && i < MaxNotes
                    ? currentNotes[i].ToString()
                    : "";
            }
        }
    }

    [SerializeField] private Image backgroundImage; // Sleep hier de Image component van de knop in
    [SerializeField] private Color initialCellColor = new Color(0.9f, 0.9f, 0.9f, 1f);
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
        if (isSelected)
        {
            return Color.yellow;
        }

        return isInitial ? initialCellColor : Color.white;
    }
}