using UnityEngine;

// First-person controller. Attach to the Player capsule (which also has a CharacterController).
// The Main Camera must be a child of the Player and assigned to cameraTransform.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float jumpHeight = 1.6f;
    public float gravity = -20f;
    public float mouseSensitivity = 2f;
    public float fallLimit = -10f;   // below this Y value the player has fallen off the world

    private CharacterController controller;
    private float verticalVelocity = 0f;
    private float pitch = 0f;
    private Vector3 startPosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        startPosition = transform.position;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.GameEnded) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Look();
        Move();
        CheckFall();
    }

    // Mouse look: horizontal turns the whole body, vertical tilts only the camera.
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0f, mouseX, 0f);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Move()
    {
        // "Horizontal" and "Vertical" axes respond to both WASD and the arrow keys.
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        move = Vector3.ClampMagnitude(move, 1f);   // stops diagonal movement being faster

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Keep the player stuck to the ground when grounded.
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        // Jump: v = sqrt(h * -2 * g) gives exactly the requested jump height.
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private void CheckFall()
    {
        if (transform.position.y < fallLimit)
        {
            bool stillAlive = GameManager.Instance.PlayerFell();
            if (stillAlive)
            {
                Respawn();
            }
        }
    }

    private void Respawn()
    {
        // A CharacterController overrides position changes, so disable it while teleporting.
        controller.enabled = false;
        transform.position = startPosition;
        controller.enabled = true;
        verticalVelocity = 0f;
    }
}
