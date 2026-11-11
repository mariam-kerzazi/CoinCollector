using UnityEngine;
using UnityEngine.Rendering;

// Player controller with first-person and third-person camera modes (press V to switch).
// Attach to the Player capsule (which also has a CharacterController).
// The Main Camera stays a child of the Player and is assigned to cameraTransform.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public Transform cameraTransform;
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float jumpHeight = 1.6f;
    public float gravity = -20f;
    public float mouseSensitivity = 2f;
    public float fallLimit = -10f;   // below this Y value the player has fallen off the world

    [Header("Camera")]
    public bool startInFirstPerson = true;
    public float eyeHeight = 0.6f;              // first-person camera height above the capsule center
    public float thirdPersonDistance = 4f;      // how far behind the player the camera sits
    public float thirdPersonHeight = 0.9f;      // height of the point the camera orbits around

    private CharacterController controller;
    private Renderer bodyRenderer;
    private bool firstPerson;
    private float verticalVelocity = 0f;
    private float pitch = 0f;
    private Vector3 startPosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        bodyRenderer = GetComponent<Renderer>();
        startPosition = transform.position;

        firstPerson = startInFirstPerson;
        ApplyCameraMode();

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

        if (Input.GetKeyDown(KeyCode.V))
        {
            firstPerson = !firstPerson;
            ApplyCameraMode();
        }

        Look();
        Move();
        CheckFall();
    }

    // The camera is positioned after all movement for the frame, so it never lags behind the player.
    void LateUpdate()
    {
        UpdateCamera();
    }

    // Hide the capsule in first person (but keep its shadow), show it in third person.
    private void ApplyCameraMode()
    {
        if (bodyRenderer != null)
        {
            bodyRenderer.shadowCastingMode = firstPerson ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
        }
        pitch = Mathf.Clamp(pitch, MinPitch(), MaxPitch());
    }

    private float MinPitch() { return firstPerson ? -80f : -20f; }
    private float MaxPitch() { return firstPerson ? 80f : 70f; }

    // Mouse look: horizontal turns the whole body, vertical tilts the camera.
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0f, mouseX, 0f);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, MinPitch(), MaxPitch());
    }

    private void UpdateCamera()
    {
        Quaternion rot = transform.rotation * Quaternion.Euler(pitch, 0f, 0f);

        if (firstPerson)
        {
            cameraTransform.SetPositionAndRotation(transform.position + Vector3.up * eyeHeight, rot);
        }
        else
        {
            // Orbit behind the player. Cast a ray backwards so the camera doesn't go through walls or floors.
            Vector3 pivot = transform.position + Vector3.up * thirdPersonHeight;
            Vector3 backDir = rot * Vector3.back;
            float dist = thirdPersonDistance;

            RaycastHit hit;
            if (Physics.Raycast(pivot, backDir, out hit, thirdPersonDistance + 0.3f, ~0, QueryTriggerInteraction.Ignore))
            {
                dist = Mathf.Max(0.5f, hit.distance - 0.3f);
            }

            cameraTransform.SetPositionAndRotation(pivot + backDir * dist, rot);
        }
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