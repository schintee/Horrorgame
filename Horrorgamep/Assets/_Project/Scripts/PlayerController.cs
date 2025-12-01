using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;        // Viteza de mișcare
    [SerializeField] private float sprintMultiplier = 1.5f; // Cât de rapid alergi (Shift)

    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;  // Sensibilitate mouse
    [SerializeField] private Transform cameraTransform;    // Referință la camera player-ului

    private Rigidbody rb;
    private float verticalRotation = 0f;  // Pentru rotație camera sus/jos

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Ascunde și blochează cursorul în mijlocul ecranului
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Dacă nu ai setat camera în Inspector, o găsim automat
        if (cameraTransform == null)
        {
            cameraTransform = GetComponentInChildren<Camera>().transform;
        }
    }

    void Update()
    {
        // MIȘCARE WASD
        HandleMovement();

        // ROTAȚIE MOUSE
        HandleMouseLook();

        // ESC pentru a debloca cursorul (debug)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMovement()
    {
        // Input WASD (Horizontal = A/D, Vertical = W/S)
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");     // W/S

        // Direcția de mișcare relativă la unde ne uităm
        Vector3 direction = transform.right * horizontal + transform.forward * vertical;
        direction.Normalize(); // Normalizăm ca să nu mergem mai rapid diagonal

        // Verificăm dacă ținem Shift pentru sprint
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= sprintMultiplier;
        }

        // Mișcăm player-ul (păstrăm velocity-ul pe Y pentru gravity)
        Vector3 velocity = direction * currentSpeed;
        velocity.y = rb.linearVelocity.y; // Păstrăm gravity
        rb.linearVelocity = velocity;
    }

    void HandleMouseLook()
    {
        // Input mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // ROTAȚIE ORIZONTALĂ (stânga/dreapta) - rotăm întreg player-ul
        transform.Rotate(Vector3.up * mouseX);

        // ROTAȚIE VERTICALĂ (sus/jos) - rotăm doar camera
        verticalRotation -= mouseY; // Minus ca să fie natural (mouse sus = privire sus)
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // Limităm ca să nu dai flip

        cameraTransform.localEulerAngles = new Vector3(verticalRotation, 0f, 0f);
    }
}
