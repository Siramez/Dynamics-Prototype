using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Vector2 gridWorldSize;      // Size of the grid in world units (e.g., 18 x 6 units)
    public float nodeRadius;           // Radius of each node in the grid
    public LayerMask obstacleLayer;    // Set this to the "Obstacle" layer for black tiles
    public LayerMask waypointLayer;    // Set this to the "Waypoint" layer to check for waypoints

    private Node[,] grid;
    private float nodeDiameter;        // Diameter of each node
    public int gridSizeX, gridSizeY;  // Number of nodes in the grid (columns and rows)

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);  // Calculate grid columns
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);  // Calculate grid rows
        CreateGrid();
    }

    // Create the grid based on world size and node size
    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector2 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Calculate the world position of each grid node
                Vector2 worldPoint = worldBottomLeft + Vector2.right * (x * nodeDiameter + nodeRadius) + Vector2.up * (y * nodeDiameter + nodeRadius);

                // Check if the node overlaps with an obstacle (black tile)
                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius, obstacleLayer);

                // Check if there is a waypoint at this grid position
                if (Physics2D.OverlapCircle(worldPoint, nodeRadius, waypointLayer))
                {
                    walkable = true; // Node is walkable if there's a waypoint at this position
                }

                grid[x, y] = new Node(walkable, worldPoint, x, y);  // Assign the node with walkable state
            }
        }
    }

    // Convert a world position to a node in the grid
    public Node GetNodeFromWorldPosition(Vector2 worldPosition)
    {
        // Normalize world position relative to the grid
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        // Convert percentage to grid coordinates
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];  // Return the node at the calculated grid position
    }

    // Get all the walkable neighbor nodes of a given node (only cardinal directions)
    public List<Node> GetNeighborNodes(Node node)
    {
        List<Node> neighbors = new List<Node>();

        // Cardinal directions (Up, Right, Down, Left)
        int[,] directions = new int[,] { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };

        // Check each cardinal direction
        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int checkX = node.gridX + directions[i, 0];
            int checkY = node.gridY + directions[i, 1];

            // Ensure the neighbor is within grid bounds
            if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
            {
                Node neighbor = grid[checkX, checkY];
                if (neighbor.walkable)  // Only add walkable neighbors
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    // Get a random walkable node from the grid
    public Node GetRandomWaypointNode()
    {
        List<Node> walkableNodes = new List<Node>();

        // Iterate over all nodes and collect walkable ones
        foreach (Node node in grid)
        {
            if (node.walkable)
            {
                walkableNodes.Add(node);
            }
        }

        // If we have walkable nodes, return a random one
        if (walkableNodes.Count > 0)
        {
            return walkableNodes[Random.Range(0, walkableNodes.Count)];
        }
        else
        {
            Debug.LogWarning("No walkable nodes available!");
            return null;  // Return null if no walkable nodes are found
        }
    }

    // Add this method to GridManager to convert grid position to world space
    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        // Assuming each grid square is 1 unit in world space
        return new Vector3(gridPosition.x, gridPosition.y, 0f);
    }

    public List<Node> GetAllWalkableNodes()
    {
        List<Node> walkableNodes = new List<Node>();

        // Iterate over all nodes and collect walkable ones
        foreach (Node node in grid)
        {
            if (node.walkable)
            {
                walkableNodes.Add(node);
            }
        }

        return walkableNodes;
    }
}
