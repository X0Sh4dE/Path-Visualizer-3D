using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public GridManager gridManager;
    public MazeGenerator mazeGenerator;
    public AgentController agent;
    public PathfindingManager pathfindingManager;

    [Header("UI Elements")]
    public TMP_Dropdown algorithmDropdown;
    public Button generateMazeButton;
    public Button startPathfindingButton;
    public Button setSourceButton;
    public Button setDestinationButton;
    public TMP_Text statusText;
    public Slider speedSlider;

    public Slider pathVariationSlider;
    public TMP_Text pathVariationText;

    // New UI elements for stats
    public TMP_Text nodesExploredText;
    public TMP_Text totalCostText;
    public TMP_Text algorithmNameText;
    public TMP_Text pathLengthText;

    private Node sourceNode;
    private Node destinationNode;
    private bool settingSource = false;
    private bool settingDestination = false;

    void Start()
    {
        algorithmDropdown.ClearOptions();
        algorithmDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "BFS",
            "DFS",
            "A*",
            "Greedy Best First",
            "Dijkstra"
        });
        algorithmDropdown.onValueChanged.AddListener(OnAlgorithmChanged);

        generateMazeButton.onClick.AddListener(OnGenerateMaze);
        startPathfindingButton.onClick.AddListener(OnStartPathfinding);
        setSourceButton.onClick.AddListener(OnSetSource);
        setDestinationButton.onClick.AddListener(OnSetDestination);

        if (speedSlider != null)
        {
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
            speedSlider.value = 0.05f;
        }

        if (pathVariationSlider != null)
        {
            pathVariationSlider.minValue = 0f;
            pathVariationSlider.maxValue = 1f;
            pathVariationSlider.value = 0.3f;
            pathVariationSlider.wholeNumbers = false;
            pathVariationSlider.onValueChanged.AddListener(OnPathVariationChanged);
            OnPathVariationChanged(pathVariationSlider.value);
        }

        OnGenerateMaze();
        UpdateStatusText("Click 'Set Source' to choose starting point");

        ResetStatsUI();
    }

    void Update()
    {
        HandleMouseHover();

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Node clickedNode = gridManager.NodeFromWorldPoint(hit.point);

                if (clickedNode != null && clickedNode.isWalkable)
                {
                    if (settingSource)
                    {
                        sourceNode = clickedNode;
                        agent.SetPosition(sourceNode.worldPosition + Vector3.up);
                        settingSource = false;
                        gridManager.MarkSourceAsBlue(sourceNode);
                        UpdateStatusText("Source set! Now click 'Set Destination'");
                    }
                    else if (settingDestination)
                    {
                        destinationNode = clickedNode;
                        settingDestination = false;
                        gridManager.MarkDestinationAsRed(destinationNode);
                        UpdateStatusText("Destination set! Click 'Start Pathfinding'");
                    }
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
            Node hoveredNode = gridManager.NodeFromWorldPoint(hit.point);

            if (hoveredNode != null)
            {
                gridManager.UpdateHoverEffect(hoveredNode);
            }
            else
            {
                gridManager.ClearHover();
            }
        }
        else
        {
            gridManager.ClearHover();
        }
    }

    void OnGenerateMaze()
    {
        bool[,] mazeLayout = mazeGenerator.GenerateMaze(50, 50);
        gridManager.CreateGrid(mazeLayout);

        sourceNode = gridManager.GetNode(0, 0);
        destinationNode = gridManager.GetNode(49, 49);

        if (agent != null && sourceNode != null)
        {
            agent.SetPosition(sourceNode.worldPosition + Vector3.up);
        }

        UpdateStatusText("Maze generated (50x50)! Click 'Start Pathfinding' to begin");

        ResetStatsUI();
    }

    void OnStartPathfinding()
    {
        if (sourceNode == null || destinationNode == null)
        {
            UpdateStatusText("Please set source and destination first!");
            return;
        }

        if (!sourceNode.isWalkable || !destinationNode.isWalkable)
        {
            UpdateStatusText("Source or destination is not walkable!");
            return;
        }

        UpdateStatusText("Finding path using " + pathfindingManager.currentAlgorithm);
        pathfindingManager.StartPathfinding(sourceNode, destinationNode);
    }

    void OnSetSource()
    {
        settingSource = true;
        settingDestination = false;
        UpdateStatusText("Click on the maze to set source position");
    }

    void OnSetDestination()
    {
        settingDestination = true;
        settingSource = false;
        UpdateStatusText("Click on the maze to set destination position");
    }

    void OnAlgorithmChanged(int index)
    {
        pathfindingManager.SetAlgorithm(index);
        UpdateStatusText("Algorithm changed to " + pathfindingManager.currentAlgorithm);
    }

    void OnSpeedChanged(float value)
    {
        pathfindingManager.visualizationDelay = value;
    }

    void OnPathVariationChanged(float value)
    {
        if (pathVariationText != null)
        {
            int percentage = Mathf.RoundToInt(value * 100);
            pathVariationText.text = "Path Variation: " + percentage + "%";
        }

        if (mazeGenerator != null)
        {
            mazeGenerator.additionalPathChance = value;
        }
    }

    public void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log(message);
    }

    public void UpdateNodesExplored(int count)
    {
        if (nodesExploredText != null)
            nodesExploredText.text = "Nodes Explored: " + count;
    }

    public void UpdateTotalCost(float cost)
    {
        if (totalCostText != null)
            totalCostText.text = "Total Cost: " + cost;
    }

    public void UpdateAlgorithmName(string name)
    {
        if (algorithmNameText != null)
            algorithmNameText.text = "Algorithm: " + name;
    }

    public void UpdatePathLength(int length)
    {
        if (pathLengthText != null)
            pathLengthText.text = "Path Length: " + length;
    }

    private void ResetStatsUI()
    {
        UpdateNodesExplored(0);
        UpdateTotalCost(0);
        UpdateAlgorithmName("None");
        UpdatePathLength(0);
    }
}

