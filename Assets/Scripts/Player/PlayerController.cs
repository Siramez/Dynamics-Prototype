using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public LayerMask obstacleLayer;  // The layer for obstacles (black tiles)

    [SerializeField] public Vector2Int initialPosition = new Vector2Int(0, 0); // Set an initial position in Inspector
    [HideInInspector] public Vector2Int currentPosition;  // In grid coordinates
    private Vector2Int targetPosition;  // Target grid position to move towards
    private bool isMoving = false;

    // UI Buttons for movement
    public Button upButton;
    public Button downButton;
    public Button leftButton;
    public Button rightButton;

    void Start()
    {
        // Set the current position to the initial position defined in the Inspector
        currentPosition = initialPosition;
        targetPosition = currentPosition;
        transform.position = GridToWorldPosition(currentPosition);  // Update world position based on initial spawn position

        // Set up listeners for each button to call the Move method in the specified direction
        upButton.onClick.AddListener(() => Move(0, 1));
        downButton.onClick.AddListener(() => Move(0, -1));
        leftButton.onClick.AddListener(() => Move(-1, 0));
        rightButton.onClick.AddListener(() => Move(1, 0));
    }

    void Move(int x, int y)
    {
        if (isMoving) return;  // Prevent movement if already moving

        // Set the target position based on button click
        targetPosition = currentPosition + new Vector2Int(x, y);

        // Check if the target position is blocked by an obstacle
        if (IsPositionBlocked(targetPosition))
        {
            Debug.Log("Cannot move to blocked position.");
            return;  // Prevent movement if blocked
        }

        SnapToTarget();  // Snap directly to target position
    }

    bool IsPositionBlocked(Vector2Int position)
    {
        // Check if the target grid position is blocked by an obstacle
        Vector3 targetWorldPosition = GridToWorldPosition(position);
        Collider2D hit = Physics2D.OverlapPoint(targetWorldPosition, obstacleLayer);
        return hit != null;  // Return true if there's an obstacle, false otherwise
    }

    void SnapToTarget()
    {
        // Directly snap to target position without gradual movement
        Vector3 targetWorldPosition = GridToWorldPosition(targetPosition);
        transform.position = targetWorldPosition;
        currentPosition = targetPosition;
    }

    Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        // Convert grid position to world space (1 unit = 1 grid square)
        return new Vector3(gridPosition.x, gridPosition.y, 0f);
    }
}
