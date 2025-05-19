using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float speed = 1f;
    [SerializeField] private float movementTime = 0.1f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 15f;            // For Q/E or Mouse X
    [SerializeField] private float rotationSpeedTouch = 0.5f;       // Tweak for two-finger twist

    [Header("Zoom")]
    [SerializeField] private Vector3 zoomAmount = new Vector3(0, 0, 1f);
    [SerializeField] private Vector3 zoomLimitClose = new Vector3(0, 5, -5);
    [SerializeField] private Vector3 zoomLimitFar = new Vector3(0, 20, -20);

    [Header("Bounds")]
    [SerializeField] private int constraintXMax = 5, constraintXMin = -5;
    [SerializeField] private int constraintZMax = 5, constraintZMin = -5;

    [SerializeField] private CinemachineVirtualCamera cameraReference;
    private CinemachineTransposer cameraTransposer;

    private Vector3 newZoom;
    private Quaternion targetRotation;
    private Vector2 input;

    private void Start()
    {
        cameraTransposer = cameraReference.GetCinemachineComponent<CinemachineTransposer>();
        targetRotation = transform.rotation;
        newZoom = cameraTransposer.m_FollowOffset;
    }

    void Update()
    {
        HandleInput();
        ApplyMovement();
        ApplyRotation();
        ApplyZoom();
    }

    private void HandleInput()
    {
        // Movement input
        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // ----- PC: Right-mouse drag Y-rotation -----
        if (Input.GetMouseButton(1))
        {
            float mouseDeltaX = Input.GetAxis("Mouse X");
            targetRotation *= Quaternion.Euler(0f, mouseDeltaX * rotationSpeed * Time.deltaTime, 0f);
        }

        // ----- Mobile: Two-finger twist Y-rotation -----
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prevDir = (t1.position - t1.deltaPosition) - (t0.position - t0.deltaPosition);
            Vector2 currDir = t1.position - t0.position;

            float anglePrev = Mathf.Atan2(prevDir.y, prevDir.x) * Mathf.Rad2Deg;
            float angleCurr = Mathf.Atan2(currDir.y, currDir.x) * Mathf.Rad2Deg;
            float deltaAngle = Mathf.DeltaAngle(anglePrev, angleCurr);

            targetRotation *= Quaternion.Euler(0f, deltaAngle * rotationSpeedTouch, 0f);
        }

        // (Optional) Q/E keys fallback
        int rotDir = 0;
        if (Input.GetKey(KeyCode.Q)) rotDir = -1;
        if (Input.GetKey(KeyCode.E)) rotDir = 1;
        if (rotDir != 0)
            targetRotation *= Quaternion.Euler(0f, rotDir * rotationSpeed * Time.deltaTime, 0f);

        // Zoom wheel
        if (!Mathf.Approximately(Input.mouseScrollDelta.y, 0f))
        {
            int zoomDir = Input.mouseScrollDelta.y > 0f ? 1 : -1;
            newZoom += zoomAmount * zoomDir;
            newZoom = ClampVector(newZoom, zoomLimitClose, zoomLimitFar);
        }
    }

    private void ApplyMovement()
    {
        Vector3 move = (transform.forward * input.y + transform.right * input.x) * speed * Time.deltaTime;
        transform.position += move;

        // Clamp within XZ bounds
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, constraintXMin, constraintXMax),
            transform.position.y,
            Mathf.Clamp(transform.position.z, constraintZMin, constraintZMax)
        );
    }

    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime / movementTime);
    }

    private void ApplyZoom()
    {
        cameraTransposer.m_FollowOffset = Vector3.Lerp(
            cameraTransposer.m_FollowOffset,
            newZoom,
            Time.deltaTime / movementTime
        );
    }

    private Vector3 ClampVector(Vector3 value, Vector3 min, Vector3 max)
    {
        return new Vector3(
            Mathf.Clamp(value.x, min.x, max.x),
            Mathf.Clamp(value.y, min.y, max.y),
            Mathf.Clamp(value.z, min.z, max.z)
        );
    }
}
