using UnityEngine;
using UnityEngine.AI;

public class BearHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    
    private float currentHealth;
    private Animator animator;
    private NavMeshAgent agent;
    private BearController bearController;
    private bool isDead;
    
    private static readonly int HASH_DEATH = Animator.StringToHash("Die");
    private static readonly int HASH_HIT = Animator.StringToHash("Get Hit Front");
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        bearController = GetComponent<BearController>();
        currentHealth = maxHealth;
    }
    
    private void Die()
    {
        animator.SetTrigger(HASH_DEATH);
        
        // Disable NavMesh and controller
        agent.enabled = false;
        bearController.enabled = false;
        
        // Optional: Add NavMeshObstacle to prevent other bears from walking through
        NavMeshObstacle obstacle = gameObject.AddComponent<NavMeshObstacle>();
        obstacle.carving = true;
        obstacle.shape = NavMeshObstacleShape.Box;
        obstacle.size = new Vector3(1f, 1f, 1f);
        
        Destroy(gameObject, 5f);
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            GameLogger.LogCombat("Damage ignored - bear is already dead");
            return;
        }
        
        currentHealth -= damage;
        GameLogger.LogCombat($"Bear took {damage} damage. Current health: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            isDead = true;
            GameLogger.LogCombat("Bear died");
            Die();
        }
        else
        {
            animator.SetTrigger(HASH_HIT);
        }
    }
}