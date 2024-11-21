using UnityEngine;

public class BearHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private Animator animator;
    private UnityEngine.AI.NavMeshAgent agent;
    private BearController bearController;
    
    private static readonly int DeathTrigger = Animator.StringToHash("Die");
    private static readonly int GetHitTrigger = Animator.StringToHash("Get Hit");

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        bearController = GetComponent<BearController>();
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;
        
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger(GetHitTrigger);
        }
    }

    private void Die()
    {
        animator.SetTrigger(DeathTrigger);
        
        // Disable components
        GetComponent<Collider>().enabled = false;
        agent.enabled = false;
        bearController.enabled = false;
        
        // Optional: Destroy after delay
        Destroy(gameObject, 5f);
    }
}