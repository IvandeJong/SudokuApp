using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System;
using System.Globalization;

public class SudokuGameManager : MonoBehaviour
{
    private SudokuCell selectedCell;
    [SerializeField] private float wrongAnswerLockDuration = 1.5f;
    [SerializeField] private float correctAnswerFlashDuration = 1.5f;
    [SerializeField] private SudokuWinPanel winPanel;
    [SerializeField] private TMPro.TextMeshProUGUI modeTitleText;

    [Header("Grid Configuration")]
    [SerializeField] private int gridSize = 9;
    [SerializeField] private int subGridWidth = 3;
    [SerializeField] private int subGridHeight = 3;

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

    public void SelectCell(SudokuCell cell)
    {
        if (selectedCell != null)
        {
            selectedCell.SetSelected(false);
        }
        selectedCell = cell;

        if (selectedCell != null)
        {
            selectedCell.SetSelected(true);
        }

        Debug.Log($"Geselecteerde cel: Rij {cell.row}, Kolom {cell.col}");
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
            board[row, col] = 0;
            selectedCell.SetValue(0);
            return;
        }

        if (IsValidMove(row, col, number))
        {
            board[row, col] = number;
            selectedCell.SetValue(number);
            selectedCell.SetSelected(false);
            selectedCell.FlashCorrectAnswer(correctAnswerFlashDuration);
            selectedCell = null;
        }
        else
        {
            mistakeCount++;
            selectedCell.FlashWrongAnswer(wrongAnswerLockDuration);
            StartCoroutine(LockInputTemporarily());
        }
        
        if (CheckWinCondition())
        {
            HandlePuzzleSolved();
        }
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

        if (selectedCell != null)
        {
            selectedCell.SetSelected(false);
            selectedCell = null;
        }

        if (winPanel != null)
        {
            winPanel.Show(elapsedTime, mistakeCount);
        }

        Debug.Log("Sudoku opgelost!");
    }

    private bool IsValidMove(int row, int col, int number)
    {
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

        if (winPanel == null)
        {
            winPanel = FindFirstObjectByType<SudokuWinPanel>(FindObjectsInactive.Include);
        }

        if (winPanel != null)
        {
            winPanel.Hide();
        }

        bool dailyMode = dailyModeInScene;
        string difficulty = configuredDifficulty;
        string dailyDate = GetConfiguredDailyDate();

        if (useMenuSettings && PlayerPrefs.HasKey(MainMenuController.GameModeKey))
        {
            string mode = PlayerPrefs.GetString(MainMenuController.GameModeKey);
            dailyMode = mode == "Daily";
            difficulty = PlayerPrefs.GetString(MainMenuController.DifficultyKey, configuredDifficulty);
            dailyDate = PlayerPrefs.GetString(MainMenuController.DailyDateKey, dailyDate);
            
            UpdateTitle(mode, difficulty);
        }
        else
        {
            UpdateTitle(dailyMode ? "Daily" : "FreePlay", difficulty);
        }

        if (dailyMode)
        {
            difficulty = "Medium";
            gridSize = 9;
            subGridWidth = 3;
            subGridHeight = 3;
        }
        else if (useMenuSettings && PlayerPrefs.HasKey(MainMenuController.GridSizeKey))
        {
            string sizeStr = PlayerPrefs.GetString(MainMenuController.GridSizeKey);
            if (sizeStr == "Nano") { gridSize = 4; subGridWidth = 2; subGridHeight = 2; }
            else if (sizeStr == "Mini") { gridSize = 6; subGridWidth = 3; subGridHeight = 2; }
            else { gridSize = 9; subGridWidth = 3; subGridHeight = 3; }
        }

        board = new int[gridSize, gridSize];

        string seedText = dailyMode
            ? $"Daily:{dailyDate}"
            : $"FreePlay:{DateTime.Now.Ticks}";
        int seed = SudokuPuzzleGenerator.CreateSeed(seedText);
        int[,] puzzle = SudokuPuzzleGenerator.Generate(seed, difficulty, gridSize, subGridWidth, subGridHeight);

        LoadPuzzle(puzzle);
    }

    private void UpdateTitle(string mode, string difficulty)
    {
        if (modeTitleText == null) return;
        if (mode == "Daily")
        {
            modeTitleText.text = "DAILY SUDOKU";
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
        SudokuCell[] cells = FindObjectsByType<SudokuCell>(FindObjectsSortMode.None);

        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                int val = puzzle[r, c];
                board[r, c] = val;

                foreach (var cell in cells)
                {
                    if (cell.row == r && cell.col == c)
                    {
                        cell.SetValue(val, val != 0);
                        break;
                    }
                }
            }
        }
    }

    private const string MainMenuScene = "MainMenu";
    public void OpenMainMenu()
    {
        SceneManager.LoadScene(MainMenuScene);
    }
}