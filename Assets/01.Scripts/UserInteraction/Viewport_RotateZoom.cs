using UnityEngine;

public class Viewport_RotateZoom : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 0.2f;
    public float minY = -180f;
    public float maxY = 180f;
    public float minX = -10f;
    public float maxX = 10f;

    [Header("Zoom Settings (Percent-based)")]
    public Camera orthoCamera; // assign your isometric RenderTexture camera
    public float zoomSpeed = 0.5f;
    public float minZoomPercent = 100f;
    public float maxZoomPercent = 200f;

    private float baseOrthoSize;
    private float currentZoomPercent = 100f;

    private Vector2 lastTouchPos;
    private bool isDragging;

    void Start()
    {
        if (orthoCamera != null)
            baseOrthoSize = orthoCamera.orthographicSize;
    }

    void Update()
    {
#if UNITY_EDITOR
        // Rotate with mouse
        if (Input.GetMouseButtonDown(0)) {
            isDragging = true;
            lastTouchPos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0)) {
            isDragging = false;
        }
        if (isDragging) {
            Vector2 delta = (Vector2)Input.mousePosition - lastTouchPos;
            ApplyRotation(delta);
            lastTouchPos = Input.mousePosition;
        }

        // Zoom with mouse scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f) {
            Zoom(-scroll * 100f);
        }

#else
        // Rotate with one finger drag
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                ApplyRotation(touch.deltaPosition);
            }
        }

        // Zoom with pinch
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float prevDist = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
            float currDist = (t0.position - t1.position).magnitude;
            float delta = currDist - prevDist;

            Zoom(delta);
        }
#endif
    }

    void ApplyRotation(Vector2 delta)
    {
        Vector3 euler = transform.rotation.eulerAngles;

        float newY = NormalizeAngle(euler.y - delta.x * rotationSpeed);
        float newX = NormalizeAngle(euler.x + delta.y * rotationSpeed);

        newY = Mathf.Clamp(newY, minY, maxY);
        newX = Mathf.Clamp(newX, minX, maxX);

        transform.rotation = Quaternion.Euler(newX, newY, 0f);
    }

    void Zoom(float delta)
    {
        currentZoomPercent = Mathf.Clamp(currentZoomPercent + delta * zoomSpeed, minZoomPercent, 50f);
        if (orthoCamera != null)
            orthoCamera.orthographicSize = baseOrthoSize * (currentZoomPercent / 100f);
    }

    float NormalizeAngle(float angle)
    {
        return (angle > 180f) ? angle - 360f : angle;
    }
}
