
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    private int width;
    private int height;
    private bool[,] maze;

    public GridManager gridManager;
    public float carvingDelay = 0.02f;

    // NEW: Control multiple paths
    public float additionalPathChance = 0.3f;  // 30% extra paths

    public bool[,] GenerateMazeWithAnimation(int w, int h)
    {
        width = w;
        height = h;
        maze = new bool[width, height];

        // Initialize ALL as walls
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = false;
            }
        }

        StartCoroutine(CarveMazeAnimated());
        return maze;
    }

    IEnumerator CarveMazeAnimated()
    {
        // Random starting position
        int startX = Random.Range(0, width);
        int startY = Random.Range(0, height);

        if (startX % 2 == 0) startX++;
        if (startY % 2 == 0) startY++;

        Debug.Log("Maze carving started from: " + startX + ", " + startY);

        // Start carving
        yield return StartCoroutine(CarveMazeRecursiveAnimated(startX, startY));

        // NEW: Add extra paths for variation
        yield return StartCoroutine(AddMultiplePaths());

        maze[0, 0] = true;
        maze[width - 1, height - 1] = true;

        Debug.Log("Maze with multiple paths complete!");
    }

    IEnumerator CarveMazeRecursiveAnimated(int x, int y)
    {
        maze[x, y] = true;

        if (gridManager != null)
        {
            yield return new WaitForSeconds(carvingDelay);
        }

        int[] directions = { 0, 1, 2, 3 };

        for (int i = 0; i < directions.Length; i++)
        {
            int temp = directions[i];
            int randomIndex = Random.Range(i, directions.Length);
            directions[i] = directions[randomIndex];
            directions[randomIndex] = temp;
        }

        foreach (int dir in directions)
        {
            int nx = x;
            int ny = y;

            switch (dir)
            {
                case 0: ny -= 2; break;
                case 1: nx += 2; break;
                case 2: ny += 2; break;
                case 3: nx -= 2; break;
            }

            if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && !maze[nx, ny])
            {
                int wx = x + (nx - x) / 2;
                int wy = y + (ny - y) / 2;
                maze[wx, wy] = true;

                yield return StartCoroutine(CarveMazeRecursiveAnimated(nx, ny));
            }
        }
    }

    // NEW METHOD: Add multiple paths for variation
    IEnumerator AddMultiplePaths()
    {
        Debug.Log("Adding multiple paths...");

        // Go through maze and randomly open walls
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // If it's a wall
                if (!maze[x, y])
                {
                    // Check if neighbors are walkable
                    bool hasWalkableNeighbors = false;
                    int walkableCount = 0;

                    // Check all 4 directions
                    if (x > 0 && maze[x - 1, y]) walkableCount++;
                    if (x < width - 1 && maze[x + 1, y]) walkableCount++;
                    if (y > 0 && maze[x, y - 1]) walkableCount++;
                    if (y < height - 1 && maze[x, y + 1]) walkableCount++;

                    // If wall is between 2+ walkable paths, might connect them
                    if (walkableCount >= 2)
                    {
                        // Random chance to open this wall
                        if (Random.value < additionalPathChance)
                        {
                            maze[x, y] = true;  // Open wall
                            yield return new WaitForSeconds(0.01f);
                        }
                    }
                }

                yield return null;
            }
        }

        Debug.Log("Multiple paths added!");
    }

    public bool[,] GenerateMaze(int w, int h)
    {
        return GenerateMazeWithAnimation(w, h);
    }

    public bool[,] GenerateRandomMaze(int w, int h, float wallDensity = 0.3f)
    {
        width = w;
        height = h;
        maze = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = Random.value > wallDensity;
            }
        }

        maze[0, 0] = true;
        maze[width - 1, height - 1] = true;

        return maze;
    }
}
