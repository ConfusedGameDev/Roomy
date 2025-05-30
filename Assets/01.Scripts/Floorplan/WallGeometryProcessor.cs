using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class WallGeometryProcessor
{
    public static GameObject BuildWallsFromRectangles(List<List<List<float>>> rectangles, float height, Material wallMaterial)
    {
        var edges = new List<Edge>();
        var vertices = new HashSet<Vector2>();

        // 1. Rectangles → Edges
        foreach (var rect in rectangles)
        {
            Vector2 a = new Vector2(rect[0][0], rect[0][1]);
            Vector2 b = new Vector2(rect[1][0], rect[1][1]);

            float xMin = Mathf.Min(a.x, b.x);
            float xMax = Mathf.Max(a.x, b.x);
            float yMin = Mathf.Min(a.y, b.y);
            float yMax = Mathf.Max(a.y, b.y);

            Vector2[] corners = new Vector2[]
            {
                new Vector2(xMin, yMin),
                new Vector2(xMax, yMin),
                new Vector2(xMax, yMax),
                new Vector2(xMin, yMax)
            };

            for (int i = 0; i < 4; i++)
            {
                Edge e = new Edge(corners[i], corners[(i + 1) % 4]);
                edges.Add(e);
                vertices.Add(e.a);
                vertices.Add(e.b);
            }
        }

        // 2. Split edges at intersections
        var splitEdges = SplitEdgesAtIntersections(edges);

        // 3. Build closed loops
        var loops = FindClosedLoops(splitEdges);

        // 4. Triangulate & Extrude each loop into wall meshes
        GameObject root = new GameObject("Walls");
        foreach (var loop in loops)
        {
            if (loop.Count < 3) continue;
            var mesh = TriangulateAndExtrude(loop, height, wallMaterial);
            mesh.transform.parent = root.transform;
        }

        return root;
    }

    public struct Edge
    {
        public Vector2 a;
        public Vector2 b;
        public Edge(Vector2 a, Vector2 b) { this.a = a; this.b = b; }
    }

    private static List<Edge> SplitEdgesAtIntersections(List<Edge> edges)
    {
        List<Edge> result = new List<Edge>();

        for (int i = 0; i < edges.Count; i++)
        {
            var e1 = edges[i];
            List<Vector2> splitPoints = new() { e1.a, e1.b };

            for (int j = 0; j < edges.Count; j++)
            {
                if (i == j) continue;
                var e2 = edges[j];
                if (LineSegmentsIntersect(e1.a, e1.b, e2.a, e2.b, out Vector2 p))
                {
                    if (!splitPoints.Contains(p))
                        splitPoints.Add(p);
                }
            }

            splitPoints = splitPoints.OrderBy(p => Vector2.Distance(e1.a, p)).ToList();
            for (int k = 0; k < splitPoints.Count - 1; k++)
                result.Add(new Edge(splitPoints[k], splitPoints[k + 1]));
        }

        return result;
    }

    private static bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out Vector2 intersection)
    {
        intersection = Vector2.zero;
        float A1 = p2.y - p1.y;
        float B1 = p1.x - p2.x;
        float C1 = A1 * p1.x + B1 * p1.y;

        float A2 = q2.y - q1.y;
        float B2 = q1.x - q2.x;
        float C2 = A2 * q1.x + B2 * q1.y;

        float det = A1 * B2 - A2 * B1;
        if (Mathf.Abs(det) < 0.001f)
            return false;

        float x = (B2 * C1 - B1 * C2) / det;
        float y = (A1 * C2 - A2 * C1) / det;
        intersection = new Vector2(x, y);

        return IsBetween(p1, p2, intersection) && IsBetween(q1, q2, intersection);
    }

    private static bool IsBetween(Vector2 a, Vector2 b, Vector2 p)
    {
        return Mathf.Min(a.x, b.x) - 0.01f <= p.x && p.x <= Mathf.Max(a.x, b.x) + 0.01f &&
               Mathf.Min(a.y, b.y) - 0.01f <= p.y && p.y <= Mathf.Max(a.y, b.y) + 0.01f;
    }

    private static List<List<Vector2>> FindClosedLoops(List<Edge> edges)
    {
        var loops = new List<List<Vector2>>();
        var edgeDict = edges.GroupBy(e => e.a).ToDictionary(g => g.Key, g => g.ToList());
        var visited = new HashSet<(Vector2, Vector2)>();

        foreach (var edge in edges)
        {
            if (visited.Contains((edge.a, edge.b)) || visited.Contains((edge.b, edge.a)))
                continue;

            var loop = new List<Vector2> { edge.a };
            Vector2 current = edge.b;
            Vector2 prev = edge.a;

            while (current != edge.a)
            {
                loop.Add(current);
                visited.Add((prev, current));

                if (!edgeDict.ContainsKey(current)) break;

                var nextEdges = edgeDict[current]
                    .Where(e => !visited.Contains((current, e.b)) && e.b != prev)
                    .ToList();

                if (nextEdges.Count == 0) break;

                prev = current;
                current = nextEdges[0].b;
            }

            if (loop.Count >= 3 && current == edge.a)
                loops.Add(loop);
        }

        return loops;
    }

    private static GameObject TriangulateAndExtrude(List<Vector2> loop, float height, Material material)
    {
        Vector3[] baseVerts = loop.Select(p => new Vector3(p.x, 0, p.y)).ToArray();
        int[] tris = new int[(baseVerts.Length - 2) * 3];

        for (int i = 0; i < baseVerts.Length - 2; i++)
        {
            tris[i * 3] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i + 2;
        }

        Vector3[] fullVerts = new Vector3[baseVerts.Length * 2];
        for (int i = 0; i < baseVerts.Length; i++)
        {
            fullVerts[i] = baseVerts[i];
            fullVerts[i + baseVerts.Length] = baseVerts[i] + Vector3.up * height;
        }

        List<int> allTris = new List<int>(tris);

        int baseOffset = baseVerts.Length;

        // Add top cap (reverse winding to flip normals upward)
        for (int i = 0; i < baseVerts.Length - 2; i++)
        {
            allTris.Add(baseOffset);            // top 0
            allTris.Add(baseOffset + i + 2);    // top i+2
            allTris.Add(baseOffset + i + 1);    // top i+1
        }

        for (int i = 0; i < baseVerts.Length; i++)
        {
            int next = (i + 1) % baseVerts.Length;
            int a = i;
            int b = next;
            int c = i + baseVerts.Length;
            int d = next + baseVerts.Length;

            allTris.AddRange(new int[] { a, c, d, a, d, b });
        }

        GameObject wall = new GameObject("WallMesh");
        var mf = wall.AddComponent<MeshFilter>();
        var mr = wall.AddComponent<MeshRenderer>();
        var mesh = new Mesh();

        mesh.vertices = fullVerts;
        mesh.triangles = allTris.ToArray();
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        mr.material = material != null ? material : new Material(Shader.Find("Standard"));

        return wall;
    }
}