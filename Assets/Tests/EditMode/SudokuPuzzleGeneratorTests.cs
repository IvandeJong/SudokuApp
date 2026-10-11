using NUnit.Framework;
using System.Collections.Generic;

public class SudokuPuzzleGeneratorTests
{
    [TestCase(9, 3, 3)]
    [TestCase(6, 3, 2)]
    [TestCase(4, 2, 2)]
    public void GeneratedPuzzleHasExpectedSizeAndOneSolution(int gridSize, int subGridWidth, int subGridHeight)
    {
        int[,] puzzle = SudokuPuzzleGenerator.Generate(
            12345 + gridSize,
            "Medium",
            gridSize,
            subGridWidth,
            subGridHeight);

        Assert.That(puzzle.GetLength(0), Is.EqualTo(gridSize));
        Assert.That(puzzle.GetLength(1), Is.EqualTo(gridSize));
        Assert.That(IsValidPuzzle(puzzle, gridSize, subGridWidth, subGridHeight), Is.True);
        Assert.That(CountSolutions(puzzle, gridSize, subGridWidth, subGridHeight, 2), Is.EqualTo(1));
    }

    [TestCase(9, 3, 3)]
    [TestCase(6, 3, 2)]
    [TestCase(4, 2, 2)]
    public void DifficultyLevelsHaveExpectedClueOrdering(int gridSize, int subGridWidth, int subGridHeight)
    {
        int easyClues = CountClues(SudokuPuzzleGenerator.Generate(100 + gridSize, "Easy", gridSize, subGridWidth, subGridHeight));
        int mediumClues = CountClues(SudokuPuzzleGenerator.Generate(200 + gridSize, "Medium", gridSize, subGridWidth, subGridHeight));
        int hardClues = CountClues(SudokuPuzzleGenerator.Generate(300 + gridSize, "Hard", gridSize, subGridWidth, subGridHeight));

        Assert.That(easyClues, Is.GreaterThanOrEqualTo(mediumClues));
        Assert.That(mediumClues, Is.GreaterThanOrEqualTo(hardClues));
        Assert.That(hardClues, Is.GreaterThan(0));
    }

    [TestCase("Easy")]
    [TestCase("Medium")]
    [TestCase("Hard")]
    public void ClassicPuzzleIsSolveableAtEveryDifficulty(string difficulty)
    {
        int[,] puzzle = SudokuPuzzleGenerator.Generate(9876, difficulty, 9, 3, 3);

        Assert.That(IsValidPuzzle(puzzle, 9, 3, 3), Is.True);
        Assert.That(CountSolutions(puzzle, 9, 3, 3, 2), Is.EqualTo(1));
    }

    private static int CountClues(int[,] puzzle)
    {
        int clues = 0;
        for (int row = 0; row < puzzle.GetLength(0); row++)
        {
            for (int col = 0; col < puzzle.GetLength(1); col++)
            {
                if (puzzle[row, col] != 0)
                {
                    clues++;
                }
            }
        }

        return clues;
    }

    private static bool IsValidPuzzle(int[,] puzzle, int gridSize, int subGridWidth, int subGridHeight)
    {
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                int value = puzzle[row, col];
                if (value < 0 || value > gridSize)
                {
                    return false;
                }

                if (value != 0 && !IsSafe(puzzle, row, col, value, gridSize, subGridWidth, subGridHeight))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static int CountSolutions(int[,] puzzle, int gridSize, int subGridWidth, int subGridHeight, int limit)
    {
        if (!FindBestEmptyCell(puzzle, gridSize, subGridWidth, subGridHeight, out int row, out int col, out List<int> candidates))
        {
            return 1;
        }

        int solutions = 0;
        foreach (int candidate in candidates)
        {
            puzzle[row, col] = candidate;
            solutions += CountSolutions(puzzle, gridSize, subGridWidth, subGridHeight, limit);
            puzzle[row, col] = 0;

            if (solutions >= limit)
            {
                return solutions;
            }
        }

        return solutions;
    }

    private static bool FindBestEmptyCell(int[,] puzzle, int gridSize, int subGridWidth, int subGridHeight, out int bestRow, out int bestCol, out List<int> bestCandidates)
    {
        bestRow = -1;
        bestCol = -1;
        bestCandidates = null;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                if (puzzle[row, col] != 0)
                {
                    continue;
                }

                List<int> candidates = GetCandidates(puzzle, row, col, gridSize, subGridWidth, subGridHeight);
                if (bestCandidates == null || candidates.Count < bestCandidates.Count)
                {
                    bestRow = row;
                    bestCol = col;
                    bestCandidates = candidates;
                }
            }
        }

        return bestCandidates != null;
    }

    private static List<int> GetCandidates(int[,] puzzle, int row, int col, int gridSize, int subGridWidth, int subGridHeight)
    {
        List<int> candidates = new List<int>();
        for (int value = 1; value <= gridSize; value++)
        {
            if (IsSafe(puzzle, row, col, value, gridSize, subGridWidth, subGridHeight))
            {
                candidates.Add(value);
            }
        }

        return candidates;
    }

    private static bool IsSafe(int[,] puzzle, int row, int col, int value, int gridSize, int subGridWidth, int subGridHeight)
    {
        for (int index = 0; index < gridSize; index++)
        {
            if (index != col && puzzle[row, index] == value)
            {
                return false;
            }

            if (index != row && puzzle[index, col] == value)
            {
                return false;
            }
        }

        int startRow = (row / subGridHeight) * subGridHeight;
        int startCol = (col / subGridWidth) * subGridWidth;
        for (int currentRow = startRow; currentRow < startRow + subGridHeight; currentRow++)
        {
            for (int currentCol = startCol; currentCol < startCol + subGridWidth; currentCol++)
            {
                if ((currentRow != row || currentCol != col) && puzzle[currentRow, currentCol] == value)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
