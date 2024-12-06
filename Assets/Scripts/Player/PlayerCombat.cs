using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthBar;
    public int attackDamage = 10;
    public float attackInterval = 1.0f; // Time between each auto-attack

    private bool isInAttackRange = false;
    private bool isAttacking = false; // Tracks if auto-attack is active
    private EnemyAI targetEnemy;

    public Button attackButton; // UI button to start/stop attacking
    public Button defendButton; // UI button for defense

    private bool isDefending = false; // Track if the player is in the defensive state
    public float defendDuration = 2.0f; // Duration of invulnerability
    public float defendCooldown = 5.0f; // Cooldown period for the defense
    private bool canDefend = true; // To check if defense is off cooldown

    private SpriteRenderer spriteRenderer; // Reference to the player's SpriteRenderer
    public Color defendColor = Color.blue; // Color to change the player to during defense
    public Color attackColor = Color.red; // Color to change the player to during attack
    public Color defaultColor = Color.green; // Default color for the player (green)

    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;

        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set up the Attack button to call StartAutoAttack when clicked
        attackButton.onClick.AddListener(ToggleAutoAttack);
        attackButton.interactable = false; // Disable the button initially

        // Set up the Defend button
        defendButton.onClick.AddListener(ToggleDefend);
        defendButton.interactable = true; // Enable defend button

        FindNearestEnemy();
    }

    private void FindNearestEnemy()
    {
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        float closestDistance = float.MaxValue;
        EnemyAI closestEnemy = null;

        foreach (EnemyAI enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy != null)
        {
            targetEnemy = closestEnemy;
            isInAttackRange = true;
            attackButton.interactable = true;
            Debug.Log($"Nearest enemy found: {targetEnemy}, Distance: {closestDistance}");
        }
    }

    private void ToggleAutoAttack()
    {
        if (isAttacking)
        {
            Debug.Log("Stopping auto-attack.");
            isAttacking = false;
            StopCoroutine(AutoAttack());
            spriteRenderer.color = defaultColor;
        }
        else
        {
            Debug.Log("Starting auto-attack.");
            isAttacking = true;
            StartCoroutine(AutoAttack());
            spriteRenderer.color = attackColor;
        }
    }

    private IEnumerator AutoAttack()
    {
        Debug.Log("AutoAttack Coroutine STARTED");
        Debug.Log($"Initial Conditions - isAttacking: {isAttacking}, targetEnemy: {targetEnemy}");

        while (isAttacking && targetEnemy != null)
        {
            Debug.Log($"Inside AutoAttack Loop - isAttacking: {isAttacking}, targetEnemy: {targetEnemy}");

            if (targetEnemy == null)
            {
                Debug.LogError("Target Enemy became NULL inside the loop!");
                break;
            }

            float attackRange = 1.5f;
            float distanceToEnemy = Vector2.Distance(transform.position, targetEnemy.transform.position);

            Debug.Log($"Distance to Enemy: {distanceToEnemy}, Attack Range: {attackRange}");

            if (distanceToEnemy <= attackRange)
            {
                Debug.Log("Enemy in Attack Range - Attempting Damage");
                targetEnemy.TakeDamage(attackDamage);
            }
            else
            {
                Debug.Log($"Enemy out of range by {distanceToEnemy - attackRange} units");
            }

            yield return new WaitForSeconds(attackInterval);
        }

        Debug.Log("AutoAttack Coroutine ENDED");
        Debug.Log($"Final Conditions - isAttacking: {isAttacking}, targetEnemy: {targetEnemy}");

        // Reset attacking state
        isAttacking = false;
        spriteRenderer.color = defaultColor;
    }

    // Defend command - makes player invulnerable for a short time
    private void ToggleDefend()
    {
        if (canDefend && !isDefending)
        {
            StartCoroutine(DefendCoroutine());
        }
    }

    private IEnumerator DefendCoroutine()
    {
        isDefending = true;
        canDefend = false;

        // Set the player to invulnerable
        Debug.Log("Defending: Player is now invulnerable!");

        // Change the player's sprite color to indicate defense mode
        spriteRenderer.color = defendColor;

        // Wait for the defend duration (invulnerability period)
        yield return new WaitForSeconds(defendDuration);

        // End the defensive state
        isDefending = false;

        // Reset the player's sprite color back to default (green) after defense ends
        spriteRenderer.color = defaultColor;

        // After defend is finished, start cooldown
        Debug.Log("Defend finished, cooldown started.");
        yield return new WaitForSeconds(defendCooldown);

        // Reset the cooldown
        canDefend = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            isInAttackRange = true;
            targetEnemy = other.GetComponent<EnemyAI>(); // Get the enemy script reference
            attackButton.interactable = true; // Enable the attack button
            Debug.Log("Enemy in range for attack: " + targetEnemy); // Log the assigned target enemy
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        // Stop attacking and disable the attack button when exiting the enemy's range
        if (other.CompareTag("Enemy"))
        {
            isInAttackRange = false;
            targetEnemy = null;
            isAttacking = false;
            StopCoroutine(AutoAttack());
            attackButton.interactable = false; // Disable the attack button
            spriteRenderer.color = defaultColor;
            Debug.Log("Enemy left attack range");
        }
    }

    public void TakeDamage(int damage)
    {
        // Only take damage if not defending
        if (!isDefending)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            healthBar.value = currentHealth;

            if (currentHealth <= 0)
            {
                Debug.Log("Player defeated!");
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("Player is defending and took no damage!");
        }
    }
}
