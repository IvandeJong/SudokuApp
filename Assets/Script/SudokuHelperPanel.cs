using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuHelperPanel : MonoBehaviour
{
    private const float HintCooldownDuration = 30f;

    private Button hintButton;
    private Button notesButton;
    private Button undoButton;
    private Button eraseButton;
    private Image undoIcon;
    private Image eraseIcon;
    private TextMeshProUGUI hintCountText;
    private TextMeshProUGUI hintTimerText;
    private TextMeshProUGUI penLabel;
    private TextMeshProUGUI notesLabel;
    private Image penBackground;
    private Image notesBackground;
    private Image hintIcon;
    private Image hintTimerLight;
    private Image hintTimerDark;
    private SudokuGameManager gameManager;
    private int remainingHints;
    private float hintCooldownRemaining;
    public bool CanRequestHint => remainingHints > 0 && hintCooldownRemaining <= 0f;
    private readonly Color activeButtonColor = Color.white;
    private readonly Color inactiveButtonColor = new Color(0.65f, 0.65f, 0.65f, 1f);
    private readonly Color selectedModeColor = new Color(0.2470588f, 0.5019608f, 0.7607843f, 1f);
    private readonly Color unselectedModeColor = Color.white;
    private readonly Color unselectedModeLabelColor = new Color(0.0980392f, 0.2039216f, 0.345098f, 1f);

    private void Awake()
    {
        ResolveReferences();
        SetNotesMode(false);
        hintCooldownRemaining = HintCooldownDuration;
        UpdateHintTimerVisuals();
        UpdateHintButtonState();
    }

    private void Update()
    {
        if (hintCooldownRemaining <= 0f) return;

        hintCooldownRemaining = Mathf.Max(0f, hintCooldownRemaining - Time.deltaTime);
        UpdateHintTimerVisuals();
        UpdateHintButtonState();
    }

    private void ResolveReferences()
    {
        hintButton = transform.Find("HintButton")?.GetComponent<Button>();
        if (hintButton != null)
        {
            // This script owns the hint button's enabled/disabled colors. Disable
            // Button's tint transition so its disabled CanvasRenderer tint cannot
            // persist after the cooldown has ended.
            hintButton.transition = Selectable.Transition.None;
        }

        notesButton = transform.Find("NotesButton")?.GetComponent<Button>();
        undoButton = transform.Find("UndoButton")?.GetComponent<Button>();
        eraseButton = transform.Find("EraseButton")?.GetComponent<Button>();
        undoIcon = transform.Find("UndoButton/Icon")?.GetComponent<Image>();
        eraseIcon = transform.Find("EraseButton/Icon")?.GetComponent<Image>();
        penBackground = transform.Find("NotesButton/PEN")?.GetComponent<Image>();
        notesBackground = transform.Find("NotesButton/NOTES")?.GetComponent<Image>();
        penLabel = transform.Find("NotesButton/PEN/Label")?.GetComponent<TextMeshProUGUI>();
        notesLabel = transform.Find("NotesButton/NOTES/Label")?.GetComponent<TextMeshProUGUI>();
        hintCountText = transform.Find("HintButton/HINT/Count")?.GetComponent<TextMeshProUGUI>();
    

        if (hintIcon == null)
        {
            Transform icon = transform.Find("HintButton/Icon");
            if (icon != null) hintIcon = icon.GetComponent<Image>();
        }

        if (hintTimerLight == null)
        {
            Transform timerLight = transform.Find("HintButton/TIME/TimerLight");
            if (timerLight != null) hintTimerLight = timerLight.GetComponent<Image>();
        }
        
        if (hintTimerDark == null)
        {
            Transform timerDark = transform.Find("HintButton/TIME/TimerDark");
            if (timerDark != null) hintTimerDark = timerDark.GetComponent<Image>();
        }
        
        if (hintTimerText == null)
        {
            Transform timer = transform.Find("HintButton/TIME/Timer");
            if (timer != null) hintTimerText = timer.GetComponent<TextMeshProUGUI>();
        }
    }

    public void Bind(SudokuGameManager gameManager)
{
    ResolveReferences();
    this.gameManager = gameManager;

    if (hintButton != null)
    {
        hintButton.onClick.RemoveAllListeners();
        hintButton.onClick.AddListener(HandleHintRequest);
        UpdateHintButtonState();
    }

    if (notesButton != null)
    {
        notesButton.onClick.RemoveAllListeners();
        notesButton.onClick.AddListener(gameManager.ToggleNotesMode);
        notesButton.interactable = true;
    }

    if (undoButton != null)
    {
        undoButton.onClick.RemoveAllListeners();
        undoButton.onClick.AddListener(gameManager.Undo);
    }

    if (eraseButton != null)
    {
        eraseButton.onClick.RemoveAllListeners();
        eraseButton.onClick.AddListener(gameManager.Erase);
    }
}

public void UpdateActionButtonStates(bool canUndo, bool canErase)
{
    SetButtonState(undoButton, undoIcon, canUndo);
    SetButtonState(eraseButton, eraseIcon, canErase);
}

private void SetButtonState(Button button, Image icon, bool isActive)
{
    if (button == null) return;

    button.interactable = isActive;
    Color color = isActive ? activeButtonColor : inactiveButtonColor;

    if (button.targetGraphic != null) button.targetGraphic.color = color;
    if (icon != null) icon.color = color;
}

public void SetNotesMode(bool isNotesMode)
{
    if (penBackground != null) penBackground.color = isNotesMode ? unselectedModeColor : selectedModeColor;
    if (notesBackground != null) notesBackground.color = isNotesMode ? selectedModeColor : unselectedModeColor;
    if (penLabel != null) penLabel.color = isNotesMode ? unselectedModeLabelColor : Color.white;
    if (notesLabel != null) notesLabel.color = isNotesMode ? Color.white : unselectedModeLabelColor;
}

public void UpdateHintBadge(int remainingHints)
{
    this.remainingHints = remainingHints;
    UpdateHintButtonState();

    if (hintCountText != null)
    {
        hintCountText.text = remainingHints.ToString();
        hintCountText.gameObject.SetActive(true);
    }
}

public void ResetHintCooldown()
{
        hintCooldownRemaining = HintCooldownDuration;
    UpdateHintTimerVisuals();
    UpdateHintButtonState();
}

public void StartHintCooldown()
{
    hintCooldownRemaining = HintCooldownDuration;
    UpdateHintTimerVisuals();
    UpdateHintButtonState();
}

private void HandleHintRequest()
{
    if (gameManager == null || !CanRequestHint) return;
    gameManager.Hint();
}

private void UpdateHintTimerVisuals()
{
    float progress = hintCooldownRemaining / HintCooldownDuration;
    if (hintTimerLight != null) hintTimerLight.fillAmount = progress;
    if (hintTimerDark != null) hintTimerDark.fillAmount = 1f;
    if (hintTimerText != null) hintTimerText.text = $"00:{Mathf.CeilToInt(hintCooldownRemaining):00}";
}

private void UpdateHintButtonState()
{
        if (hintButton == null) return;

        bool canRequestHint = CanRequestHint;
        hintButton.interactable = canRequestHint;

        Color buttonColor = canRequestHint ? hintButton.colors.normalColor : inactiveButtonColor;
        if (hintButton.targetGraphic != null) hintButton.targetGraphic.color = buttonColor;
        if (hintIcon != null) hintIcon.color = canRequestHint ? activeButtonColor : inactiveButtonColor;
}
}
