using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

// Base class for all puzzles
public abstract class PuzzleBase : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] protected string puzzleID;
    [SerializeField] protected bool isCompleted = false;

    [Header("Events")]
    public UnityEvent OnPuzzleComplete;
    public UnityEvent OnPuzzleFailed;

    [Header("Audio")]
    [SerializeField] protected AudioSource puzzleAudio;
    [SerializeField] protected AudioClip completeSound;
    [SerializeField] protected AudioClip failSound;

    public bool IsCompleted => isCompleted;
    public string PuzzleID => puzzleID;

    protected virtual void Start()
    {
        if (string.IsNullOrEmpty(puzzleID))
        {
            puzzleID = gameObject.name;
        }
    }

    protected virtual void CompletePuzzle()
    {
        if (isCompleted) return;

        isCompleted = true;

        if (puzzleAudio != null && completeSound != null)
        {
            puzzleAudio.PlayOneShot(completeSound);
        }

        OnPuzzleComplete?.Invoke();

        // Notify puzzle manager
        PuzzleManager.Instance?.OnPuzzleCompleted(puzzleID);
    }

    protected virtual void FailPuzzle()
    {
        if (puzzleAudio != null && failSound != null)
        {
            puzzleAudio.PlayOneShot(failSound);
        }

        OnPuzzleFailed?.Invoke();
    }

    public abstract void ResetPuzzle();
}

// Collectible Puzzle - Collect all items to complete
public class CollectiblePuzzle : PuzzleBase
{
    [Header("Collectible Settings")]
    [SerializeField] private int requiredCollectibles = 3;

    // Uses CollectibleItem (your existing pickup script) instead of a missing Collectible class
    [SerializeField] private List<CollectibleItem> collectibles = new List<CollectibleItem>();

    private int collectedCount = 0;

    protected override void Start()
    {
        base.Start();

        // Register listeners that detect when CollectibleItem was collected (without modifying CollectibleItem)
        foreach (var item in collectibles)
        {
            if (item == null) continue;

            var listener = item.gameObject.GetComponent<CollectibleCollectedListener>();
            if (listener == null)
                listener = item.gameObject.AddComponent<CollectibleCollectedListener>();

            listener.Bind(item, this);
        }
    }

    internal void NotifyCollectedOnce()
    {
        collectedCount++;

        if (collectedCount >= requiredCollectibles)
        {
            CompletePuzzle();
        }
    }

    public override void ResetPuzzle()
    {
        collectedCount = 0;
        isCompleted = false;

        foreach (var item in collectibles)
        {
            if (item != null)
            {
                item.ResetItem();
            }
        }
    }
}

/// <summary>
/// Helper component: watches CollectibleItem's private "isCollected" flag via reflection,
/// so we don't have to change CollectibleItem.
/// </summary>
public class CollectibleCollectedListener : MonoBehaviour
{
    private CollectibleItem item;
    private CollectiblePuzzle puzzle;
    private bool fired;

    private System.Reflection.FieldInfo isCollectedField;

    public void Bind(CollectibleItem item, CollectiblePuzzle puzzle)
    {
        this.item = item;
        this.puzzle = puzzle;
        fired = false;

        // cache reflection
        isCollectedField = typeof(CollectibleItem).GetField(
            "isCollected",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        );
    }

    private void Update()
    {
        if (fired || item == null || puzzle == null || isCollectedField == null) return;

        bool isCollected = (bool)isCollectedField.GetValue(item);
        if (isCollected)
        {
            fired = true;
            puzzle.NotifyCollectedOnce();
        }
    }

    private void OnEnable()
    {
        // allow counting again after ResetItem()
        fired = false;
    }
}

// Symbol/Code Puzzle - Input correct sequence
public class SymbolPuzzle : PuzzleBase
{
    [Header("Symbol Puzzle Settings")]
    [SerializeField] private int[] correctSequence = new int[] { 1, 2, 3, 4 };
    [SerializeField] private int maxAttempts = 3;
    [SerializeField] private float resetDelay = 2f;

    private List<int> currentSequence = new List<int>();
    private int attemptsLeft;

    protected override void Start()
    {
        base.Start();
        attemptsLeft = maxAttempts;
    }

    public void InputSymbol(int symbolID)
    {
        if (isCompleted) return;

        currentSequence.Add(symbolID);

        // Check if sequence is correct so far
        if (!IsSequenceCorrectSoFar())
        {
            FailPuzzle();
            ResetSequence();
            return;
        }

        // Check if complete
        if (currentSequence.Count == correctSequence.Length)
        {
            CompletePuzzle();
        }
    }

    private bool IsSequenceCorrectSoFar()
    {
        for (int i = 0; i < currentSequence.Count; i++)
        {
            if (currentSequence[i] != correctSequence[i])
            {
                attemptsLeft--;

                if (attemptsLeft <= 0)
                {
                    // Game over or penalty
                }

                return false;
            }
        }
        return true;
    }

    private void ResetSequence()
    {
        currentSequence.Clear();
    }

    public override void ResetPuzzle()
    {
        ResetSequence();
        attemptsLeft = maxAttempts;
        isCompleted = false;
    }
}

// Lever/Switch Puzzle - Activate in correct order
public class SwitchPuzzle : PuzzleBase
{
    [System.Serializable]
    public class Switch
    {
        public GameObject switchObject;
        public bool isActive;
    }

    [Header("Switch Puzzle Settings")]
    [SerializeField] private List<Switch> switches = new List<Switch>();
    [SerializeField] private bool requireOrder = true;
    [SerializeField] private int[] correctOrder;

    private List<int> activatedOrder = new List<int>();

    public void ActivateSwitch(int switchIndex)
    {
        if (isCompleted || switchIndex < 0 || switchIndex >= switches.Count) return;

        switches[switchIndex].isActive = !switches[switchIndex].isActive;

        if (switches[switchIndex].isActive)
        {
            activatedOrder.Add(switchIndex);
        }
        else
        {
            activatedOrder.Remove(switchIndex);
        }

        CheckPuzzleState();
    }

    private void CheckPuzzleState()
    {
        if (requireOrder)
        {
            // Check if activated in correct order
            if (activatedOrder.Count == correctOrder.Length)
            {
                bool correct = true;
                for (int i = 0; i < correctOrder.Length; i++)
                {
                    if (activatedOrder[i] != correctOrder[i])
                    {
                        correct = false;
                        break;
                    }
                }

                if (correct)
                {
                    CompletePuzzle();
                }
                else
                {
                    FailPuzzle();
                    ResetPuzzle();
                }
            }
        }
        else
        {
            // Check if all switches are active
            bool allActive = true;
            foreach (var sw in switches)
            {
                if (!sw.isActive)
                {
                    allActive = false;
                    break;
                }
            }

            if (allActive)
            {
                CompletePuzzle();
            }
        }
    }

    public override void ResetPuzzle()
    {
        activatedOrder.Clear();
        foreach (var sw in switches)
        {
            sw.isActive = false;
        }
        isCompleted = false;
    }
}