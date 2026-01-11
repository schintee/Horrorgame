using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private Transform doorTransform;
    [SerializeField] private bool isLocked = false;
    [SerializeField] private string requiredKey = "";
    [SerializeField] private string requiredPuzzleID = "";

    [Header("Animation")]
    [SerializeField] private bool slideOpen = false; // false = rotate, true = slide
    [SerializeField] private Vector3 openPosition = Vector3.zero; // For sliding
    [SerializeField] private Vector3 openRotation = new Vector3(0, 90, 0); // For rotating
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool autoClose = false;
    [SerializeField] private float autoCloseDelay = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource doorAudio;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;

    // State
    private bool isOpen = false;
    private bool isMoving = false;
    private Vector3 closedPosition;
    private Quaternion closedRotation;
    private Coroutine autoCloseCoroutine;

    public bool IsLocked => isLocked;
    public bool IsOpen => isOpen;

    void Start()
    {
        if (doorTransform == null)
        {
            doorTransform = transform;
        }

        closedPosition = doorTransform.localPosition;
        closedRotation = doorTransform.localRotation;
    }

    public void Interact()
    {
        if (isMoving) return;

        // Check if locked
        if (isLocked)
        {
            // Check if player has key
            if (!string.IsNullOrEmpty(requiredKey))
            {
                // Unity 6+: prefer FindAnyObjectByType
                PlayerInventory inventory = FindAnyObjectByType<PlayerInventory>();
                if (inventory != null && inventory.HasKey(requiredKey))
                {
                    Unlock();
                }
                else
                {
                    PlayLockedSound();
                    return;
                }
            }
            // Check if puzzle is complete
            else if (!string.IsNullOrEmpty(requiredPuzzleID))
            {
                if (PuzzleManager.Instance != null && PuzzleManager.Instance.IsPuzzleCompleted(requiredPuzzleID))
                {
                    Unlock();
                }
                else
                {
                    PlayLockedSound();
                    return;
                }
            }
            else
            {
                PlayLockedSound();
                return;
            }
        }

        // Toggle open/close
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Unlock()
    {
        isLocked = false;
        // Optionally consume key here if you want:
        // var inv = FindAnyObjectByType<PlayerInventory>();
        // inv?.RemoveKey(requiredKey);
    }

    public void Open()
    {
        if (isOpen || isMoving) return;

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
        }

        StartCoroutine(MoveDoor(true));
    }

    public void Close()
    {
        if (!isOpen || isMoving) return;

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        StartCoroutine(MoveDoor(false));
    }

    private IEnumerator MoveDoor(bool opening)
    {
        isMoving = true;

        if (doorAudio != null)
        {
            doorAudio.PlayOneShot(opening ? openSound : closeSound);
        }

        float t = 0f;

        Vector3 startPos = doorTransform.localPosition;
        Quaternion startRot = doorTransform.localRotation;

        Vector3 targetPos = opening ? (closedPosition + openPosition) : closedPosition;
        Quaternion targetRot = opening ? (closedRotation * Quaternion.Euler(openRotation)) : closedRotation;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;

            if (slideOpen)
            {
                doorTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            }
            else
            {
                doorTransform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            }

            yield return null;
        }

        isOpen = opening;
        isMoving = false;

        if (autoClose && isOpen)
        {
            autoCloseCoroutine = StartCoroutine(AutoCloseRoutine());
        }
    }

    private IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        Close();
    }

    private void PlayLockedSound()
    {
        if (doorAudio != null && lockedSound != null)
        {
            doorAudio.PlayOneShot(lockedSound);
        }
    }
}
