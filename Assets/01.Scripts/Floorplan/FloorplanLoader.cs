using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class FloorplanLoader : MonoBehaviour
{
    [Header("Labelme JSON")]
    public TextAsset jsonFile;

    [Header("Wall Settings")]
    public float wallHeight = 2.5f;

    [Header("Materials")]
    public Material wallMaterial;
    public Material floorMaterial;

    void Start()
    {
        if (jsonFile == null)
        {
            Debug.LogError("No JSON file assigned.");
            return;
        }

        // Parse JSON
        var data = JsonConvert.DeserializeObject<LabelmeData>(jsonFile.text);

        // Collect all wall rectangles (2-point rectangle definition)
        List<List<List<float>>> wallRects = data.shapes
            .Where(s => s.label == "wall" && s.shape_type == "rectangle")
            .Select(s => s.points)
            .ToList();

        // Build wall meshes from rectangles
        GameObject walls = WallGeometryProcessor.BuildWallsFromRectangles(wallRects, wallHeight, wallMaterial);
        walls.transform.SetParent(transform);

        // Optional: Build floor if available
        var floorShape = data.shapes.FirstOrDefault(s => s.label == "area");
        if (floorShape != null)
        {
            var floorObj = BuildFloor(floorShape.points);
            floorObj.name = "Floor";
            floorObj.transform.SetParent(transform);
        }
    }

    GameObject BuildFloor(List<List<float>> points)
    {
        Vector2 p1 = new Vector2(points[0][0], points[0][1]);
        Vector2 p2 = new Vector2(points[1][0], points[1][1]);

        float xMin = Mathf.Min(p1.x, p2.x);
        float xMax = Mathf.Max(p1.x, p2.x);
        float yMin = Mathf.Min(p1.y, p2.y);
        float yMax = Mathf.Max(p1.y, p2.y);

        Vector3[] verts = new Vector3[]
        {
            new Vector3(xMin, 0, yMin),
            new Vector3(xMax, 0, yMin),
            new Vector3(xMax, 0, yMax),
            new Vector3(xMin, 0, yMax)
        };

        // ✅ Correct winding order to make normals point UP
        int[] tris = new int[] { 0, 2, 1, 0, 3, 2 };

        GameObject quad = new GameObject("Floor");
        var mf = quad.AddComponent<MeshFilter>();
        var mr = quad.AddComponent<MeshRenderer>();
        var mesh = new Mesh();

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        mr.material = floorMaterial != null ? floorMaterial : new Material(Shader.Find("Standard"));

        return quad;
    }

    // JSON data structure
    [System.Serializable]
    public class LabelmeData
    {
        public List<Shape> shapes;
    }

    [System.Serializable]
    public class Shape
    {
        public string label;
        public List<List<float>> points;
        public string shape_type;
    }
}