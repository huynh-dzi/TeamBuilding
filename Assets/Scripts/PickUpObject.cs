using UnityEngine;

public class PickUpObject : MonoBehaviour
{
    [SerializeField] private float throwForce = 600f;
    [SerializeField] private float maxDistance = 3f;

    // How far in front of the camera the object should be held
    [SerializeField] private float holdDistance = 1.2f;
    [SerializeField] private float holdHeight = -0.2f;
    [SerializeField] private float followSmoothTime = 0.06f;
    [SerializeField] private float collisionSphereRadius = 0.2f;

    TempParent tempParent;
    Rigidbody rb;
    Collider col;

    bool isHolding = false;
    Vector3 currentVelocity;

    void Start()
    {
        tempParent = TempParent.instance;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (rb == null)
            Debug.LogError($"PickUpObject: no Rigidbody found on {name}");
        if (col == null)
            Debug.LogError($"PickUpObject: no Collider found on {name} (required for picking)");
        if (tempParent == null)
            Debug.LogWarning("PickUpObject: TempParent.instance is null. Create an object with TempParent in the scene.");
    }

    void Update()
    {
        // Pick up on left mouse button down using a raycast
        if (!isHolding && Input.GetMouseButtonDown(0))
        {
            TryPickupUnderCursor();
        }

        // Release on left mouse up
        if (isHolding && Input.GetMouseButtonUp(0))
        {
            Release();
        }

        if (isHolding)
        {
            // Keep object in front of camera each frame
            UpdateHeldPosition();

            // Throw with right click
            if (Input.GetMouseButtonDown(1))
                Throw();
        }
    }

    private void TryPickupUnderCursor()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            if (hit.collider != col)
                return; // clicked something else

            DoPickup();
        }
    }

    private void DoPickup()
    {
        if (rb == null || col == null) return;

        isHolding = true;

        // Use kinematic while held to avoid physics fighting position updates
        rb.isKinematic = true;
        rb.useGravity = false;

        // IMPORTANT: disable collisions while held so the object won't push the player
        rb.detectCollisions = false;
        col.enabled = false;

        // Unparent — we'll position relative to the camera every frame so it never leaves view
        transform.SetParent(null);

        // stop residual motion
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log($"PickUpObject: picked up {name}");
    }

    private void UpdateHeldPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Desired point in front of camera
        Vector3 desired = cam.transform.position + cam.transform.forward * holdDistance + cam.transform.up * holdHeight;

        // Prevent placing the object inside geometry: do a spherecast from camera towards desired
        Vector3 toDesired = desired - cam.transform.position;
        float dist = toDesired.magnitude;
        Vector3 dir = toDesired.normalized;

        if (Physics.SphereCast(cam.transform.position, collisionSphereRadius, dir, out RaycastHit hit, dist))
        {
            // place the object slightly before the hit point
            desired = cam.transform.position + dir * Mathf.Max(0.1f, hit.distance - collisionSphereRadius);
        }

        // Smoothly move to the desired position
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref currentVelocity, followSmoothTime);

        // Orient the object to face camera (or keep world rotation if you prefer)
        Quaternion targetRot = Quaternion.LookRotation((cam.transform.position - transform.position).normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 25f * Time.deltaTime);
    }

    private void Release()
    {
        if (!isHolding) return;
        isHolding = false;

        // Restore physics
        rb.isKinematic = false;
        rb.useGravity = true;

        // Re-enable collisions
        rb.detectCollisions = true;
        col.enabled = true;

        // keep velocity zero so it drops in place
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log($"PickUpObject: released {name}");
    }

    private void Throw()
    {
        if (!isHolding) return;
        isHolding = false;

        // Restore physics and re-enable collisions before applying force
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.detectCollisions = true;
        col.enabled = true;

        Camera cam = Camera.main;
        Vector3 dir = (cam != null) ? cam.transform.forward : transform.forward;

        // use impulse so force is immediate and independent of mass timestep
        rb.AddForce(dir.normalized * throwForce, ForceMode.Impulse);

        Debug.Log($"PickUpObject: threw {name}");
    }
}
