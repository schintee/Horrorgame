// Puzzle Manager - Manages all puzzles in the scene
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Puzzle Tracking")]
    [SerializeField] private List<PuzzleBase> allPuzzles = new List<PuzzleBase>();

    private Dictionary<string, PuzzleBase> puzzleDictionary = new Dictionary<string, PuzzleBase>();
    private HashSet<string> completedPuzzles = new HashSet<string>();

    public int TotalPuzzles => allPuzzles.Count;
    public int CompletedPuzzlesCount => completedPuzzles.Count;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Find all puzzles if list is empty
        if (allPuzzles.Count == 0)
        {
            allPuzzles.AddRange(FindObjectsOfType<PuzzleBase>());
        }

        // Build dictionary
        foreach (var puzzle in allPuzzles)
        {
            if (!puzzleDictionary.ContainsKey(puzzle.PuzzleID))
            {
                puzzleDictionary.Add(puzzle.PuzzleID, puzzle);
            }
        }
    }

    public void OnPuzzleCompleted(string puzzleID)
    {
        if (!completedPuzzles.Contains(puzzleID))
        {
            completedPuzzles.Add(puzzleID);

            Debug.Log($"Puzzle completed: {puzzleID} ({CompletedPuzzlesCount}/{TotalPuzzles})");

            // Check if all puzzles complete
            if (CompletedPuzzlesCount >= TotalPuzzles)
            {
                OnAllPuzzlesComplete();
            }
        }
    }

    private void OnAllPuzzlesComplete()
    {
        Debug.Log("All puzzles completed!");
        // Trigger game progression
        //GameManager.Instance?.OnAllPuzzlesComplete();
    }

    public bool IsPuzzleCompleted(string puzzleID)
    {
        return completedPuzzles.Contains(puzzleID);
    }

    public PuzzleBase GetPuzzle(string puzzleID)
    {
        return puzzleDictionary.ContainsKey(puzzleID) ? puzzleDictionary[puzzleID] : null;
    }
}