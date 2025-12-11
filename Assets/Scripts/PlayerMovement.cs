using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Transform _transform;

    public PhotonView _pv;

    InputManager inputManager;

    Vector3 moveDirection;

    private Transform cameraObject;

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

    private void Awake()
    {
        _transform = transform;
        inputManager = GetComponent<InputManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        _gm = GameObject.Find("GameSceneManager").GetComponent<GameSceneManager>();
        _transform = this.transform;
        stamina = 100;

        if (!_pv.IsMine)
        {
            Destroy(this);
        }

        if (Camera.main != null)
            cameraObject = Camera.main.transform;
        else
            cameraObject = null;
    }

    private void Update()
    {
        if (_pv.IsMine)
        {
            HandleAllMovement();
        }
    }

    public void HandleAllMovement()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        if (cameraObject == null)
            return;

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
        Camera cam = cameraObject != null ? cameraObject.GetComponent<Camera>() ?? Camera.main : Camera.main;
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
                // Compute camera forward projected on XZ (reference direction)
                Vector3 camForward = (cameraObject != null) ? cameraObject.forward : cam.transform.forward;
                camForward.y = 0f;
                camForward.Normalize();

                // Angle from camera forward to the desired direction
                float signedAngle = Vector3.SignedAngle(camForward, desiredDirection.normalized, Vector3.up);

                // Clamp angle so character cannot look further than maxViewAngle away from camera forward
                float clampedAngle = Mathf.Clamp(signedAngle, -maxViewAngle, maxViewAngle);

                // Build the clamped direction by rotating camera forward by the clamped angle
                Vector3 clampedDirection = Quaternion.AngleAxis(clampedAngle, Vector3.up) * camForward;

                // Smoothly rotate toward the clamped direction
                Quaternion targetRotation = Quaternion.LookRotation(clampedDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                return;
            }
        }

        // Fallback: rotate toward movement direction when ray doesn't hit
        if (cameraObject == null)
            return;

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
