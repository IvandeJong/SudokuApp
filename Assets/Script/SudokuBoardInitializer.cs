using UnityEngine;
using System.Linq;

public class SudokuBoardInitializer : MonoBehaviour
{
    [SerializeField] private int gridSize = 9;

    [ContextMenu("Initialize Board Now")]
    public void InitializeBoard()
    {
        SudokuCell[] cells = GetComponentsInChildren<SudokuCell>(true);

        int detectedGridSize = Mathf.RoundToInt(Mathf.Sqrt(cells.Length));
        if (detectedGridSize * detectedGridSize == cells.Length)
        {
            gridSize = detectedGridSize;
        }

        if (cells.Length != gridSize * gridSize)
        {
            Debug.LogWarning($"Verwacht {gridSize * gridSize} cellen, maar vond {cells.Length}. Initialisatie gaat toch door.");
        }

        var orderedByY = cells
            .OrderByDescending(c => c.transform.position.y)
            .ToArray();

        for (int row = 0; row < gridSize; row++)
        {
            var rowCells = orderedByY
                .Skip(row * gridSize)
                .Take(gridSize)
                .OrderBy(c => c.transform.position.x)
                .ToArray();

            for (int col = 0; col < gridSize; col++)
            {
                if (col < rowCells.Length)
                {
                    rowCells[col].row = row;
                    rowCells[col].col = col;
                    rowCells[col].gameObject.name = $"Cell_{row}_{col}";
                    rowCells[col].SetValue(0);
                }
            }
        }
        Debug.Log($"Alle {cells.Length} cellen zijn permanent ingesteld voor {gridSize}x{gridSize}!");
    }
}