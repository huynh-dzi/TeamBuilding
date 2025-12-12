using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private Transform _transform;

    public PhotonView _pv;

    InputManager inputManager;

    Vector3 moveDirection;

    // cameraObject is assigned by PlayerManager for the local player
    [HideInInspector] public Transform cameraObject;

    Rigidbody playerRigidbody;
    Collider playerCollider;

    public float movementSpeed;
    public float rotationSpeed;
    public int stamina;

    GameSceneManager _gm;

    [Header("View Limits")]
    [SerializeField]
    [Tooltip("Maximum angle (degrees) the character can look away from the camera forward direction.")]
    private float maxViewAngle = 80f;

    // log once if input missing
    private bool inputMissingLogged = false;

    private void Awake()
    {
        // Ensure we have a PhotonView reference (don't rely on inspector)
        if (_pv == null)
            _pv = GetComponent<PhotonView>();

        if (_pv == null)
        {
            Debug.LogError("PlayerMovement requires a PhotonView on the same GameObject.");
            enabled = false;
            return;
        }

        // Disable this component for remote instances (PlayerManager will not call it)
        if (!_pv.IsMine)
        {
            enabled = false;
            return;
        }

        // Local-only initialization
        _transform = transform;
        inputManager = GetComponent<InputManager>();

        if (inputManager == null)
        {
            Debug.LogError($"PlayerMovement on '{name}': InputManager not found on player prefab. Attach InputManager to the player prefab.");
            enabled = false;
            return;
        }

        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        var gmObj = GameObject.Find("GameSceneManager");
        if (gmObj != null)
            _gm = gmObj.GetComponent<GameSceneManager>();
        stamina = 100;
    }

    // Called by PlayerManager to inject the correct camera for the local player
    public void SetCamera(Transform cam)
    {
        cameraObject = cam;
    }

    private void Update()
    {
        // PlayerManager calls HandleAllMovement in FixedUpdate; keep Update light.
    }

    public void HandleAllMovement()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        // Defensive checks to avoid NullReferenceException
        if (cameraObject == null || inputManager == null || playerRigidbody == null)
        {
            if (!inputMissingLogged)
            {
                Debug.LogWarning($"PlayerMovement: missing component(s). cameraObject={(cameraObject==null)}, inputManager={(inputManager==null)}, playerRigidbody={(playerRigidbody==null)} on {name}");
                inputMissingLogged = true;
            }
            return;
        }

        moveDirection = cameraObject.forward * inputManager.verticalInput;
        moveDirection = moveDirection + cameraObject.right * inputManager.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;
        moveDirection = moveDirection * movementSpeed;

        Vector3 movementVelocity = moveDirection;
        playerRigidbody.linearVelocity = movementVelocity;
    }

    private void HandleRotation()
    {
        if (cameraObject == null || inputManager == null)
            return;

        Camera cam = cameraObject.GetComponent<Camera>() ?? Camera.main;
        if (cam == null)
            return;

        // Ray from camera through mouse position
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Horizontal plane at player's y to compute target point on ground plane
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 desiredDirection = hitPoint - transform.position;
            desiredDirection.y = 0f;

            if (desiredDirection.sqrMagnitude > 0.0001f)
            {
                Vector3 camForward = cameraObject.forward;
                camForward.y = 0f;
                camForward.Normalize();

                float signedAngle = Vector3.SignedAngle(camForward, desiredDirection.normalized, Vector3.up);

                float clampedAngle = Mathf.Clamp(signedAngle, -maxViewAngle, maxViewAngle);

                Vector3 clampedDirection = Quaternion.AngleAxis(clampedAngle, Vector3.up) * camForward;

                Quaternion targetRotation = Quaternion.LookRotation(clampedDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                return;
            }
        }

        Vector3 targetDirection = cameraObject.forward * inputManager.verticalInput;
        targetDirection = targetDirection + cameraObject.right * inputManager.horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;

        if (targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        Quaternion fallbackRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, fallbackRotation, rotationSpeed * Time.deltaTime);
    }
}
