using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform agentModel;

    private List<Node> currentPath;
    private int currentPathIndex = 0;
    private bool isFollowingPath = false;

    public Material agentMaterial;
    public GameObject trailPrefab;

    public AudioClip walkSound;
    private AudioSource audioSource;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = agentMaterial;
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.cyan;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = walkSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;
    }

    public void FollowPath(List<Node> path)
    {
        currentPath = path;
        currentPathIndex = 0;
        isFollowingPath = true;

        VisualizePath();
        StartCoroutine(MoveAlongPath());
    }

    void VisualizePath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = currentPath.Count;
        for (int i = 0; i < currentPath.Count; i++)
        {
            lineRenderer.SetPosition(i, currentPath[i].worldPosition + Vector3.up * 0.5f);
        }
    }

    IEnumerator MoveAlongPath()
    {
        if (audioSource != null && walkSound != null)
        {
            audioSource.Play();
        }

        while (currentPathIndex < currentPath.Count)
        {
            Vector3 targetPosition = currentPath[currentPathIndex].worldPosition;

            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

                Vector3 direction = (targetPosition - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
                }

                yield return null;
            }

            currentPathIndex++;
        }

        isFollowingPath = false;

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        Debug.Log("Agent reached destination!");
    }

    public void SetPosition(Vector3 position)
    {
        StopAllCoroutines();
        transform.position = position;
        isFollowingPath = false;
        lineRenderer.positionCount = 0;

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public bool IsMoving()
    {
        return isFollowingPath;
    }
}
