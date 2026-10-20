using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SudokuCell : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public int row;
    public int col;
    public int value = 0; // 0 betekent leeg
    public bool isInitial; // Bepaalt of dit een vast startgetal is

    private Button cellButton;
    private TextMeshProUGUI cellText;
    private SudokuGameManager gameManager;
    private bool isSelected;
    private bool isRelatedHighlight;
    private Coroutine feedbackCoroutine;
    [SerializeField] private float incorrectEntryFadeDelay = 30f;
    [SerializeField] private float incorrectEntryFadeDuration = 5f;
    private bool isIncorrectEntry;
    private float incorrectEntryElapsedTime;

    void Awake()
    {
        cellButton = GetComponent<Button>();
        cellText = GetComponentInChildren<TextMeshProUGUI>();
        gameManager = Object.FindFirstObjectByType<SudokuGameManager>();
        // Zoek automatisch de Image component op ditzelfde knop-object
        backgroundImage = GetComponent<Image>();

        // SudokuCell owns the background color. A Button color-tint transition also
        // writes a tint to this Image's CanvasRenderer after a click, which can make
        // the selected color differ from the value returned by GetBaseColor().
        if (cellButton != null)
        {
            cellButton.transition = Selectable.Transition.None;
        }

        if (backgroundImage != null)
        {
            backgroundImage.canvasRenderer.SetColor(Color.white);
            backgroundImage.color = GetBaseColor();
        }

        // Luister naar een klik op deze cel
        if (cellButton != null)
        {
            cellButton.onClick.AddListener(OnCellClicked);
        }
    }

    private void Update()
    {
        if (!isIncorrectEntry)
        {
            return;
        }

        incorrectEntryElapsedTime += Time.deltaTime;
        if (backgroundImage != null && feedbackCoroutine == null)
        {
            backgroundImage.color = GetBaseColor();
        }
    }

    void OnCellClicked()
    {
        SelectFromPointer();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Select while hovering with a mouse/pen or dragging a touch across the grid.
        SelectFromPointer();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Touch devices have no hover state, so select immediately on contact.
        SelectFromPointer();
    }

    private void SelectFromPointer()
    {
        if (gameManager == null)
        {
            gameManager = Object.FindFirstObjectByType<SudokuGameManager>();
        }

        gameManager?.SelectCell(this);
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
        isIncorrectEntry = false;
        incorrectEntryElapsedTime = 0f;

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

    public void SetIncorrectEntry(bool isIncorrect, float elapsedTime = 0f)
    {
        isIncorrectEntry = isIncorrect && value != 0 && !isInitial;
        incorrectEntryElapsedTime = isIncorrectEntry ? Mathf.Max(0f, elapsedTime) : 0f;

        if (backgroundImage != null && feedbackCoroutine == null)
        {
            backgroundImage.color = GetBaseColor();
        }
    }

    private float GetIncorrectEntryFadeProgress()
    {
        return isIncorrectEntry
            ? Mathf.Clamp01((incorrectEntryElapsedTime - incorrectEntryFadeDelay) / Mathf.Max(incorrectEntryFadeDuration, 0.01f))
            : 0f;
    }

    public float GetIncorrectEntryElapsedTime()
    {
        return isIncorrectEntry ? incorrectEntryElapsedTime : 0f;
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
    [SerializeField] private Color selectedCellColor = new Color(0.35f, 0.65f, 1f, 1f);
    [SerializeField] private Color relatedCellColor = new Color(0.88f, 0.94f, 1f, 1f);
    [SerializeField] private Color relatedInitialCellColor = new Color(0.55f, 0.72f, 0.95f, 1f);

    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;

        RefreshBackgroundColor();
    }

    public void SetRelatedHighlight(bool isHighlighted)
    {
        isRelatedHighlight = isHighlighted;

        RefreshBackgroundColor();
    }

    private void RefreshBackgroundColor()
    {
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
        float incorrectFadeProgress = GetIncorrectEntryFadeProgress();
        if (isIncorrectEntry && incorrectFadeProgress > 0f)
        {
            return Color.Lerp(isInitial ? initialCellColor : Color.white, Color.red, incorrectFadeProgress);
        }

        Color baseColor;
        if (isSelected)
        {
            baseColor = selectedCellColor;
        }
        else if (isRelatedHighlight)
        {
            baseColor = isInitial ? relatedInitialCellColor : relatedCellColor;
        }
        else
        {
            baseColor = isInitial ? initialCellColor : Color.white;
        }

        return baseColor;
    }
}