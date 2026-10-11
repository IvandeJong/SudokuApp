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
    private GameObject notesOffOverlay;
    private Image hintIcon;
    private Sprite noHintsIcon;
    private Sprite availableHintsIcon;
    private readonly Color activeButtonColor = Color.white;
    private readonly Color inactiveButtonColor = new Color(0.65f, 0.65f, 0.65f, 1f);

    private void Awake()
    {
        ResolveReferences();
    }

    private void ResolveReferences()
    {
        hintButton = transform.Find("HintButton")?.GetComponent<Button>();
        notesButton = transform.Find("NotesButton")?.GetComponent<Button>();
        undoButton = transform.Find("UndoButton")?.GetComponent<Button>();
        eraseButton = transform.Find("EraseButton")?.GetComponent<Button>();
        undoIcon = transform.Find("UndoButton/Icon")?.GetComponent<Image>();
        eraseIcon = transform.Find("EraseButton/Icon")?.GetComponent<Image>();

        if (hintBadgeText == null)
        {
            Transform badge = transform.Find("HintButton/Icon/Badge/Text");
            if (badge != null) hintBadgeText = badge.GetComponent<TextMeshProUGUI>();
        }

        if (notesOffOverlay == null)
        {
            Transform overlay = transform.Find("NotesButton/Icon/OFF_Overlay");
            if (overlay != null) notesOffOverlay = overlay.gameObject;
        }

        if (hintIcon == null)
        {
            Transform icon = transform.Find("HintButton/Icon");
            if (icon != null) hintIcon = icon.GetComponent<Image>();
        }

        if (hintIcon != null)
        {
            availableHintsIcon = hintIcon.sprite;
        }

        noHintsIcon = Resources.Load<Sprite>("shape029_style03_color10");
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
        if (notesOffOverlay != null)
        {
            notesOffOverlay.SetActive(!isNotesMode);
        }
    }

    public void UpdateHintBadge(int remainingHints)
    {
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

        if (hintIcon != null)
        {
            hintIcon.sprite = remainingHints > 0 ? availableHintsIcon : noHintsIcon;
        }
    }
}
