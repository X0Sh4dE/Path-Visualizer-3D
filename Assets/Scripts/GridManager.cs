using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Vector2 gridWorldSize = new Vector2(50, 50);
    public float nodeRadius = 0.5f;
    public LayerMask unwalkableMask;
    public Transform wallParent;

    Node[,] grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public Material walkableMaterial;
    public Material unwalkableMaterial;
    public Material visitedMaterial;
    public Color visitedColor = new Color(1f, 1f, 0f, 0.3f);
    public Color pathColor = new Color(0f, 1f, 0f, 0.8f);

    private Dictionary<Node, GameObject> nodeToFloorObject = new Dictionary<Node, GameObject>();
    private Dictionary<Vector2Int, GameObject> wallObjects = new Dictionary<Vector2Int, GameObject>();

    //public float blockRenderDelay = 0.001f;
    public float wallRemovalDelay = 0.002f;

    private Node hoveredNode = null;
    private Node sourceNode = null;
    private Node destinationNode = null;
    public Material hoverMaterial;
    public Color hoverColor = new Color(1f, 1f, 1f, 0.5f);

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
    }

    void Update()
    {
        HandleMouseHover();

        // Right-click to toggle cost
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Node clickedNode = NodeFromWorldPoint(hit.point);
                if (clickedNode != null && clickedNode.isWalkable)
                {
                    clickedNode.CycleCost();
                    UpdateCostVisual(clickedNode);
                    Debug.Log($"Tile Cost: {clickedNode.GetCost()}");
                }
            }
        }
    }

    void HandleMouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Node hoveredNode = NodeFromWorldPoint(hit.point);
            if (hoveredNode != null)
                UpdateHoverEffect(hoveredNode);
            else
                ClearHover();
        }
        else
        {
            ClearHover();
        }
    }

    public void CreateGrid(bool[,] mazeLayout)
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        if (wallParent != null)
        {
            foreach (Transform child in wallParent)
            {
                Destroy(child.gameObject);
            }
        }

        nodeToFloorObject.Clear();
        wallObjects.Clear();

        StartCoroutine(CreateGridSequential(mazeLayout, worldBottomLeft));
    }

    IEnumerator CreateGridSequential(bool[,] mazeLayout, Vector3 worldBottomLeft)
    {
        Debug.Log("PHASE 1: Filling grid with walls...");
        yield return StartCoroutine(CreateFullGridWithWalls(worldBottomLeft));

        Debug.Log("PHASE 2: Carving maze paths...");
        yield return StartCoroutine(CarveMazeAnimated(mazeLayout, worldBottomLeft));

        Debug.Log("Maze generation complete!");
    }

    IEnumerator CreateFullGridWithWalls(Vector3 worldBottomLeft)
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);

                grid[x, y] = new Node(worldPoint, x, y, false);

                if (wallPrefab != null)
                {
                    GameObject wall = Instantiate(wallPrefab, worldPoint, Quaternion.identity);
                    wall.transform.parent = wallParent;
                    wallObjects[new Vector2Int(x, y)] = wall;
                }

                yield return new WaitForSeconds(0);//blockRenderDelay);
            }
        }

        Debug.Log("Grid filled complete!");
    }

    IEnumerator CarveMazeAnimated(bool[,] mazeLayout, Vector3 worldBottomLeft)
    {
        yield return new WaitForSeconds(0.5f);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (mazeLayout[x, y])
                {
                    grid[x, y].isWalkable = true;
                    Vector2Int pos = new Vector2Int(x, y);

                    if (wallObjects.ContainsKey(pos))
                    {
                        GameObject wall = wallObjects[pos];
                        Destroy(wall);
                        wallObjects.Remove(pos);
                    }

                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                    if (floorPrefab != null)
                    {
                        GameObject floor = Instantiate(floorPrefab, worldPoint, Quaternion.identity);
                        floor.transform.parent = wallParent;
                        nodeToFloorObject[grid[x, y]] = floor;

                        // Random cost on generation (0 or 1 = cost 1 or 4)
                        grid[x, y].costLevel = Random.Range(0, 2);
                        UpdateCostVisual(grid[x, y]);
                    }

                    yield return new WaitForSeconds(wallRemovalDelay);
                }
            }
        }

        Debug.Log("Maze carving complete!");
    }

    public void MarkNodeAsVisited(Node node)
    {
        if (node == null) return;

        if (nodeToFloorObject.ContainsKey(node))
        {
            GameObject floorObj = nodeToFloorObject[node];
            if (floorObj != null)
            {
                Renderer renderer = floorObj.GetComponent<Renderer>();
                if (renderer != null && visitedMaterial != null)
                {
                    Material mat = new Material(visitedMaterial);
                    mat.color = visitedColor;
                    renderer.material = mat;
                }
            }
        }
    }

    public void MarkNodeAsPathAnimated(Node node, int stepIndex)
    {
        if (node == null) return;

        if (nodeToFloorObject.ContainsKey(node))
        {
            GameObject floorObj = nodeToFloorObject[node];
            if (floorObj != null)
            {
                Renderer renderer = floorObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    StartCoroutine(AnimatePathColor(renderer, stepIndex));
                }
            }
        }
    }

    IEnumerator AnimatePathColor(Renderer renderer, int stepIndex)
    {
        Material mat = new Material(renderer.material);
        renderer.material = mat;

        Color brightGreen = new Color(0f, 1f, 0f, 0.8f);
        mat.color = brightGreen;

        yield return new WaitForSeconds(0.3f);

        float fadeDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0.8f, 0.1f, elapsedTime / fadeDuration);
            mat.color = new Color(0f, 1f, 0f, alpha);
            yield return null;
        }

        mat.color = new Color(0f, 1f, 0f, 0.1f);
    }

    public void MarkDestinationAsPath(Node node)
    {
        if (node == null) return;

        if (nodeToFloorObject.ContainsKey(node))
        {
            GameObject floorObj = nodeToFloorObject[node];
            if (floorObj != null)
            {
                Renderer renderer = floorObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(renderer.material);
                    mat.color = new Color(0f, 1f, 0f, 0.9f);
                    renderer.material = mat;
                }
            }
        }
    }

    public void MarkDestinationAsRed(Node node)
    {
        if (node == null) return;

        destinationNode = node;

        if (nodeToFloorObject.ContainsKey(node))
        {
            GameObject floorObj = nodeToFloorObject[node];
            if (floorObj != null)
            {
                Renderer renderer = floorObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(renderer.material);
                    mat.color = new Color(1f, 0f, 0f, 0.8f);
                    renderer.material = mat;
                }
            }
        }
    }

    public void MarkSourceAsBlue(Node node)
    {
        if (node == null) return;

        sourceNode = node;

        if (nodeToFloorObject.ContainsKey(node))
        {
            GameObject floorObj = nodeToFloorObject[node];
            if (floorObj != null)
            {
                Renderer renderer = floorObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(renderer.material);
                    mat.color = new Color(0f, 0.5f, 1f, 0.8f);
                    renderer.material = mat;
                }
            }
        }
    }

    public void UpdateHoverEffect(Node node)
    {
        if (hoveredNode != null && hoveredNode != node)
        {
            ResetNodeMaterial(hoveredNode);
        }

        if (node != null && node.isWalkable)
        {
            if (node == sourceNode || node == destinationNode)
            {
                hoveredNode = null;
                return;
            }

            if (nodeToFloorObject.ContainsKey(node))
            {
                GameObject floorObj = nodeToFloorObject[node];
                if (floorObj != null)
                {
                    Renderer renderer = floorObj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material mat = new Material(renderer.material);
                        mat.color = hoverColor;
                        renderer.material = mat;
                    }
                }
            }
            hoveredNode = node;
        }
    }

    void ResetNodeMaterial(Node node)
    {
        if (node == null) return;

        if (node == sourceNode || node == destinationNode)
        {
            return;
        }

        if (nodeToFloorObject.ContainsKey(node))
        {
            GameObject floorObj = nodeToFloorObject[node];
            if (floorObj != null)
            {
                Renderer renderer = floorObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = walkableMaterial;
                }
            }
        }
    }

    public void ClearHover()
    {
        if (hoveredNode != null)
        {
            ResetNodeMaterial(hoveredNode);
            hoveredNode = null;
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int checkX = node.gridX + dx[i];
            int checkY = node.gridY + dy[i];

            if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
            {
                neighbours.Add(grid[checkX, checkY]);
            }
        }

        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    public Node GetNode(int x, int y)
    {
        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
            return grid[x, y];
        return null;
    }

    public void ResetGrid()
    {
        foreach (Node node in grid)
        {
            node.ResetNode();
        }

        foreach (var kvp in nodeToFloorObject)
        {
            if (kvp.Value != null)
            {
                Renderer renderer = kvp.Value.GetComponent<Renderer>();
                if (renderer != null && walkableMaterial != null)
                {
                    renderer.material = walkableMaterial;
                }
            }
        }
    }

    // UPDATED: Update cost visual - NO TEXT
    void UpdateCostVisual(Node node)
    {
        if (nodeToFloorObject.ContainsKey(node))
        {
            GameObject floorObj = nodeToFloorObject[node];
            if (floorObj != null)
            {
                Renderer renderer = floorObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(walkableMaterial);

                    switch (node.costLevel)
                    {
                        case 0:
                            mat.color = new Color(1f, 1f, 1f, 0.8f);  // White
                            break;
                        case 1:
                            mat.color = new Color(1f, 0.8f, 0.5f, 0.8f);  // Orange
                            break;
                    }
                    renderer.material = mat;
                }
            }
        }
    }

    public int GridSizeX { get { return gridSizeX; } }
    public int GridSizeY { get { return gridSizeY; } }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = n.isWalkable ? Color.white : Color.red;
                if (n.isPath)
                    Gizmos.color = Color.blue;
                else if (n.isVisited)
                    Gizmos.color = Color.yellow;

                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
