using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class FurnitureItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private FurnitureGridManager gridManager;
    private Vector3 targetPosition;
    private Vector3 dragOffset;
    private bool isDragging = false;
    private bool canDrag = false;

    [Header("Drag Settings")]
    public float longPressDuration = 0.4f;

    void Start()
    {
        gridManager = FindObjectOfType<FurnitureGridManager>();
        targetPosition = transform.position;
    }

    void Update()
    {
        if (!isDragging)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
        }
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        StartCoroutine(LongPressCheck(eventData));
    }

    IEnumerator LongPressCheck(PointerEventData eventData)
    {
        float timer = 0f;
        while (timer < longPressDuration)
        {
            if (!Input.GetMouseButton(0))
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        canDrag = true;
        isDragging = true;
        dragOffset = transform.position - GetMouseWorldPosition();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag) return;

        Vector3 pos = GetMouseWorldPosition() + dragOffset;
        pos.y = 0f; // lock to ground
        transform.position = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        StopAllCoroutines();

        if (canDrag)
        {
            isDragging = false;
            canDrag = false;
            gridManager.SnapToNearestCell(this);
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return Vector3.zero;
    }
}
