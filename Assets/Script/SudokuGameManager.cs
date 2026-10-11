using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class SudokuGameManager : MonoBehaviour
{
    private SudokuCell selectedCell;
    [SerializeField] private float wrongAnswerLockDuration = 1.5f;
    [SerializeField] private float correctAnswerFlashDuration = 1.5f;
    [SerializeField] private SudokuWinPanel winPanel;
    private bool isInputLocked;
    private bool isGameFinished;
    private float elapsedTime;
    private int mistakeCount;

    // Een 9x9 rooster om de cijfers centraal bij te houden
    private int[,] board = new int[9, 9];

    // Wordt aangeroepen wanneer een cel wordt aangeklikt
    public void SelectCell(SudokuCell cell)
    {
        // Zet de kleur van de oude cel terug
        if (selectedCell != null)
        {
            selectedCell.SetSelected(false);
        }
        selectedCell = cell;

        // Markeer de nieuwe cel
        if (selectedCell != null)
        {
            selectedCell.SetSelected(true);
        }

        Debug.Log($"Geselecteerde cel: Rij {cell.row}, Kolom {cell.col}");
    }

    public void InputNumber(int number)
    {
        if (isGameFinished)
        {
            return;
        }

        if (isInputLocked)
        {
            Debug.LogWarning("Wacht even tot de foutmelding klaar is.");
            return;
        }

        if (selectedCell == null)
        {
            Debug.LogWarning("Geen cel geselecteerd!");
            return;
        }

        // In InputNumber, voeg deze controle toe vooraan:
        if (selectedCell.isInitial)
        {
            Debug.LogWarning("Dit is een vast startgetal en kan niet worden aangepast!");
            return;
        }

        int row = selectedCell.row;
        int col = selectedCell.col;

        // Als het getal 0 is (wissen), mogen we dat altijd doen
        if (number == 0)
        {
            board[row, col] = 0;
            selectedCell.SetValue(0);
            return;
        }

        // Controleer of de zet geldig is volgens de Sudoku-regels
        if (IsValidMove(row, col, number))
        {
            board[row, col] = number;
            selectedCell.SetValue(number);
            selectedCell.SetSelected(false);
            selectedCell.FlashCorrectAnswer(correctAnswerFlashDuration);
            selectedCell = null;
            Debug.Log($"Cijfer {number} geplaatst op Rij {row}, Kolom {col}");
        }
        else
        {
            mistakeCount++;
            selectedCell.FlashWrongAnswer(wrongAnswerLockDuration);
            StartCoroutine(LockInputTemporarily());
            Debug.LogWarning($"Ongeldige zet! Cijfer {number} staat al in deze rij, kolom of 3x3 blok.");
        }
        
        // Voeg dit toe onderaan in InputNumber, net nadat een getal succesvol is geplaatst:
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
        if (keyboard == null)
        {
            return;
        }

        for (int i = 1; i <= 9; i++)
        {
            if (keyboard[(Key)((int)Key.Digit1 + (i - 1))].wasPressedThisFrame ||
                keyboard[(Key)((int)Key.Numpad1 + (i - 1))].wasPressedThisFrame)
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
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                // Als er ergens nog een 0 staat, is de puzzel nog niet af
                if (board[r, c] == 0) return false;
            }
        }

        // Als het bord helemaal vol is, is het gewonnen (aangenomen dat allezetten via IsValidMove gingen)
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
        else
        {
            Debug.LogError("Sudoku is opgelost, maar Win Panel is niet gekoppeld of gevonden.");
        }

        Debug.Log("Sudoku opgelost!");
    }

    private bool IsValidMove(int row, int col, int number)
    {
        // 1. Controleer de rij en kolom
        for (int i = 0; i < 9; i++)
        {
            if (board[row, i] == number) return false;
            if (board[i, col] == number) return false;
        }

        // 2. Controleer het 3x3 subgrid
        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
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

    // Een eenvoudige startpuzzel (0 = lege cel)
    private int[,] startingPuzzle = new int[,]
    {
        {5, 3, 0, 0, 7, 0, 0, 0, 0},
        {6, 0, 0, 1, 9, 5, 0, 0, 0},
        {0, 9, 8, 0, 0, 0, 0, 6, 0},
        {8, 0, 0, 0, 6, 0, 0, 0, 3},
        {4, 0, 0, 8, 0, 3, 0, 0, 1},
        {7, 0, 0, 0, 2, 0, 0, 0, 6},
        {0, 6, 0, 0, 0, 0, 2, 8, 0},
        {0, 0, 0, 4, 1, 9, 0, 0, 5},
        {0, 0, 0, 0, 8, 0, 0, 7, 9}
    };

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
        else
        {
            Debug.LogWarning("Geen SudokuWinPanel gevonden. Voeg SudokuWinPanel toe aan de scene en koppel hem aan Win Panel.");
        }

        LoadPuzzle(startingPuzzle);
    }

    private void LoadPuzzle(int[,] puzzle)
    {
        // Zoek alle cellen op het bord
        SudokuCell[] cells = FindObjectsByType<SudokuCell>(FindObjectsSortMode.None);

        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                int val = puzzle[r, c];
                board[r, c] = val;

                // Vind de juiste cel op basis van rij en kolom en zet de waarde
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
        Debug.Log("Startpuzzel ingeladen!");
    }

    private const string MainMenuScene = "MainMenu";
    public void OpenMainMenu()
    {
        SceneManager.LoadScene(MainMenuScene);
    }
}