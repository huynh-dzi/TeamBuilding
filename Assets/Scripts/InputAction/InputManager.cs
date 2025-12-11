using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerControls playerControls;
    PlayerMovement playerMovement;

    public Vector2 movementInput;

    public float verticalInput;

    public float horizontalInput;

    private void Awake()
    {
        // Cache the PlayerMovement reference early to avoid null refs later
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();

        }

        playerControls.Enable();
    }

    private void OnDisable()
    {
        if (playerControls != null)
            playerControls.Disable();
    }

    public void HandleAllInputs()
    {
        HandleMovement();
    }
    private void HandleMovement()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;
    }

}
