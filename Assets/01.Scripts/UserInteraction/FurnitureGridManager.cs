using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FurnitureGridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2Int gridSize = new Vector2Int(10, 10);
    public float cellSize = 1.5f;
    public float cellMargin = 0.1f;              // extra padding around each piece
    public List<FurnitureItem> items = new List<FurnitureItem>();

    [Header("Collision Check")]
    public LayerMask furnitureLayer;                    // layer for collider tests

    [Header("Debug Draw")]
    public bool debugDraw = true;
    public Color gridColor = Color.green;

    // internal
    private bool[,] occupied;
    private Dictionary<FurnitureItem, Vector2Int> origins = new Dictionary<FurnitureItem, Vector2Int>();
    private Vector3 originOffset;

    // Called whenever you change values in the Inspector (or when the script loads in edit mode)
    void OnValidate()
    {
        // Recompute offset so (0,0) is in the middle of the grid
        float halfW = (gridSize.x - 1) * 0.5f * cellSize;
        float halfH = (gridSize.y - 1) * 0.5f * cellSize;
        originOffset = new Vector3(-halfW, 0f, -halfH);
    }

    void Start()
    {
        InitializeGrid();
    }

    void InitializeGrid()
    {
        occupied = new bool[gridSize.x, gridSize.y];
        origins.Clear();
        foreach (var item in items)
            SnapToNearestCell(item, initialize: true);
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 local = worldPos - (transform.position + originOffset);
        int gx = Mathf.RoundToInt(local.x / cellSize);
        int gy = Mathf.RoundToInt(local.z / cellSize);
        return new Vector2Int(gx, gy);
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return transform.position
             + originOffset
             + new Vector3(gridPos.x * cellSize, 0f, gridPos.y * cellSize);
    }

    Vector2Int GetFootprint(FurnitureItem item)
    {
        Collider col = item.GetComponent<Collider>();
        Vector3 size = col.bounds.size + Vector3.one * cellMargin;
        int w = Mathf.CeilToInt(size.x / cellSize);
        int h = Mathf.CeilToInt(size.z / cellSize);
        return new Vector2Int(w, h);
    }

    bool CanFit(Vector2Int origin, Vector2Int footprint)
    {
        for (int x = 0; x < footprint.x; x++)
            for (int y = 0; y < footprint.y; y++)
            {
                int gx = origin.x + x, gy = origin.y + y;
                if (gx < 0 || gy < 0 || gx >= gridSize.x || gy >= gridSize.y) return false;
                if (occupied[gx, gy]) return false;
            }
        return true;
    }

    bool IsSpaceClear(FurnitureItem item, Vector3 worldCenter)
    {
        Collider col = item.GetComponent<Collider>();
        Vector3 halfExt = col.bounds.extents;
        Quaternion rot = item.transform.rotation;
        var hits = Physics.OverlapBox(worldCenter, halfExt, rot, furnitureLayer);
        foreach (var h in hits)
            if (h.gameObject != item.gameObject)
                return false;
        return true;
    }

    void SetOccupied(Vector2Int origin, Vector2Int footprint, bool value)
    {
        for (int x = 0; x < footprint.x; x++)
            for (int y = 0; y < footprint.y; y++)
                occupied[origin.x + x, origin.y + y] = value;
    }

    /// <summary>
    /// Snaps an item into the nearest valid block of grid cells.
    /// Swaps if the target cell is occupied, and checks actual collider space.
    /// </summary>
    public void SnapToNearestCell(FurnitureItem item, bool initialize = false)
    {
        // 1) Clear old occupancy
        Vector2Int prev;
        if (!initialize && origins.TryGetValue(item, out prev))
            SetOccupied(prev, GetFootprint(item), false);
        else
            prev = WorldToGrid(item.transform.position);

        // 2) Desired + footprint
        Vector2Int desired = WorldToGrid(item.transform.position);
        Vector2Int footprint = GetFootprint(item);

        // clamp inside bounds
        desired.x = Mathf.Clamp(desired.x, 0, gridSize.x - footprint.x);
        desired.y = Mathf.Clamp(desired.y, 0, gridSize.y - footprint.y);

        // 3) Search for first valid spot
        Vector2Int best = desired;
        int maxRadius = Mathf.Max(gridSize.x, gridSize.y);
        bool found = false;

        for (int r = 0; r <= maxRadius && !found; r++)
        {
            for (int dx = -r; dx <= r && !found; dx++)
                for (int dy = -r; dy <= r && !found; dy++)
                {
                    Vector2Int test = desired + new Vector2Int(dx, dy);
                    if (!CanFit(test, footprint)) continue;

                    Vector3 worldPos = GridToWorld(test)
                        + new Vector3((footprint.x - 1) * cellSize * 0.5f,
                                      0f,
                                      (footprint.y - 1) * cellSize * 0.5f);

                    if (!IsSpaceClear(item, worldPos)) continue;

                    best = test;
                    found = true;
                }
        }

        // 4) If occupied, swap
        FurnitureItem occupant = null;
        foreach (var kv in origins)
        {
            if (kv.Key != item && kv.Value == best)
            {
                occupant = kv.Key;
                break;
            }
        }
        if (occupant != null)
        {
            Vector2Int occFoot = GetFootprint(occupant);
            SetOccupied(best, occFoot, false);
            SetOccupied(prev, occFoot, true);
            origins[occupant] = prev;

            Vector3 occWorld = GridToWorld(prev)
                             + new Vector3((occFoot.x - 1) * cellSize * 0.5f,
                                           0f,
                                           (occFoot.y - 1) * cellSize * 0.5f);
            occupant.SetTargetPosition(occWorld);
        }

        // 5) Occupy & move this item
        SetOccupied(best, footprint, true);
        origins[item] = best;

        Vector3 finalPos = GridToWorld(best)
                         + new Vector3((footprint.x - 1) * cellSize * 0.5f,
                                       0f,
                                       (footprint.y - 1) * cellSize * 0.5f);
        item.SetTargetPosition(finalPos);
    }

    void OnDrawGizmos()
    {
        if (!debugDraw) return;
        Gizmos.color = gridColor;

        // Always recalc offset in editor just in case
        float halfW = (gridSize.x - 1) * 0.5f * cellSize;
        float halfH = (gridSize.y - 1) * 0.5f * cellSize;
        Vector3 drawOffset = new Vector3(-halfW, 0, -halfH);

        for (int x = 0; x < gridSize.x; x++)
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 center = transform.position
                               + drawOffset
                               + new Vector3(x * cellSize, 0f, y * cellSize);
                Gizmos.DrawWireCube(center, new Vector3(cellSize, 0.01f, cellSize));
            }
    }
}
