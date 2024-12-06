using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Aggressive,
        Battle,
        Defensive
    }

    public float moveSpeed = 2f;
    public float waitTimeAtWaypoint = 1f;
    public float detectionRange = 5f; // Range within which the enemy will become aggressive
    public float battleRange = 1.5f; // Range where the enemy stops moving and enters battle state
    public float fleeRange = 3f; // Distance to maintain from player when fleeing

    public int maxHealth = 50;
    public Slider healthBar;
    public int attackPower = 1; // Reduced attack power to avoid rapid health drain
    public float attackInterval = 1.5f;
    public float lowHealthThreshold = 0.3f; // Threshold to enter defensive state

    public int currentHealth;
    public EnemyState currentState = EnemyState.Patrol;
    private Transform playerTransform;
    private PlayerCombat playerCombat;
    private GridManager gridManager;
    private Node targetNode;
    private bool isMoving = false;
    private Coroutine attackCoroutine;

    [SerializeField] private Vector2Int initialSpawnPosition = new Vector2Int(0, 0); // Set this in the Inspector

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerCombat = playerTransform.GetComponent<PlayerCombat>();

        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;

        transform.position = gridManager.GridToWorldPosition(initialSpawnPosition);

        // Start the enemy behavior
        StartCoroutine(EnemyBehavior());
    }

    private IEnumerator EnemyBehavior()
    {
        while (true)
        {
            switch (currentState)
            {
                case EnemyState.Patrol:
                    if (!isMoving)
                    {
                        SetRandomTargetNode();
                        yield return MoveToNode(targetNode);
                        yield return new WaitForSeconds(waitTimeAtWaypoint);
                    }
                    break;

                case EnemyState.Aggressive:
                    if (!isMoving)
                    {
                        SetAggressiveTargetNode();
                        yield return MoveToNode(targetNode);
                    }
                    break;

                case EnemyState.Battle:
                    if (attackCoroutine == null)
                    {
                        attackCoroutine = StartCoroutine(AutoAttack());
                    }
                    break;

                case EnemyState.Defensive:
                    FleeFromPlayer();
                    break;
            }

            yield return null;
        }
    }

    private IEnumerator MoveToNode(Node node)
    {
        isMoving = true;

        // Stop movement if in Battle or Defensive state
        if (currentState == EnemyState.Battle || currentState == EnemyState.Defensive)
        {
            isMoving = false;
            yield break;
        }

        Vector2 targetPosition = node.worldPosition;
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }

    private void SetRandomTargetNode()
    {
        Node currentNode = gridManager.GetNodeFromWorldPosition(transform.position);
        List<Node> neighbors = gridManager.GetNeighborNodes(currentNode);
        if (neighbors.Count > 0)
        {
            targetNode = neighbors[Random.Range(0, neighbors.Count)];
        }
    }

    private void SetAggressiveTargetNode()
    {
        if (Vector2.Distance(transform.position, playerTransform.position) <= detectionRange)
        {
            Node currentNode = gridManager.GetNodeFromWorldPosition(transform.position);
            List<Node> neighbors = gridManager.GetNeighborNodes(currentNode);

            Vector2 playerPosition2D = new Vector2(playerTransform.position.x, playerTransform.position.y);
            neighbors.RemoveAll(node => node.worldPosition == playerPosition2D || !node.walkable);

            if (neighbors.Count > 0)
            {
                Node closestNode = null;
                float shortestDistance = float.MaxValue;

                foreach (Node neighbor in neighbors)
                {
                    float distance = Vector2.Distance(neighbor.worldPosition, playerPosition2D);
                    if (distance < shortestDistance)
                    {
                        closestNode = neighbor;
                        shortestDistance = distance;
                    }
                }

                if (closestNode != null && closestNode.worldPosition != playerPosition2D)
                {
                    targetNode = closestNode;
                }
            }
        }

        if (Vector2.Distance(transform.position, playerTransform.position) <= battleRange)
        {
            currentState = EnemyState.Battle;
        }
    }

    private void FleeFromPlayer()
    {
        if (Vector2.Distance(transform.position, playerTransform.position) < fleeRange)
        {
            Vector2 fleeDirection = (transform.position - playerTransform.position).normalized;
            transform.position += (Vector3)fleeDirection * Time.deltaTime * moveSpeed;
        }
    }

    private IEnumerator AutoAttack()
    {
        while (currentState == EnemyState.Battle && currentHealth > 0 && playerCombat.currentHealth > 0)
        {
            if (Vector2.Distance(transform.position, playerTransform.position) <= battleRange)
            {
                playerCombat.TakeDamage(attackPower); // Deal damage to the player
                yield return new WaitForSeconds(attackInterval); // Wait between attacks
            }
            else
            {
                break; // Stop attacking if the player is out of range
            }
        }

        attackCoroutine = null;
    }

    public void TakeDamage(int damage)
    {
        // Extensive logging for damage reception
        Debug.Log("==================== ENEMY DAMAGE RECEIVED ====================");
        Debug.Log($"Current Health BEFORE damage: {currentHealth}");
        Debug.Log($"Damage Received: {damage}");

        // Additional safety checks
        if (damage < 0)
        {
            Debug.LogWarning("Negative damage received! Ignoring.");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update health bar
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
        else
        {
            Debug.LogError("Health Bar is not assigned!");
        }

        Debug.Log($"Current Health AFTER damage: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("Enemy DEFEATED!");
            Destroy(gameObject);
        }
        else if (currentHealth <= maxHealth * lowHealthThreshold)
        {
            Debug.Log("Enemy entering DEFENSIVE state due to low health");
            currentState = EnemyState.Defensive;
        }

        Debug.Log("============================================================");
    }
    private void Update()
    {
        if (Vector2.Distance(transform.position, playerTransform.position) <= detectionRange)
        {
            if (currentState != EnemyState.Aggressive && currentState != EnemyState.Battle)
            {
                currentState = EnemyState.Aggressive; // Switch to aggressive state when player is detected
            }
        }
        else if (currentState != EnemyState.Patrol)
        {
            currentState = EnemyState.Patrol; // Return to patrol state when player is out of range
        }

        if (Vector2.Distance(transform.position, playerTransform.position) <= battleRange)
        {
            currentState = EnemyState.Battle; // Switch to battle state when close enough to player
        }
    }
}
