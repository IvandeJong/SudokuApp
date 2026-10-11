using System;
using System.Collections.Generic;

public static class SudokuPuzzleGenerator
{
    public static int[,] Generate(int seed, string difficulty, int gridSize = 9, int subGridWidth = 3, int subGridHeight = 3)
    {
        Random random = new Random(seed);
        int[,] solution = new int[gridSize, gridSize];
        FillSolution(solution, random, gridSize, subGridWidth, subGridHeight);

        int clues = GetClueCount(difficulty, gridSize);
        int[,] puzzle = (int[,])solution.Clone();
        RemoveClues(puzzle, clues, random, gridSize, subGridWidth, subGridHeight);
        return puzzle;
    }

    public static int CreateSeed(string text)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (char character in text)
            {
                hash ^= character;
                hash *= 16777619;
            }

            return (int)hash;
        }
    }

    private static int GetClueCount(string difficulty, int gridSize)
    {
        float ratio = 0.4f; // Default Medium
        switch (difficulty)
        {
            case "Easy":
                ratio = 0.5f;
                break;
            case "Hard":
                ratio = 0.3f;
                break;
        }
        return (int)(gridSize * gridSize * ratio);
    }

    private static bool FillSolution(int[,] board, Random random, int gridSize, int subGridWidth, int subGridHeight)
    {
        if (!FindEmptyCell(board, out int row, out int col, gridSize))
        {
            return true;
        }

        List<int> numbers = CreateShuffledNumbers(random, gridSize);
        foreach (int number in numbers)
        {
            if (!IsSafe(board, row, col, number, gridSize, subGridWidth, subGridHeight))
            {
                continue;
            }

            board[row, col] = number;
            if (FillSolution(board, random, gridSize, subGridWidth, subGridHeight))
            {
                return true;
            }

            board[row, col] = 0;
        }

        return false;
    }

    private static void RemoveClues(int[,] puzzle, int clueCount, Random random, int gridSize, int subGridWidth, int subGridHeight)
    {
        List<int> positions = new List<int>();
        int totalCells = gridSize * gridSize;
        for (int position = 0; position < totalCells; position++)
        {
            positions.Add(position);
        }

        Shuffle(positions, random);
        int currentClues = totalCells;

        foreach (int position in positions)
        {
            if (currentClues <= clueCount)
            {
                break;
            }

            int row = position / gridSize;
            int col = position % gridSize;
            int originalValue = puzzle[row, col];
            puzzle[row, col] = 0;

            int[,] testBoard = (int[,])puzzle.Clone();
            if (CountSolutions(testBoard, 2, gridSize, subGridWidth, subGridHeight) == 1)
            {
                currentClues--;
            }
            else
            {
                puzzle[row, col] = originalValue;
            }
        }
    }

    private static int CountSolutions(int[,] board, int limit, int gridSize, int subGridWidth, int subGridHeight)
    {
        if (!FindBestEmptyCell(board, out int row, out int col, out List<int> candidates, gridSize, subGridWidth, subGridHeight))
        {
            return 1;
        }

        int solutionCount = 0;
        foreach (int number in candidates)
        {
            board[row, col] = number;
            solutionCount += CountSolutions(board, limit, gridSize, subGridWidth, subGridHeight);
            board[row, col] = 0;

            if (solutionCount >= limit)
            {
                return solutionCount;
            }
        }

        return solutionCount;
    }

    private static bool FindEmptyCell(int[,] board, out int row, out int col, int gridSize)
    {
        for (row = 0; row < gridSize; row++)
        {
            for (col = 0; col < gridSize; col++)
            {
                if (board[row, col] == 0)
                {
                    return true;
                }
            }
        }

        row = -1;
        col = -1;
        return false;
    }

    private static bool FindBestEmptyCell(int[,] board, out int bestRow, out int bestCol, out List<int> bestCandidates, int gridSize, int subGridWidth, int subGridHeight)
    {
        bestRow = -1;
        bestCol = -1;
        bestCandidates = null;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                if (board[row, col] != 0)
                {
                    continue;
                }

                List<int> candidates = GetCandidates(board, row, col, gridSize, subGridWidth, subGridHeight);
                if (bestCandidates == null || candidates.Count < bestCandidates.Count)
                {
                    bestRow = row;
                    bestCol = col;
                    bestCandidates = candidates;

                    if (candidates.Count <= 1)
                    {
                        return true;
                    }
                }
            }
        }

        return bestCandidates != null;
    }

    private static List<int> GetCandidates(int[,] board, int row, int col, int gridSize, int subGridWidth, int subGridHeight)
    {
        List<int> candidates = new List<int>();
        for (int number = 1; number <= gridSize; number++)
        {
            if (IsSafe(board, row, col, number, gridSize, subGridWidth, subGridHeight))
            {
                candidates.Add(number);
            }
        }

        return candidates;
    }

    private static bool IsSafe(int[,] board, int row, int col, int number, int gridSize, int subGridWidth, int subGridHeight)
    {
        for (int index = 0; index < gridSize; index++)
        {
            if (board[row, index] == number || board[index, col] == number)
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
                if (board[currentRow, currentCol] == number)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static List<int> CreateShuffledNumbers(Random random, int gridSize)
    {
        List<int> numbers = new List<int>();
        for (int number = 1; number <= gridSize; number++)
        {
            numbers.Add(number);
        }

        Shuffle(numbers, random);
        return numbers;
    }

    private static void Shuffle<T>(IList<T> values, Random random)
    {
        for (int index = values.Count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            T value = values[index];
            values[index] = values[swapIndex];
            values[swapIndex] = value;
        }
    }
}
