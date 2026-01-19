using System.Collections;
using UnityEngine;

public class SimpleMessageUI : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private float showTime = 1.5f;

    private Coroutine routine;

    private void Awake()
    {
        if (canvas != null)
            canvas.SetActive(false);
    }

    public void Show()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (canvas != null)
            canvas.SetActive(true);

        yield return new WaitForSeconds(showTime);

        if (canvas != null)
            canvas.SetActive(false);

        routine = null;
    }
}
