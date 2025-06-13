using DG.Tweening;
using Sirenix.OdinInspector;
using Slicer;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(SlicerController))]
public class FurnitureData : MonoBehaviour
{

    public GameObject rotationUI;
    public GameObject movementUI;
    public GameObject furnitureUI;
    Vector3 startMousePos;
    public bool shouldUpdateRotation;
    public bool shouldUpdateMovement;
    public float rotationSpeed = 12.5f;

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

    [HorizontalGroup("ObjectDimensions")]
    public int objectDimensionsX;
    [HorizontalGroup("ObjectDimensions")]
    public int objectDimensionsY;
    [HorizontalGroup("ObjectDimensions")]
    public int objectDimensionsZ;
    public Renderer rendererer;

    public TMPro.TMP_InputField inputFieldX,inputFieldY,inputFieldZ;
    FurnitureDataInputHandler inputHandler;
    public SlicerController slicerController;
    private void Start()
    {
        cam = Camera.main;
        Debug.Log("Im alive");
        slicerController = GetComponent<SlicerController>();
        CheckSize();
        if(inputFieldX)
        {
            inputFieldX.text= ((int)objectDimensionsX).ToString();
            inputFieldX.onValueChanged.AddListener(value => CalculateScale());
            
        }
        if (inputFieldY)
        {
            inputFieldY.text = ((int)objectDimensionsY).ToString();
            inputFieldY.onValueChanged.AddListener(value => CalculateScale());

        }
        if (inputFieldZ)
        {
            inputFieldZ.text = ((int)objectDimensionsZ).ToString();
            inputFieldZ.onValueChanged.AddListener(value => CalculateScale());
        }
        if (!rendererer)
            rendererer = GetComponentInChildren<Renderer>();
        if(rendererer)
        {
            inputHandler = rendererer.GetComponent<FurnitureDataInputHandler>();
            if(!inputHandler)
            {
                inputHandler = rendererer.gameObject.AddComponent<FurnitureDataInputHandler>();
                inputHandler.setup(this);
            }
        }

    }
    [Button]
    public void CheckSize()
    {
        if (!rendererer)
            rendererer = GetComponentInChildren<Renderer>();
        if (rendererer)
        {
            objectDimensionsX = Mathf.RoundToInt(rendererer.bounds.size.x * 100f);
            objectDimensionsY = Mathf.RoundToInt(rendererer.bounds.size.y * 100f);
            objectDimensionsZ = Mathf.RoundToInt(rendererer.bounds.size.z * 100f);
            if (inputFieldX)
                inputFieldX.text = objectDimensionsX.ToString();
            if (inputFieldY)
                inputFieldY.text = objectDimensionsY.ToString();
            if (inputFieldZ)
                inputFieldZ.text = objectDimensionsZ.ToString();
        }
             

    }
    public void CalculateScale()
    {
        int x = -1;
        int.TryParse(inputFieldX.text, out x);
        int y = -1;
        int.TryParse(inputFieldY.text, out y);
        int z = -1;
        int.TryParse(inputFieldZ.text, out z);

        Vector3 newSize = new Vector3(x, y, z);
        const float EPS = 1e-5f;   // small positive number to avoid /0

        Vector3 result= new Vector3
        (
             objectDimensionsX > EPS ? newSize.x / objectDimensionsX : 1f,
             objectDimensionsY > EPS ? newSize.y / objectDimensionsY : 1f,
             objectDimensionsZ > EPS ? newSize.z / objectDimensionsZ : 1f
        );

        if(slicerController)
        {
            slicerController.Size=result;
            slicerController.RefreshSlice();
        }


    }
    [Button]
    public Vector3 CalculateScale(Vector3 newSize)
    {

        
        const float EPS = 1e-5f;   // small positive number to avoid /0

        return new Vector3
        (
              objectDimensionsX > EPS ? newSize.x / objectDimensionsX : 1f,
             objectDimensionsY > EPS ? newSize.y / objectDimensionsY : 1f,
             objectDimensionsZ > EPS ? newSize.z / objectDimensionsZ : 1f
        );

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

    public void OnMouseDown()
    {
        isMouseDown = true;
        startMousePos = Input.mousePosition;

    }
    public void OnMouseUp()
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
        CheckSize();
        if (furnitureUI && !furnitureUI.gameObject.activeSelf)
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
