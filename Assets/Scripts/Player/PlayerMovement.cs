using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask obstacleLayer;  // The layer for obstacles (black tiles)

    // Allow this to be set directly in the Inspector
    [SerializeField] public Vector2Int initialPosition = new Vector2Int(0, 0); // Set an initial position in Inspector
    [HideInInspector] public Vector2Int currentPosition;  // In grid coordinates
    private Vector2Int targetPosition;  // Target grid position to move towards

    private bool isMoving = false;

    void Start()
    {
        // Set the current position to the initial position defined in the Inspector
        currentPosition = initialPosition;
        targetPosition = currentPosition;
        transform.position = GridToWorldPosition(currentPosition);  // Update world position based on initial spawn position
    }

    void Update()
    {
        if (!isMoving)
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) { Move(0, 1); }
        if (Input.GetKeyDown(KeyCode.S)) { Move(0, -1); }
        if (Input.GetKeyDown(KeyCode.A)) { Move(-1, 0); }
        if (Input.GetKeyDown(KeyCode.D)) { Move(1, 0); }
    }

    void Move(int x, int y)
    {
        // Set the target position based on input
        targetPosition = currentPosition + new Vector2Int(x, y);

        // Check if the target position is not blocked by an obstacle
        if (IsPositionBlocked(targetPosition))
        {
            Debug.Log("Cannot move to blocked position.");
            return;  // Prevent movement if blocked
        }

        isMoving = true;
        StartCoroutine(MoveToTarget());
    }

    bool IsPositionBlocked(Vector2Int position)
    {
        // Check if the target grid position is blocked by an obstacle using a Physics2D check
        Vector3 targetWorldPosition = GridToWorldPosition(position);
        Collider2D hit = Physics2D.OverlapPoint(targetWorldPosition, obstacleLayer);  // Check if there's an obstacle at the target position
        return hit != null;  // Return true if there's an obstacle, false otherwise
    }

    IEnumerator MoveToTarget()
    {
        // Move smoothly towards the target position (grid-based)
        Vector3 startPosition = transform.position;
        Vector3 targetWorldPosition = GridToWorldPosition(targetPosition);
        float timeElapsed = 0f;
        float journeyLength = Vector3.Distance(startPosition, targetWorldPosition);

        while (timeElapsed < journeyLength / moveSpeed)
        {
            timeElapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, targetWorldPosition, timeElapsed * moveSpeed / journeyLength);
            yield return null;
        }

        // Snap to exact grid position after movement is done
        transform.position = targetWorldPosition;
        currentPosition = targetPosition;
        isMoving = false;
    }

    Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        // Convert grid position to world space (1 unit = 1 grid square)
        return new Vector3(gridPosition.x, gridPosition.y, 0f);
    }
}
