using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralCircuitBackground : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField] private int gridWidth = 20; // Number of grid cells horizontally
    [SerializeField] private int gridHeight = 12; // Number of grid cells vertically
    [SerializeField] [Range(0.1f, 1f)] private float traceDensity = 0.5f; // Chance of a grid cell starting a trace
    [SerializeField] private int maxTraceLength = 10; // Max segments per trace
    [SerializeField] private float nodeChance = 0.15f; // Chance of a vertex becoming a node

    [Header("Appearance Settings")]
    [SerializeField] private Color traceColor = new Color(0.5f, 0.8f, 1f, 0.8f); // Light greenish-blue base
    [SerializeField] private Color nodeColor = new Color(0.8f, 1f, 1f, 1f);    // Brighter node color
    [SerializeField] private Color pulseColor = Color.white; // Color of the animated pulse
    [SerializeField] [Range(0.1f, 5f)] private float pulseSpeed = 1.5f; // How fast the pulse travels (segments per second)
    [SerializeField] [Range(1, 5)] private int pulseLength = 3; // How many segments the pulse covers
    [SerializeField] private float nodePulseFrequency = 1.0f; // How fast nodes pulse

    [Header("References")]
    [SerializeField] private Camera targetCamera; // Camera viewing this background (often Main Camera)

    // --- Private Variables ---
    private Mesh mesh;
    private MeshFilter meshFilter;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> indices = new List<int>();
    private List<Color> colors = new List<Color>();

    // Store trace information for animation
    private class Trace
    {
        public List<int> vertexIndices = new List<int>(); // Indices into the main vertices list
        public float pulsePosition = 0f; // Current position of the pulse along the trace (0 to N segments)
        public float traceLength;
    }
    private List<Trace> allTraces = new List<Trace>();

    // Store node information
    private class Node
    {
        public int vertexIndex; // Index into the main vertices list
        public float pulseOffset; // Random offset for pulsing animation
    }
    private List<Node> allNodes = new List<Node>();

    private Vector2[,] gridPoints; // Store world positions of grid intersections
    private bool[,] gridOccupied; // Keep track of occupied grid points to avoid overlap
    private float worldScreenWidth;
    private float worldScreenHeight;


    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "CircuitBackgroundMesh";
        meshFilter.mesh = mesh;

        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null)
        {
            Debug.LogError("ProceduralCircuitBackground needs a Target Camera assigned.", this);
            enabled = false;
            return;
        }

        // Make sure the material supports vertex colors
        var renderer = GetComponent<MeshRenderer>();
        if (renderer.material == null)
        {
             Debug.LogWarning("Assign a Material that uses Vertex Colors (e.g., Unlit/URP with vertex color enabled) to the MeshRenderer.", this);
        }


        GenerateLayout();
        BuildMesh();
    }

    void GenerateLayout()
    {
        // --- Calculate Grid World Positions ---
        // Get screen boundaries in world space based on the camera's view
        worldScreenHeight = targetCamera.orthographicSize * 2f;
        worldScreenWidth = worldScreenHeight * targetCamera.aspect;

        Vector3 bottomLeft = targetCamera.transform.position - targetCamera.transform.right * worldScreenWidth / 2f - targetCamera.transform.up * worldScreenHeight / 2f;
        // Adjust Z position to be slightly in front of camera near clip plane but behind other UI/game elements
        bottomLeft.z = targetCamera.nearClipPlane + 0.1f;
        // Align with the GameObject's Z position if needed (assuming it's placed correctly)
        bottomLeft.z = transform.position.z;


        float cellWidth = worldScreenWidth / (gridWidth -1); // -1 because N points define N-1 cells
        float cellHeight = worldScreenHeight / (gridHeight -1);

        gridPoints = new Vector2[gridWidth, gridHeight];
        gridOccupied = new bool[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                gridPoints[x, y] = new Vector2(bottomLeft.x + x * cellWidth, bottomLeft.y + y * cellHeight);
            }
        }

        // --- Generate Traces ---
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (Random.value < traceDensity && !gridOccupied[x,y])
                {
                    GenerateSingleTrace(x, y);
                }
            }
        }
    }

    void GenerateSingleTrace(int startX, int startY)
    {
        Trace currentTrace = new Trace();
        allTraces.Add(currentTrace);

        int currentX = startX;
        int currentY = startY;
        Vector2 lastDirection = Vector2.zero; // Prevent immediate reversal

        for (int i = 0; i < maxTraceLength; i++)
        {
            // Add current point to vertices if not already added by another trace segment
            int currentVertexIndex = AddVertex(gridPoints[currentX, currentY], traceColor);
             // Mark grid point as occupied (can be occupied by multiple traces passing through)
             gridOccupied[currentX, currentY] = true;

            // Add vertex index to the current trace
            currentTrace.vertexIndices.Add(currentVertexIndex);

            // Decide if this vertex becomes a node
            if (Random.value < nodeChance)
            {
                AddNode(currentVertexIndex);
            }

            // --- Find next point ---
            List<Vector2Int> possibleMoves = new List<Vector2Int>();
            // Check potential moves (Up, Down, Left, Right)
            int nextX, nextY;

            // Right
            nextX = currentX + 1; nextY = currentY;
            if (nextX < gridWidth && lastDirection != Vector2.left ) possibleMoves.Add(new Vector2Int(nextX, nextY));
            // Left
            nextX = currentX - 1; nextY = currentY;
            if (nextX >= 0 && lastDirection != Vector2.right) possibleMoves.Add(new Vector2Int(nextX, nextY));
            // Up
            nextX = currentX; nextY = currentY + 1;
            if (nextY < gridHeight && lastDirection != Vector2.down ) possibleMoves.Add(new Vector2Int(nextX, nextY));
            // Down
            nextX = currentX; nextY = currentY - 1;
            if (nextY >= 0 && lastDirection != Vector2.up   ) possibleMoves.Add(new Vector2Int(nextX, nextY));

            if (possibleMoves.Count == 0) break; // Dead end

            // Choose a random valid move
            Vector2Int chosenMove = possibleMoves[Random.Range(0, possibleMoves.Count)];
            int nextVertexIndex = AddVertex(gridPoints[chosenMove.x, chosenMove.y], traceColor);
            gridOccupied[chosenMove.x, chosenMove.y] = true; // Mark next point

            // Add indices for the line segment
            indices.Add(currentVertexIndex);
            indices.Add(nextVertexIndex);

            // Update current position and last direction
            Vector2Int directionVec = chosenMove - new Vector2Int(currentX, currentY);
            lastDirection = new Vector2(directionVec.x, directionVec.y); // Store direction to avoid reversal
            currentX = chosenMove.x;
            currentY = chosenMove.y;

            // Break if the next chosen point was already part of *this specific trace* (simple loop prevention)
            if (currentTrace.vertexIndices.Contains(nextVertexIndex) && currentTrace.vertexIndices.Count > 1) {
                 // Add the final point to the trace list before breaking
                 currentTrace.vertexIndices.Add(nextVertexIndex);
                 if (Random.value < nodeChance) AddNode(nextVertexIndex); // Chance for node at end
                 break;
            }


             // If it's the last segment, add the final point to the trace list
             if (i == maxTraceLength - 1) {
                 currentTrace.vertexIndices.Add(nextVertexIndex);
                 if (Random.value < nodeChance) AddNode(nextVertexIndex); // Chance for node at end
             }
        }
         currentTrace.traceLength = currentTrace.vertexIndices.Count -1; // Store number of segments
    }

    // Helper to add vertex and color, returning the index
    int AddVertex(Vector3 position, Color color)
    {
        // Could add logic here to check if an identical vertex position already exists
        // to potentially reuse vertices, but for simplicity, we add duplicates for now.
        vertices.Add(position);
        colors.Add(color);
        return vertices.Count - 1;
    }

     // Helper to add a node
     void AddNode(int vertexIndex) {
         // Check if a node already exists at this vertex index
         bool exists = false;
         foreach(var node in allNodes) {
             if (node.vertexIndex == vertexIndex) {
                 exists = true;
                 break;
             }
         }
         if (!exists) {
             allNodes.Add(new Node { vertexIndex = vertexIndex, pulseOffset = Random.value * 10f }); // Add random offset for pulsing
             // Optionally change the vertex color immediately
             // colors[vertexIndex] = nodeColor;
         }
     }


    void BuildMesh()
    {
        mesh.Clear(); // Clear previous data

        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        // Use Line topology: Each pair of indices defines a line segment
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);

        mesh.RecalculateBounds(); // Important for visibility
    }

    void Update()
    {
        bool colorsChanged = false;

        // --- Reset all colors to base ---
        for (int i = 0; i < colors.Count; i++)
        {
            // Basic check: is this vertex part of a node?
            bool isNode = false;
            foreach (var node in allNodes) {
                if (node.vertexIndex == i) {
                     isNode = true;
                     // Node pulsing animation
                     float pulse = 0.5f + Mathf.Sin((Time.time + node.pulseOffset) * nodePulseFrequency * Mathf.PI * 2f) * 0.5f; // 0 to 1 sine wave
                     colors[i] = Color.Lerp(traceColor, nodeColor, pulse); // Lerp between base trace and bright node color
                     break;
                }
            }
            // If not a node, reset to base trace color (will be overridden by pulse below if needed)
            if (!isNode) {
                colors[i] = traceColor;
            }
        }


        // --- Animate Pulses on Traces ---
        foreach (var trace in allTraces)
        {
             if (trace.traceLength <= 0) continue;

            // Move pulse position
            trace.pulsePosition += Time.deltaTime * pulseSpeed;
            // Wrap pulse position around the trace length
            if (trace.pulsePosition > trace.traceLength + pulseLength) // Wrap around after pulse fully exits
            {
                trace.pulsePosition = 0f; // Reset pulse to start (or add a delay)
            }

            // Apply pulse color to relevant segments
            for (int i = 0; i < trace.traceLength; i++)
            {
                float segmentStartPos = i;
                float segmentEndPos = i + 1;

                // Check if the current pulse overlaps this segment
                float pulseStart = trace.pulsePosition - pulseLength;
                float pulseEnd = trace.pulsePosition;

                // Check for overlap range (simple version)
                if (Mathf.Max(segmentStartPos, pulseStart) < Mathf.Min(segmentEndPos, pulseEnd))
                {
                     // Apply pulse color to the vertices of this segment
                     int vertexIndex1 = trace.vertexIndices[i];
                     int vertexIndex2 = trace.vertexIndices[i + 1];

                     // Only apply if not overridden by a brighter node color
                     if(colors[vertexIndex1] != nodeColor) colors[vertexIndex1] = pulseColor;
                     if(colors[vertexIndex2] != nodeColor) colors[vertexIndex2] = pulseColor;

                     colorsChanged = true;
                }
            }
        }


        // --- Apply Updated Colors to Mesh ---
        // Only update the mesh colors if they actually changed
        // This is a small optimization; updating vertices is much more expensive.
       // if (colorsChanged) // Can uncomment this, but updating colors every frame is usually okay
       // {
            mesh.SetColors(colors);
       // }
    }
}
