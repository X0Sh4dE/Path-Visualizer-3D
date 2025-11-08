using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathfindingManager : MonoBehaviour
{
    public GridManager gridManager;
    public AgentController agent;

    public enum Algorithm { BFS, DFS, AStar, GreedyBestFirst, Dijkstra }
    public Algorithm currentAlgorithm = Algorithm.AStar;
    public string currentAlgorithmName { get { return currentAlgorithm.ToString(); } }

    public float visualizationDelay = 0.05f;
    private bool isSearching = false;

    public void StartPathfinding(Node startNode, Node targetNode)
    {
        if (isSearching) return;

        gridManager.ResetGrid();

        switch (currentAlgorithm)
        {
            case Algorithm.BFS:
                StartCoroutine(BFS(startNode, targetNode));
                break;
            case Algorithm.DFS:
                StartCoroutine(DFS(startNode, targetNode));
                break;
            case Algorithm.AStar:
                StartCoroutine(AStar(startNode, targetNode));
                break;
            case Algorithm.GreedyBestFirst:
                StartCoroutine(GreedyBestFirst(startNode, targetNode));
                break;
            case Algorithm.Dijkstra:
                StartCoroutine(Dijkstra(startNode, targetNode));
                break;
        }
    }

    IEnumerator BFS(Node startNode, Node targetNode)
    {
        isSearching = true;
        Queue<Node> openSet = new Queue<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Enqueue(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Dequeue();
            closedSet.Add(currentNode);
            currentNode.isVisited = true;
            gridManager.MarkNodeAsVisited(currentNode);
            yield return new WaitForSeconds(visualizationDelay);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                isSearching = false;
                yield break;
            }

            foreach (Node neighbour in gridManager.GetNeighbours(currentNode))
            {
                if (!neighbour.isWalkable || closedSet.Contains(neighbour))
                    continue;
                if (!openSet.Contains(neighbour))
                {
                    neighbour.parent = currentNode;
                    openSet.Enqueue(neighbour);
                }
            }
        }

        Debug.Log("No path found!");
        isSearching = false;
    }

    IEnumerator DFS(Node startNode, Node targetNode)
    {
        isSearching = true;
        Stack<Node> openSet = new Stack<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Push(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Pop();
            if (closedSet.Contains(currentNode))
                continue;

            closedSet.Add(currentNode);
            currentNode.isVisited = true;
            gridManager.MarkNodeAsVisited(currentNode);
            yield return new WaitForSeconds(visualizationDelay);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                isSearching = false;
                yield break;
            }

            foreach (Node neighbour in gridManager.GetNeighbours(currentNode))
            {
                if (!neighbour.isWalkable || closedSet.Contains(neighbour))
                    continue;
                neighbour.parent = currentNode;
                openSet.Push(neighbour);
            }
        }

        Debug.Log("No path found!");
        isSearching = false;
    }

    IEnumerator AStar(Node startNode, Node targetNode)
    {
        isSearching = true;
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            currentNode.isVisited = true;
            gridManager.MarkNodeAsVisited(currentNode);
            yield return new WaitForSeconds(visualizationDelay);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                isSearching = false;
                yield break;
            }

            foreach (Node neighbour in gridManager.GetNeighbours(currentNode))
            {
                if (!neighbour.isWalkable || closedSet.Contains(neighbour))
                    continue;

                float newGCost = currentNode.gCost + neighbour.GetCost();

                if (newGCost < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newGCost;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;
                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        Debug.Log("No path found!");
        isSearching = false;
    }

    IEnumerator GreedyBestFirst(Node startNode, Node targetNode)
    {
        isSearching = true;
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].hCost < currentNode.hCost)
                    currentNode = openSet[i];
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            currentNode.isVisited = true;
            gridManager.MarkNodeAsVisited(currentNode);

            yield return new WaitForSeconds(visualizationDelay);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                isSearching = false;
                yield break;
            }

            foreach (Node neighbour in gridManager.GetNeighbours(currentNode))
            {
                if (!neighbour.isWalkable || closedSet.Contains(neighbour))
                    continue;
                if (!openSet.Contains(neighbour))
                {
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;
                    openSet.Add(neighbour);
                }
            }
        }

        Debug.Log("No path found!");
        isSearching = false;
    }

    IEnumerator Dijkstra(Node startNode, Node targetNode)
    {
        isSearching = true;
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        startNode.gCost = 0;
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].gCost < currentNode.gCost)
                    currentNode = openSet[i];
            }
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            currentNode.isVisited = true;
            gridManager.MarkNodeAsVisited(currentNode);
            yield return new WaitForSeconds(visualizationDelay);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                isSearching = false;
                yield break;
            }

            foreach (Node neighbour in gridManager.GetNeighbours(currentNode))
            {
                if (!neighbour.isWalkable || closedSet.Contains(neighbour))
                    continue;

                float newGCost = currentNode.gCost + neighbour.GetCost();

                if (newGCost < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newGCost;
                    neighbour.parent = currentNode;
                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        Debug.Log("No path found!");
        isSearching = false;
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode.isPath = true;
            currentNode = currentNode.parent;
        }

        path.Reverse();

        int stepIndex = 0;
        foreach (Node node in path)
        {
            if (node != endNode)
            {
                gridManager.MarkNodeAsPathAnimated(node, stepIndex);
                stepIndex++;
            }
        }

        gridManager.MarkDestinationAsPath(endNode);

        if (agent != null)
        {
            agent.FollowPath(path);
        }

        // Error-free UI update:
        UIController uiController = FindObjectOfType<UIController>();
        if (uiController != null)
        {
            int nodesExplored = 0;
            for (int x = 0; x < gridManager.GridSizeX; x++)
            {
                for (int y = 0; y < gridManager.GridSizeY; y++)
                {
                    Node n = gridManager.GetNode(x, y);
                    if (n != null && n.isVisited)
                        nodesExplored++;
                }
            }

            // Calculate total cost along path
            float totalCost = 0;
            Node current = endNode;
            while (current != startNode)
            {
                totalCost += current.GetCost();
                current = current.parent;
            }

            uiController.UpdateNodesExplored(nodesExplored);
            uiController.UpdateTotalCost(totalCost);
            uiController.UpdateAlgorithmName(currentAlgorithmName);
            uiController.UpdatePathLength(path.Count);
        }
    }

    float GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        return dstX + dstY;
    }

    public void SetAlgorithm(int index)
    {
        currentAlgorithm = (Algorithm)index;
        Debug.Log("Algorithm changed to: " + currentAlgorithm);
    }
}
