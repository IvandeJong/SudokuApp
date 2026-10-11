using UnityEngine;
using System.Linq;

public class SudokuBoardInitializer : MonoBehaviour
{
    [ContextMenu("Initialize Board Now")]
    public void InitializeBoard()
    {
        SudokuCell[] cells = GetComponentsInChildren<SudokuCell>(true);

        if (cells.Length != 81)
        {
            Debug.LogWarning($"Verwacht 81 cellen, maar vond {cells.Length}. Initialisatie gaat toch door.");
        }

        var orderedByY = cells
            .OrderByDescending(c => c.transform.position.y)
            .ToArray();

        for (int row = 0; row < 9; row++)
        {
            var rowCells = orderedByY
                .Skip(row * 9)
                .Take(9)
                .OrderBy(c => c.transform.position.x)
                .ToArray();

            for (int col = 0; col < 9; col++)
            {
                if (col < rowCells.Length)
                {
                    rowCells[col].row = row;
                    rowCells[col].col = col;
                    rowCells[col].gameObject.name = $"Cell_{row}_{col}";
                    rowCells[col].SetValue(0); // Zorg ervoor dat de waarde van de cel leeg is bij initialisatie
                }
            }
        }
        Debug.Log("Alle 81 cellen zijn permanent ingesteld!");
    }
}