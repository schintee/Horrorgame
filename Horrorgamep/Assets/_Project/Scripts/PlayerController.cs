using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float lookXLimit = 85f;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 15f;

    [Header("Audio")]
    [SerializeField] private AudioSource footstepAudio;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepInterval = 0.5f;

    // Components
    private CharacterController controller;
    private Camera playerCamera;

    // State
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private float currentStamina;
    private bool canMove = true;
    private float footstepTimer = 0f;

    // Public properties
    public bool IsMoving { get; private set; }
    public bool IsSprinting { get; private set; }
    public float StaminaPercent => currentStamina / maxStamina;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        currentStamina = maxStamina;

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!canMove) return;

        HandleMovement();
        HandleLook();
        HandleStamina();
        HandleFootsteps();
    }

    private void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Check if sprinting
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0;
        IsSprinting = wantsToSprint && vertical > 0; // Can only sprint forward

        float currentSpeed = IsSprinting ? sprintSpeed : walkSpeed;
        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * vertical + right * horizontal) * currentSpeed;

        // Check if moving
        IsMoving = horizontal != 0 || vertical != 0;

        // Jump
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            moveDirection.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity
        if (!controller.isGrounded)
        {
            moveDirection.y += gravity * Time.deltaTime;
        }

        // Move
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void HandleLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * mouseSensitivity, 0);
    }

    private void HandleStamina()
    {
        if (IsSprinting && IsMoving)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0);
        }
        else if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }
    }

    private void HandleFootsteps()
    {
        if (!controller.isGrounded || !IsMoving || footstepAudio == null || footstepSounds.Length == 0)
            return;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0)
        {
            // Play random footstep sound
            int randomIndex = Random.Range(0, footstepSounds.Length);
            footstepAudio.clip = footstepSounds[randomIndex];
            footstepAudio.pitch = Random.Range(0.9f, 1.1f);
            footstepAudio.Play();

            // Reset timer based on speed
            float interval = IsSprinting ? footstepInterval * 0.6f : footstepInterval;
            footstepTimer = interval;
        }
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!canMove)
        {
            moveDirection = Vector3.zero;
            IsSprinting = false;
            IsMoving = false;
        }
    }

    public void AddStamina(float amount)
    {
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
    }
}