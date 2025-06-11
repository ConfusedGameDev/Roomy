using DG.Tweening;
using UnityEngine;

public class FurnitureData : MonoBehaviour
{

    public GameObject rotationUI;
    public GameObject movementUI;
    public GameObject furnitureUI;
    Vector3 startMousePos;
    public bool shouldUpdateRotation;
    public bool shouldUpdateMovement;
    public float rotationSpeed=12.5f;

    public int amountofClicks = 0;
    public bool enableInteraction;
    float accumulatedRotation;

    public float clickDuration;
    bool isMouseDown;
    public Vector3 mouseDelta;

    public float dragMoveSensitivity = 0.1f;


    [Tooltip("Select only the floor layer(s) here.")]
    public LayerMask floorLayerMask;

    private Camera cam;
    private bool isDragging = false;
    private Vector3 offset; // objectPos - initialHitPoint
    private void Start()
    {
        cam = Camera.main;
        Debug.Log("Im alive");
    }
    public void OnMouseEnter()
    {
        if (!enableInteraction) return;
        rotationUI.SetActive(true);
        startMousePos = Input.mousePosition;
        shouldUpdateRotation = true;
    }
    public void OnMouseExit()
    {
        if (!enableInteraction) return;

        rotationUI.SetActive(false);
        shouldUpdateRotation=false;
    }
    public void OnMouseOver()
    {
        if (!enableInteraction) return;

        if (!shouldUpdateMovement) return;
       
    }

    private void OnMouseDown()
    {
        isMouseDown = true;
        startMousePos = Input.mousePosition;

    }
    private void OnMouseUp()
    {
        isMouseDown = false;
        if(clickDuration<0.2f)
        {
            onTap();
        }
        clickDuration = 0;
    }

    public void onTap()
    {
        if(furnitureUI && !furnitureUI.gameObject.activeSelf)
        {
            shouldUpdateRotation = false;
            var initialScale = furnitureUI.transform.localScale;
            var finalY= initialScale.y;
            initialScale.y = 0;
            furnitureUI.transform.localScale = initialScale;
            furnitureUI.transform.DOScaleY(finalY, 1f);
            furnitureUI.SetActive(true );
        }

    }
    public void Update()
    {
        if (!enableInteraction) return;

        if (isMouseDown)
            clickDuration += Time.deltaTime;
        float scrollDelta = Input.mouseScrollDelta.y;

        if (shouldUpdateRotation && scrollDelta != 0)
        {
            // Accumulate rotation in steps
            accumulatedRotation += scrollDelta;
            int steps = Mathf.FloorToInt(accumulatedRotation);

            if (steps != 0)
            {
                transform.Rotate(0, steps * rotationSpeed, 0);
                accumulatedRotation -= steps; // Keep only the fractional part
            }
        }

        if (isMouseDown && clickDuration > 0.35f)
        {

            if (!isDragging)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, floorLayerMask))
                {
                    Debug.Log("Hit floor at: " + hitInfo.point);
                    // Compute and store the offset between object and where we hit the floor
                    offset = transform.position - hitInfo.point;
                    isDragging = true;
                }
            }
            else if (isDragging)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, floorLayerMask))
                {
                    Debug.Log("Dragging object to: " + hitInfo.point);
                    // Place object so it stays under the cursor (including original offset)
                    transform.position = hitInfo.point ;
                }
            }
            if(rotationUI)
                rotationUI.SetActive(false);
            if(movementUI)
            movementUI.SetActive(isDragging);
        }
        else
        {
            isDragging = false; // Reset dragging state if not holding long enough
            if(movementUI)
            movementUI.SetActive(false);
        }

    }

    private void HandleDragMovement()
    {
        if (isMouseDown)
        {
            Vector3 currentMousePos = Input.mousePosition;
            Vector3 delta = currentMousePos - startMousePos;

            // Optional: clamp small jitter
            if (delta.magnitude > 1f)
            {
                // Translate screen-space delta to world-space movement
                Vector3 moveDir = new Vector3(delta.x, 0, delta.y) * dragMoveSensitivity;

                // Move relative to object rotation
                Vector3 worldMove = Quaternion.Euler(0, transform.eulerAngles.y, 0) * moveDir;
                transform.position += worldMove;

                startMousePos = currentMousePos; // Update start to allow continuous drag
            }
        }
    }

}
