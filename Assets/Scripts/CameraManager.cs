using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform agent;
    public GridManager gridManager;
    public Camera miniCamera;   // Assign in inspector

    public enum CameraMode
    {
        TopDown,
        FollowAgent,
        Isometric,
        DualView
    }

    public CameraMode currentMode = CameraMode.TopDown;

    public float topDownHeight = 80f;
    public float followDistance = 1f;
    public float followHeight = 0.5f;
    public float cameraSmooth = 8f;
    public float fieldOfView = 40f;

    private Camera mainCamera;
    private Vector3 targetPosition;
    private Vector3 targetLookAt;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found!");
            return;
        }

        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        if (miniCamera == null)
            Debug.LogWarning("Mini camera not assigned!");

        SetCameraMode(CameraMode.TopDown);
    }

    void Update()
    {
        switch (currentMode)
        {
            case CameraMode.TopDown:
                UpdateTopDownCamera();
                break;
            case CameraMode.FollowAgent:
                UpdateFollowCamera();
                break;
            case CameraMode.Isometric:
                UpdateIsometricCamera();
                break;
            case CameraMode.DualView:
                UpdateDualViewCamera();
                break;
        }

        if (Input.GetKeyDown(KeyCode.T))
            TopDownCamera();

        if (Input.GetKeyDown(KeyCode.F))
            FollowAgentCamera();

        if (Input.GetKeyDown(KeyCode.I))
            IsometricCamera();

        if (Input.GetKeyDown(KeyCode.D))
            DualViewCamera();
    }

    Vector3 GetMazeCenter()
    {
        if (gridManager == null)
            return new Vector3(25, 0, 25);

        float centerX = gridManager.gridWorldSize.x * 0.5f;
        float centerZ = gridManager.gridWorldSize.y * 0.5f;

        return new Vector3(-0, 25, -0); // Tumhari value as is
    }

    void UpdateTopDownCamera()
    {
        Vector3 mazeCenter = GetMazeCenter();
        mainCamera.rect = new Rect(0f, 0f, 1f, 1f);

        targetPosition = new Vector3(mazeCenter.x, mazeCenter.y + topDownHeight, mazeCenter.z);
        targetLookAt = mazeCenter + Vector3.up * 2f;

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            Time.deltaTime * cameraSmooth
        );
        mainCamera.transform.LookAt(targetLookAt);
        mainCamera.fieldOfView = 60f;
    }

    void UpdateFollowCamera()
    {
        if (agent == null) return;
        mainCamera.rect = new Rect(0f, 0f, 1f, 1f);

        Vector3 agentForward = agent.forward;
        Vector3 cameraOffset = -agentForward * followDistance + Vector3.up * followHeight;

        targetPosition = agent.position + cameraOffset;
        targetLookAt = agent.position + agentForward * 2f + Vector3.up * 0.1f;

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            Time.deltaTime * cameraSmooth
        );
        mainCamera.transform.LookAt(targetLookAt);
        mainCamera.fieldOfView = 65f;
    }

    void UpdateIsometricCamera()
    {
        Vector3 mazeCenter = GetMazeCenter();
        mainCamera.rect = new Rect(0f, 0f, 1f, 1f);

        float distance = 45f;
        float height = 35f;

        targetPosition = mazeCenter + new Vector3(distance, height, distance);
        targetLookAt = mazeCenter;

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            Time.deltaTime * cameraSmooth
        );
        mainCamera.transform.LookAt(targetLookAt);
        mainCamera.fieldOfView = 55f;
    }

    void UpdateDualViewCamera()
    {
        if (agent == null) return;

        Vector3 mazeCenter = GetMazeCenter();
        // Main Camera: Left Half, Full Maze
        mainCamera.rect = new Rect(0f, 0f, 0.5f, 1f);
        targetPosition = new Vector3(mazeCenter.x, mazeCenter.y + topDownHeight, mazeCenter.z);
        targetLookAt = mazeCenter + Vector3.up * 2f;

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            Time.deltaTime * cameraSmooth
        );
        mainCamera.transform.LookAt(targetLookAt);
        mainCamera.fieldOfView = 60f;

        // Mini Camera: Right Half, Agent Zoom
        Vector3 agentForward = agent.forward;
        Vector3 zoomOffset = Vector3.up * 4f;
        Vector3 zoomPos = agent.position + zoomOffset;
        Vector3 zoomLookAt = agent.position + agentForward * 1f + Vector3.up;

        if (miniCamera != null)
        {
            miniCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f); // Right half
            miniCamera.transform.position = Vector3.Lerp(
                miniCamera.transform.position,
                zoomPos,
                Time.deltaTime * cameraSmooth
            );
            miniCamera.transform.LookAt(zoomLookAt);
            miniCamera.fieldOfView = 60f;
            miniCamera.enabled = true;
        }
    }

    public void SetCameraMode(CameraMode newMode)
    {
        currentMode = newMode;
        // Main camera: restore full for normal modes
        if (mainCamera != null && newMode != CameraMode.DualView)
            mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
        if (miniCamera != null)
        {
            miniCamera.enabled = (newMode == CameraMode.DualView);
            if (newMode != CameraMode.DualView)
                miniCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }
        Debug.Log("Camera mode changed to: " + newMode);
    }

    // MISSING TopDownCamera for T key!
    public void TopDownCamera()
    {
        SetCameraMode(CameraMode.TopDown);
    }
    public void FollowAgentCamera()
    {
        if (agent == null)
        {
            Debug.LogWarning("Agent not assigned!");
            return;
        }
        SetCameraMode(CameraMode.FollowAgent);
    }
    public void IsometricCamera()
    {
        SetCameraMode(CameraMode.Isometric);
    }
    public void DualViewCamera()
    {
        SetCameraMode(CameraMode.DualView);
    }
}
