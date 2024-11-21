using UnityEngine;
using UnityEngine.AI;
public class BearHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    
    private float currentHealth;
    private Animator animator;
    private NavMeshAgent agent;
    private BearController bearController;
    private CapsuleCollider bearCollider;
    private bool isDead;
    
    // Cache hash IDs
    private static readonly int HASH_DEATH = Animator.StringToHash("Die");
    private static readonly int HASH_HIT = Animator.StringToHash("Get Hit Front");
    
    private void Awake()
    {
        // Cache all components at startup
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        bearController = GetComponent<BearController>();
        bearCollider = GetComponent<CapsuleCollider>();
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            isDead = true;
            Die();
        }
        else
        {
            animator.SetTrigger(HASH_HIT);
        }
    }
    
    private void Die()
    {
        animator.SetTrigger(HASH_DEATH);
        
        // Disable components
        bearCollider.enabled = false;
        agent.enabled = false;
        bearController.enabled = false;
        
        // Optional: Destroy after delay
        Destroy(gameObject, 5f);
    }
}