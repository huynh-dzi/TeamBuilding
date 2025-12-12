using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviourPunCallbacks
{
    private Transform _transform;
    public PhotonView _pv;

    InputManager inputManager;

    Vector3 moveDirection;

    [HideInInspector] public Transform cameraObject;

    Rigidbody playerRigidbody;
    Collider playerCollider;

    public float movementSpeed;
    public float rotationSpeed = 240f;
    private float pitch = 0f;

    GameSceneManager _gm;

    [Header("View Limits")]
    [SerializeField] private float maxViewAngle = 80f;

    private bool inputMissingLogged = false;

    [Header("Mouse Rotation")]
    public float mouseSensitivity = 2f; // inspector adjustable
    public float edgeSpeedMultiplier = 2f; // how much faster at screen edge
    public float deadZone = 0.05f; // ignore tiny deltas near center

    // Mouse delta tracking
    private Vector3 lastMousePos;
    private bool firstFrame = true;

    private void Awake()
    {
        if (_pv == null)
            _pv = GetComponent<PhotonView>();

        if (_pv == null)
        {
            Debug.LogError("PlayerMovement requires a PhotonView on the same GameObject.");
            enabled = false;
            return;
        }

        if (!_pv.IsMine)
        {
            enabled = false;
            return;
        }

        _transform = transform;
        inputManager = GetComponent<InputManager>();

        if (inputManager == null)
        {
            Debug.LogError($"PlayerMovement on '{name}': InputManager not found on player prefab.");
            enabled = false;
            return;
        }

        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();

        var gmObj = GameObject.Find("GameSceneManager");
        if (gmObj != null)
            _gm = gmObj.GetComponent<GameSceneManager>();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void SetCamera(Transform cam)
    {
        cameraObject = cam;
    }

    private void Update() { }

    public void HandleAllMovement()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        if (cameraObject == null || inputManager == null || playerRigidbody == null)
        {
            if (!inputMissingLogged)
            {
                Debug.LogWarning($"PlayerMovement: missing components on {name}");
                inputMissingLogged = true;
            }
            return;
        }

        moveDirection = cameraObject.forward * inputManager.verticalInput
                      + cameraObject.right * inputManager.horizontalInput;

        moveDirection.Normalize();
        moveDirection.y = 0;
        moveDirection *= movementSpeed;

        playerRigidbody.linearVelocity = moveDirection;
    }

    private void HandleRotation()
    {
        if (cameraObject == null || inputManager == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 mousePos = Input.mousePosition;
        Vector2 deltaFromCenter = mousePos - screenCenter;

        float normX = deltaFromCenter.x / (Screen.width / 2f);

        // Dead zone near center
        float deadZone = 0.05f;
        if (Mathf.Abs(normX) < deadZone) normX = 0f;

        normX = Mathf.Clamp(normX, -1f, 1f);

        // Distance from center for scaling speed
        float distance = Mathf.Clamp01(Mathf.Abs(normX));

        // Nonlinear scaling for faster rotation near edges
        float edgeMultiplier = 1f + distance * 3f; // tweak as needed

        float rotationX = normX * rotationSpeed * Time.deltaTime * mouseSensitivity * edgeMultiplier;

        // Horizontal rotation only
        transform.Rotate(Vector3.up * rotationX);
    }

}
