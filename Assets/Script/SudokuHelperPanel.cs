using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuHelperPanel : MonoBehaviour
{
    private Button hintButton;
    private Button notesButton;
    private Button undoButton;
    private Button eraseButton;
    private Image undoIcon;
    private Image eraseIcon;
    private TextMeshProUGUI hintBadgeText;
    private TextMeshProUGUI penLabel;
    private TextMeshProUGUI notesLabel;
    private Image penBackground;
    private Image notesBackground;
    private Image hintIcon;
    private readonly Color activeButtonColor = Color.white;
    private readonly Color inactiveButtonColor = new Color(0.65f, 0.65f, 0.65f, 1f);
    private readonly Color selectedModeColor = new Color(0.2470588f, 0.5019608f, 0.7607843f, 1f);
    private readonly Color unselectedModeColor = Color.white;
    private readonly Color unselectedModeLabelColor = new Color(0.0980392f, 0.2039216f, 0.345098f, 1f);

    private void Awake()
    {
        ResolveReferences();
        SetNotesMode(false);
    }

    private void ResolveReferences()
    {
        hintButton = transform.Find("HintButton")?.GetComponent<Button>();
        notesButton = transform.Find("NotesButton")?.GetComponent<Button>();
        undoButton = transform.Find("UndoButton")?.GetComponent<Button>();
        eraseButton = transform.Find("EraseButton")?.GetComponent<Button>();
        undoIcon = transform.Find("UndoButton/Icon")?.GetComponent<Image>();
        eraseIcon = transform.Find("EraseButton/Icon")?.GetComponent<Image>();
        penBackground = transform.Find("NotesButton/PEN")?.GetComponent<Image>();
        notesBackground = transform.Find("NotesButton/NOTES")?.GetComponent<Image>();
        penLabel = transform.Find("NotesButton/PEN/Label")?.GetComponent<TextMeshProUGUI>();
        notesLabel = transform.Find("NotesButton/NOTES/Label")?.GetComponent<TextMeshProUGUI>();

        if (hintBadgeText == null)
        {
            Transform badge = transform.Find("HintButton/Icon/Badge/Text");
            if (badge != null) hintBadgeText = badge.GetComponent<TextMeshProUGUI>();
        }

        if (hintIcon == null)
        {
            Transform icon = transform.Find("HintButton/Icon");
            if (icon != null) hintIcon = icon.GetComponent<Image>();
        }
    }

    public void Bind(SudokuGameManager gameManager)
    {
        ResolveReferences();

        if (hintButton != null)
        {
            hintButton.onClick.RemoveAllListeners();
            hintButton.onClick.AddListener(gameManager.Hint);
            hintButton.interactable = true;
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
        SetButtonState(hintButton, hintIcon, remainingHints > 0);

        if (hintBadgeText != null)
        {
            hintBadgeText.text = remainingHints.ToString();
            hintBadgeText.gameObject.SetActive(remainingHints > 0);

            Transform badgeBackground = hintBadgeText.transform.parent;
            if (badgeBackground != null && badgeBackground.name == "Badge")
            {
                badgeBackground.gameObject.SetActive(remainingHints > 0);
            }
        }
    }
}
