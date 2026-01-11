using UnityEngine;

public class TestMovement : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("NO RIGIDBODY!");
        }
        else
        {
            Debug.Log("Rigidbody found! Is Kinematic: " + rb.isKinematic);
        }
    }

    void Update()
    {
        // Mișcare simplă înainte constant
        if (rb != null)
        {
            rb.linearVelocity = new Vector3(2f, rb.linearVelocity.y, 0);
            Debug.Log("Setting velocity! Current velocity: " + rb.linearVelocity);
        }
    }
}