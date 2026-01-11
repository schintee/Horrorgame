using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        // Dacă enemy e în trigger, îl lasă liber
        if (other.CompareTag("Enemy"))
        {
            // Nu face nimic - enemy trece prin
        }
    }
}