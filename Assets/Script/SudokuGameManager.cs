using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;
using UnityEngine.UI;

public class SudokuGameManager : MonoBehaviour
{
    private SudokuCell selectedCell;
    private SudokuCell[,] cellsByCoordinate;
    [SerializeField] private float wrongAnswerLockDuration = 1.5f;
    [SerializeField] private float correctAnswerFlashDuration = 1.5f;
    [SerializeField] private SudokuWinPanel winPanel;
    [SerializeField] private TMPro.TextMeshProUGUI modeTitleText;
    [SerializeField] private GameObject dailyDatePickerRoot;

    [Header("Grid Configuration")]
    [SerializeField] private int gridSize ;
    [SerializeField] private int subGridWidth;
    [SerializeField] private int subGridHeight;

    [Header("Puzzle Settings")]
    [SerializeField] private bool useMenuSettings = true;
    [SerializeField] private bool dailyModeInScene = true;
    [SerializeField] private bool useCurrentDate = true;
    [SerializeField] private string configuredDailyDate = "2025-01-01";
    [SerializeField] private string configuredDifficulty = "Medium";

    private bool isInputLocked;
    private bool isGameFinished;
    private float elapsedTime;
    private int mistakeCount;

    private int[,] board;
    private bool isDailyGame;
    private bool hasInitializedGame;
    private int currentGameSeed;
    private string currentDailyDate;
    private const string DailyStateKeyPrefix = "DailySudokuState:";

    public bool IsDailyGame => isDailyGame;
    public string CurrentDailyDate => currentDailyDate;

    [Serializable]
    private class DailyGameState
    {
        public int seed;
        public int gridSize;
        public float elapsedTime;
        public int mistakeCount;
        public int hintCount;
        public bool isNotesMode;
        public bool isGameFinished;
        public List<DailyCellState> cells = new List<DailyCellState>();
    }

    [Serializable]
    private class DailyCellState
    {
        public int row;
        public int col;
        public int value;
        public bool isInitial;
        public List<int> notes = new List<int>();
    }

    public void SelectCell(SudokuCell cell)
    {
        if (cell == null || !TryGetCellCoordinates(cell.gameObject.name, out int row, out int col) ||
            board == null || row < 0 || row >= board.GetLength(0) || col < 0 || col >= board.GetLength(1))
        {
            Debug.LogError($"Ongeldige SudokuCell-co�rdinaten: {cell?.name}. Bord: {gridSize}x{gridSize}");
            return;
        }

        cell.row = row;
        cell.col = col;

        selectedCell = cell;
        UpdateCellHighlights();

        UpdateActionButtonStates();
        Debug.Log($"Geselecteerde cel: Rij {cell.row}, Kolom {cell.col}");
    }

    private struct UndoAction
    {
        public SudokuCell cell;
        public int oldValue;
        public int newValue;
        public List<int> oldNotes;
        public List<int> newNotes;
        public bool isNoteAction;
    }

    private Stack<UndoAction> undoStack = new Stack<UndoAction>();
    private bool isNotesMode = false;
    private int hintCount = 0;
    private const int MaxHints = 5;
    private int[,] solution;

    [Header("Helper UI")]
    [SerializeField] private SudokuHelperPanel helperPanel;

    public void ToggleNotesMode()
    {
        isNotesMode = !isNotesMode;
        if (helperPanel != null) helperPanel.SetNotesMode(isNotesMode);
        Debug.Log("Notes mode: " + isNotesMode);
    }

    public void Undo()
    {
        if (undoStack.Count == 0) return;

        UndoAction action = undoStack.Pop();
        if (action.isNoteAction)
        {
            action.cell.SetNotes(action.oldNotes);
        }
        else
        {
            board[action.cell.row, action.cell.col] = action.oldValue;
            action.cell.SetNotes(action.oldNotes);
            action.cell.SetValue(action.oldValue);
        }

        UpdateActionButtonStates();
        SaveDailyGameState();
    }

    public void Erase()
    {
        if (selectedCell == null || selectedCell.isInitial) return;

        RecordUndo(selectedCell);
        board[selectedCell.row, selectedCell.col] = 0;
        selectedCell.ClearNotes();
        selectedCell.SetValue(0);
        UpdateActionButtonStates();
        SaveDailyGameState();
    }

    public void Hint()
    {
        if (isGameFinished || hintCount >= MaxHints || solution == null ||
            (helperPanel != null && !helperPanel.CanRequestHint)) return;

        SudokuCell[] emptyCells = FindObjectsByType<SudokuCell>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(cell => !cell.isInitial && cell.value == 0)
            .ToArray();
        if (emptyCells.Length == 0) return;

        SudokuCell hintCell = emptyCells[UnityEngine.Random.Range(0, emptyCells.Length)];
        int correctValue = solution[hintCell.row, hintCell.col];

        board[hintCell.row, hintCell.col] = correctValue;
        hintCell.ClearNotes();
        hintCell.SetValue(correctValue, true);
        hintCell.FlashCorrectAnswer(correctAnswerFlashDuration);
        UpdateActionButtonStates();

        hintCount++;
        UpdateHintBadge();
        if (helperPanel != null) helperPanel.StartHintCooldown();
        elapsedTime += 60f; // Add 1 minute penalty

        if (CheckWinCondition())
        {
            HandlePuzzleSolved();
        }

        SaveDailyGameState();
    }

    private void UpdateHintBadge()
    {
        if (helperPanel != null) helperPanel.UpdateHintBadge(MaxHints - hintCount);
    }

    private void RecordUndo(SudokuCell cell, bool isNoteAction = false)
    {
        undoStack.Push(new UndoAction
        {
            cell = cell,
            oldValue = board[cell.row, cell.col],
            newValue = board[cell.row, cell.col],
            oldNotes = cell.GetNotes(),
            newNotes = cell.GetNotes(),
            isNoteAction = isNoteAction
        });
        UpdateActionButtonStates();
    }

    private void UpdateActionButtonStates()
    {
        bool canErase = selectedCell != null && !selectedCell.isInitial &&
                        (selectedCell.value != 0 || selectedCell.GetNotes().Count > 0);
        if (helperPanel != null) helperPanel.UpdateActionButtonStates(undoStack.Count > 0, canErase);
    }

    public void InputNumber(int number)
    {
        if (isGameFinished) return;
        if (isInputLocked) return;
        if (selectedCell == null) return;
        if (selectedCell.isInitial) return;

        int row = selectedCell.row;
        int col = selectedCell.col;

        if (number == 0)
        {
            Erase();
            return;
        }

        if (isNotesMode)
        {
            if (IsValidMove(row, col, number))
            {
                RecordUndo(selectedCell, true);
                bool noteAdded = selectedCell.ToggleNote(number);
                if (noteAdded)
                {
                    ClearCellSelection();
                }
                UpdateActionButtonStates();
            }
            else
            {
                mistakeCount++;
                selectedCell.FlashWrongAnswer(wrongAnswerLockDuration);
            }
            SaveDailyGameState();
            return;
        }

        if (IsValidMove(row, col, number))
        {
            RecordUndo(selectedCell);
            board[row, col] = number;
            selectedCell.ClearNotes();
            selectedCell.SetValue(number);
            RemoveNoteFromRelatedCells(row, col, number);
            selectedCell.FlashCorrectAnswer(correctAnswerFlashDuration);
            ClearCellSelection();
            UpdateActionButtonStates();
        }
        else
        {
            mistakeCount++;
            selectedCell.FlashWrongAnswer(wrongAnswerLockDuration);
            StartCoroutine(LockInputTemporarily());
            SaveDailyGameState();
        }

        if (CheckWinCondition())
        {
            HandlePuzzleSolved();
        }

        SaveDailyGameState();
    }

    private void Update()
    {
        if (!isGameFinished)
        {
            elapsedTime += Time.deltaTime;
        }
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        for (int i = 1; i <= gridSize; i++)
        {
            if (keyboard[(Key)((int)Key.Digit1 + (i - 1))].wasPressedThisFrame ||
                (i <= 9 && keyboard[(Key)((int)Key.Numpad1 + (i - 1))].wasPressedThisFrame))
            {
                InputNumber(i);
                return;
            }
        }

        if (keyboard[Key.Digit0].wasPressedThisFrame ||
            keyboard[Key.Numpad0].wasPressedThisFrame ||
            keyboard[Key.Backspace].wasPressedThisFrame ||
            keyboard[Key.Delete].wasPressedThisFrame)
        {
            InputNumber(0);
        }
    }

    private bool CheckWinCondition()
    {
        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                if (board[r, c] == 0) return false;
            }
        }
        return true;
    }

    private void HandlePuzzleSolved()
    {
        isGameFinished = true;
        isInputLocked = true;
        SaveDailyGameState();

        if (selectedCell != null)
        {
            ClearCellSelection();
        }

        int currentCampaignIdx = PlayerPrefs.GetInt("CampaignLevelIndex", 0);
        bool showNextLevel = PlayerPrefs.GetString(MainMenuController.GameModeKey, "") == "Campaign" &&
                             CampaignManager.Instance.GetNextLevel(currentCampaignIdx) != null &&
                             CampaignManager.Instance.IsLevelUnlocked(currentCampaignIdx + 1);
        Debug.Log($"WinPanel openen: {winPanel.name}, next level: {showNextLevel}");
        winPanel.Show(elapsedTime, mistakeCount, showNextLevel);


        Debug.Log("Sudoku opgelost!");
    }

    private bool IsValidMove(int row, int col, int number)
    {
        if (board == null || row < 0 || row >= gridSize || col < 0 || col >= gridSize)
        {
            Debug.LogError($"Ongeldige zetpositie: rij {row}, kolom {col}, bord {gridSize}x{gridSize}.");
            return false;
        }

        for (int i = 0; i < gridSize; i++)
        {
            if (board[row, i] == number) return false;
            if (board[i, col] == number) return false;
        }

        int startRow = (row / subGridHeight) * subGridHeight;
        int startCol = (col / subGridWidth) * subGridWidth;

        for (int r = 0; r < subGridHeight; r++)
        {
            for (int c = 0; c < subGridWidth; c++)
            {
                if (board[startRow + r, startCol + c] == number) return false;
            }
        }
        return true;
    }

    private void RemoveNoteFromRelatedCells(int enteredRow, int enteredCol, int number)
    {
        if (cellsByCoordinate == null)
        {
            return;
        }

        int subGridStartRow = (enteredRow / subGridHeight) * subGridHeight;
        int subGridStartCol = (enteredCol / subGridWidth) * subGridWidth;

        for (int row = 0; row < cellsByCoordinate.GetLength(0); row++)
        {
            for (int col = 0; col < cellsByCoordinate.GetLength(1); col++)
            {
                if (row == enteredRow && col == enteredCol)
                {
                    continue;
                }

                bool sameRow = row == enteredRow;
                bool sameColumn = col == enteredCol;
                bool sameSubGrid = row >= subGridStartRow && row < subGridStartRow + subGridHeight &&
                                   col >= subGridStartCol && col < subGridStartCol + subGridWidth;

                if (sameRow || sameColumn || sameSubGrid)
                {
                    cellsByCoordinate[row, col]?.RemoveNote(number);
                }
            }
        }
    }

    private IEnumerator LockInputTemporarily()
    {
        isInputLocked = true;
        yield return new WaitForSeconds(wrongAnswerLockDuration);
        isInputLocked = false;
    }

    void Start()
    {
        isGameFinished = false;
        isInputLocked = false;
        elapsedTime = 0f;
        mistakeCount = 0;
        if (helperPanel == null || !helperPanel.gameObject.scene.IsValid())
        {
            helperPanel = FindFirstObjectByType<SudokuHelperPanel>();
        }

        if (helperPanel == null)
        {
            Debug.LogError("No SudokuHelperPanel instance was found in the active scene.", this);
        }

        if (helperPanel != null)
        {
            helperPanel.Bind(this);
        }
        UpdateActionButtonStates();


        winPanel.Hide();

        bool dailyMode = dailyModeInScene;
        string difficulty = configuredDifficulty;
        string dailyDate = GetConfiguredDailyDate();

        bool campaignMode = false;
        int campaignIndex = -1;

        if (useMenuSettings && PlayerPrefs.HasKey(MainMenuController.GameModeKey))
        {
            string mode = PlayerPrefs.GetString(MainMenuController.GameModeKey);
            dailyMode = mode == "Daily";
            campaignMode = mode == "Campaign";
            difficulty = PlayerPrefs.GetString(MainMenuController.DifficultyKey, configuredDifficulty);
            dailyDate = PlayerPrefs.GetString(MainMenuController.DailyDateKey, dailyDate);

            if (campaignMode)
            {
                campaignIndex = PlayerPrefs.GetInt("CampaignLevelIndex", 0);
                var level = CampaignManager.Instance.GetLevel(campaignIndex);
                if (level != null)
                {
                    difficulty = level.difficulty;
                    string sizeStr = level.gridSize;
                    if (sizeStr == "Nano") { gridSize = 4; subGridWidth = 2; subGridHeight = 2; }
                    else if (sizeStr == "Mini") { gridSize = 6; subGridWidth = 3; subGridHeight = 2; }
                    else { gridSize = 9; subGridWidth = 3; subGridHeight = 3; }
                }
            }

            UpdateTitle(mode, difficulty);
        }
        else
        {
            UpdateTitle(dailyMode ? "Daily" : "FreePlay", difficulty);
        }

        if (dailyMode)
        {
            difficulty = "Medium";
        }
        else if (!campaignMode && useMenuSettings && PlayerPrefs.HasKey(MainMenuController.GridSizeKey))
        {
            string sizeStr = PlayerPrefs.GetString(MainMenuController.GridSizeKey);
            if (sizeStr == "Nano") { gridSize = 4; subGridWidth = 2; subGridHeight = 2; }
            else if (sizeStr == "Mini") { gridSize = 6; subGridWidth = 3; subGridHeight = 2; }
            else { gridSize = 9; subGridWidth = 3; subGridHeight = 3; }
        }

        board = new int[gridSize, gridSize];

        string seedText = dailyMode
            ? $"Daily:{dailyDate}"
            : (campaignMode ? $"Campaign:{campaignIndex}" : $"FreePlay:{DateTime.Now.Ticks}");
        int seed = SudokuPuzzleGenerator.CreateSeed(seedText);
        int[,] puzzle = SudokuPuzzleGenerator.GenerateWithSolution(seed, difficulty, out solution, gridSize, subGridWidth, subGridHeight);

        LoadPuzzle(puzzle);

        hintCount = 0;
        isDailyGame = dailyMode;
        currentDailyDate = dailyMode ? dailyDate : null;
        currentGameSeed = seed;
        hasInitializedGame = true;

        ConfigureDailyDatePicker();

        if (isDailyGame)
        {
            RestoreDailyGameState();
        }

        UpdateHintBadge();
        if (helperPanel != null) helperPanel.ResetHintCooldown();
        if (helperPanel != null) helperPanel.SetNotesMode(isNotesMode);

        if (isDailyGame && isGameFinished)
        {
            winPanel.Show(elapsedTime, mistakeCount, false);
        }
    }

    private void ConfigureDailyDatePicker()
    {
        if (dailyDatePickerRoot == null)
        {
            MonoBehaviour[] sceneBehaviours = FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            MonoBehaviour picker = sceneBehaviours.FirstOrDefault(component =>
                component.gameObject.scene == gameObject.scene &&
                component.GetType().FullName == "UI.Dates.DatePicker");

            if (picker != null)
            {
                dailyDatePickerRoot = picker.gameObject;
            }
            else
            {
                MonoBehaviour controller = sceneBehaviours.FirstOrDefault(component =>
                    component.gameObject.scene == gameObject.scene &&
                    component.GetType().Name == "DailySudokuDatePickerController");
                dailyDatePickerRoot = controller != null ? controller.gameObject : null;
            }
        }

        if (dailyDatePickerRoot != null)
        {
            dailyDatePickerRoot.SetActive(isDailyGame);
        }
    }

    public void SelectDailyDate(DateTime selectedDate)
    {
        if (!hasInitializedGame || !isDailyGame)
        {
            return;
        }

        string selectedDateText = selectedDate.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (selectedDateText == currentDailyDate)
        {
            return;
        }

        SaveDailyGameState();

        currentDailyDate = selectedDateText;
        PlayerPrefs.SetString(MainMenuController.DailyDateKey, currentDailyDate);
        PlayerPrefs.Save();

        selectedCell = null;
        undoStack.Clear();
        isInputLocked = false;
        isGameFinished = false;
        isNotesMode = false;
        elapsedTime = 0f;
        mistakeCount = 0;
        hintCount = 0;

        currentGameSeed = SudokuPuzzleGenerator.CreateSeed($"Daily:{currentDailyDate}");
        int[,] puzzle = SudokuPuzzleGenerator.GenerateWithSolution(
            currentGameSeed, "Medium", out solution, gridSize, subGridWidth, subGridHeight);

        LoadPuzzle(puzzle);
        RestoreDailyGameState();
        ClearCellSelection();
        UpdateHintBadge();
        UpdateActionButtonStates();

        if (helperPanel != null)
        {
            helperPanel.ResetHintCooldown();
            helperPanel.SetNotesMode(isNotesMode);
        }

        winPanel.Hide();
        if (isGameFinished)
        {
            winPanel.Show(elapsedTime, mistakeCount, false);
        }
    }

    private string GetDailyStateKey()
    {
        return $"{DailyStateKeyPrefix}{currentGameSeed}";
    }

    public bool IsDailyDateCompleted(DateTime date)
    {
        Color? badgeColor = GetDailyDateBadgeColor(date);
        return badgeColor.HasValue && badgeColor.Value == new Color32(46, 204, 113, 255);
    }

    public Color? GetDailyDateBadgeColor(DateTime date)
    {
        int seed = SudokuPuzzleGenerator.CreateSeed(
            $"Daily:{date.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        string key = $"{DailyStateKeyPrefix}{seed}";
        if (!PlayerPrefs.HasKey(key))
        {
            return null;
        }

        try
        {
            DailyGameState state = JsonUtility.FromJson<DailyGameState>(PlayerPrefs.GetString(key));
            if (state == null || state.seed != seed)
            {
                return null;
            }

            if (state.isGameFinished)
            {
                return new Color32(46, 204, 113, 255);
            }

            bool hasEnteredValue = state.cells != null &&
                state.cells.Any(cell => !cell.isInitial && cell.value != 0);
            return hasEnteredValue ? new Color32(243, 156, 18, 255) : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private void SaveDailyGameState()
    {
        if (!hasInitializedGame || !isDailyGame || cellsByCoordinate == null)
        {
            return;
        }

        DailyGameState state = new DailyGameState
        {
            seed = currentGameSeed,
            gridSize = gridSize,
            elapsedTime = elapsedTime,
            mistakeCount = mistakeCount,
            hintCount = hintCount,
            isNotesMode = isNotesMode,
            isGameFinished = isGameFinished
        };

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                SudokuCell cell = cellsByCoordinate[row, col];
                if (cell == null)
                {
                    continue;
                }

                state.cells.Add(new DailyCellState
                {
                    row = row,
                    col = col,
                    value = cell.value,
                    isInitial = cell.isInitial,
                    notes = cell.GetNotes()
                });
            }
        }

        PlayerPrefs.SetString(GetDailyStateKey(), JsonUtility.ToJson(state));
        PlayerPrefs.Save();
    }

    private void RestoreDailyGameState()
    {
        string key = GetDailyStateKey();
        if (!PlayerPrefs.HasKey(key))
        {
            return;
        }

        DailyGameState state;
        try
        {
            state = JsonUtility.FromJson<DailyGameState>(PlayerPrefs.GetString(key));
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Kon dagelijkse Sudoku-status voor seed {currentGameSeed} niet laden: {exception.Message}");
            return;
        }

        if (state == null || state.seed != currentGameSeed || state.gridSize != gridSize ||
            state.cells == null || state.cells.Count != gridSize * gridSize)
        {
            Debug.LogWarning($"Opgeslagen dagelijkse Sudoku-status voor seed {currentGameSeed} is ongeldig.");
            return;
        }

        foreach (DailyCellState cellState in state.cells)
        {
            if (cellState.row < 0 || cellState.row >= gridSize ||
                cellState.col < 0 || cellState.col >= gridSize)
            {
                continue;
            }

            SudokuCell cell = cellsByCoordinate[cellState.row, cellState.col];
            if (cell == null)
            {
                continue;
            }

            board[cellState.row, cellState.col] = cellState.value;
            cell.SetValue(cellState.value, cellState.isInitial);
            cell.SetNotes(cellState.notes ?? new List<int>());
        }

        elapsedTime = Mathf.Max(0f, state.elapsedTime);
        mistakeCount = Mathf.Max(0, state.mistakeCount);
        hintCount = Mathf.Clamp(state.hintCount, 0, MaxHints);
        isNotesMode = state.isNotesMode;
        isGameFinished = state.isGameFinished;
        isInputLocked = isGameFinished;
        undoStack.Clear();
        ClearCellSelection();
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            SaveDailyGameState();
        }
    }

    private void OnApplicationQuit()
    {
        SaveDailyGameState();
    }

    private void OnDisable()
    {
        SaveDailyGameState();
    }

    private void UpdateTitle(string mode, string difficulty)
    {
        if (modeTitleText == null) return;
        if (mode == "Daily")
        {
            modeTitleText.text = "Daily";
        }
        else if (mode == "Campaign")
        {
            modeTitleText.text = $"CAMPAIGN - {difficulty.ToUpper()}";
        }
        else
        {
            modeTitleText.text = $"SUDOKU - {difficulty.ToUpper()}";
        }
    }

    private string GetConfiguredDailyDate()
    {
        if (useCurrentDate)
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        if (DateTime.TryParseExact(configuredDailyDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            return parsedDate.ToString("yyyy-MM-dd");
        }
        return DateTime.Now.ToString("yyyy-MM-dd");
    }

    private void LoadPuzzle(int[,] puzzle)
    {
        SudokuCell[] cells = FindObjectsByType<SudokuCell>(FindObjectsInactive.Include)
            .OrderByDescending(cell => cell.transform.position.y)
            .ThenBy(cell => cell.transform.position.x)
            .ToArray();

        if (cells.Length != gridSize * gridSize)
        {
            Debug.LogWarning($"Verwacht {gridSize * gridSize} SudokuCell-objecten, maar vond {cells.Length}.");
        }

        cellsByCoordinate = new SudokuCell[gridSize, gridSize];
        bool hasAllNamedCoordinates = true;

        foreach (SudokuCell cell in cells)
        {
            if (TryGetCellCoordinates(cell.gameObject.name, out int namedRow, out int namedCol) &&
                namedRow < gridSize && namedCol < gridSize &&
                cellsByCoordinate[namedRow, namedCol] == null)
            {
                cellsByCoordinate[namedRow, namedCol] = cell;
            }
        }

        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                if (cellsByCoordinate[r, c] == null)
                {
                    hasAllNamedCoordinates = false;
                }
            }
        }

        if (!hasAllNamedCoordinates)
        {
            cellsByCoordinate = CreateVisualCellMapping(cells);
        }

        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                int val = puzzle[r, c];
                board[r, c] = val;

                SudokuCell cell = cellsByCoordinate[r, c];
                if (cell != null)
                {
                    cell.row = r;
                    cell.col = c;
                    cell.ClearNotes();
                    cell.SetValue(val, val != 0);
                }
            }
        }
    }

    private void UpdateCellHighlights()
    {
        if (cellsByCoordinate == null)
        {
            return;
        }

        for (int row = 0; row < cellsByCoordinate.GetLength(0); row++)
        {
            for (int col = 0; col < cellsByCoordinate.GetLength(1); col++)
            {
                SudokuCell cell = cellsByCoordinate[row, col];
                if (cell == null)
                {
                    continue;
                }

                bool isSelected = cell == selectedCell;
                bool isRelated = selectedCell != null && !isSelected &&
                                 (row == selectedCell.row || col == selectedCell.col);
                cell.SetSelected(isSelected);
                cell.SetRelatedHighlight(isRelated);
            }
        }
    }

    private void ClearCellSelection()
    {
        selectedCell = null;
        UpdateCellHighlights();
    }

    private SudokuCell[,] CreateVisualCellMapping(SudokuCell[] cells)
    {
        SudokuCell[,] mapping = new SudokuCell[gridSize, gridSize];
        for (int index = 0; index < cells.Length && index < gridSize * gridSize; index++)
        {
            int row = index / gridSize;
            int col = index % gridSize;
            mapping[row, col] = cells[index];
        }

        return mapping;
    }

    private bool TryGetCellCoordinates(string cellName, out int row, out int col)
    {
        row = -1;
        col = -1;

        string[] parts = cellName.Split('_');
        if (parts.Length != 3 || parts[0] != "Cell")
        {
            return false;
        }

        if (!int.TryParse(parts[1], out row) || !int.TryParse(parts[2], out col))
        {
            return false;
        }

        return row >= 0 && col >= 0;
    }

    private const string MainMenuScene = "MainMenu";
    public void OpenMainMenu()
    {
        SaveDailyGameState();
        SceneManager.LoadScene(MainMenuScene);
    }
}