using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public bool isWalkable;
    public bool isVisited;
    public bool isPath;
    public Node parent;

    // For A* and Greedy Best-First
    public float gCost;
    public float hCost;
    public float fCost { get { return gCost + hCost; } }

    // NEW: Cost system
    public int costLevel = 0;  // 0=default(cost 1), 1=high(cost 4)

    public int GetCost()
    {
        switch (costLevel)
        {
            case 0: return 1;    // Default
            case 1: return 4;    // +3
            default: return 1;
        }
    }

    public void CycleCost()
    {
        costLevel = (costLevel + 1) % 2;  // Toggle: 0 ↔ 1
    }

    public Node(Vector3 _worldPos, int _gridX, int _gridY, bool _walkable)
    {
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        isWalkable = _walkable;
        isVisited = false;
        isPath = false;
        parent = null;
        gCost = 0;
        hCost = 0;
        costLevel = 0;  // NEW
    }

    public void ResetNode()
    {
        isVisited = false;
        isPath = false;
        parent = null;
        gCost = 0;
        hCost = 0;
        // costLevel NOT reset - persists!
    }
}
