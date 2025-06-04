using DG.Tweening;
using UnityEngine;

public class FurnitureData : MonoBehaviour
{

    public GameObject rotationUI;
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
     private void Start()
    {
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

        if (scrollDelta != 0)
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
            HandleDragMovement();

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
