using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject agentPrefab;

    [Header("Managers")]
    public GridManager gridManager;
    public MazeGenerator mazeGenerator;
    public PathfindingManager pathfindingManager;
    public UIController uiController;

    private AgentController agent;

    void Awake()
    {
        // NEW: Set GridManager position to (0, 0, 0)
        if (gridManager != null)
        {
            gridManager.transform.position = Vector3.zero;  // (0, 0, 0)
            Debug.Log("GridManager position set to (0, 0, 0)");
        }

        if (agentPrefab != null && agent == null)
        {
            GameObject agentObj = Instantiate(agentPrefab, Vector3.up, Quaternion.identity);
            agent = agentObj.GetComponent<AgentController>();

            CameraManager camManager = FindObjectOfType<CameraManager>();
            if (camManager != null)
            {
                camManager.agent = agent.transform;
            }

            if (pathfindingManager != null)
            {
                pathfindingManager.agent = agent;
            }

            if (uiController != null)
            {
                uiController.agent = agent;
            }
        }
    }

    void Start()
    {
        Debug.Log("3D Pathfinding Visualizer Started!");
        Debug.Log("Controls:");
        Debug.Log("- Use dropdown to select algorithm");
        Debug.Log("- Click 'Generate Maze' for new maze");
        Debug.Log("- Click 'Set Source' and click on maze");
        Debug.Log("- Click 'Set Destination' and click on maze");
        Debug.Log("- Click 'Start Pathfinding' to visualize");
    }
}
