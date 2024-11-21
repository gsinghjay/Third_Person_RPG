using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class BearController : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float sleepingDetectionRange = 5f;
    
    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float combatMovementSpeed = 5f;
    [SerializeField] private float patrolMovementSpeed = 3f;
    
    [Header("References")]
    [SerializeField] private Transform player;
    
    private NavMeshAgent agent;
    private Animator animator;
    private BearState currentState;
    private float distanceToPlayer;
    private float nextAttackTime;
    private float stateEnterTime;
    
    // Animation parameter names from the existing animator
    private static readonly int IdleTrigger = Animator.StringToHash("Idle");
    private static readonly int RunForwardTrigger = Animator.StringToHash("Run Forward");
    private static readonly int Attack1Trigger = Animator.StringToHash("Attack1");
    private static readonly int Attack2Trigger = Animator.StringToHash("Attack2");
    private static readonly int SleepTrigger = Animator.StringToHash("Sleep");
    private static readonly int GetHitTrigger = Animator.StringToHash("Get Hit Front");
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Add or configure capsule collider
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            capsule = gameObject.AddComponent<CapsuleCollider>();
        }
        capsule.center = new Vector3(0, 1f, 0);
        capsule.radius = 0.5f;
        capsule.height = 2f;
        
        // Add Rigidbody for collisions
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        
        // Find player if not assigned
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
            
        // Initialize state
        ChangeState(BearState.Sleeping);
        
        // Configure NavMeshAgent
        agent.speed = patrolMovementSpeed;
        agent.stoppingDistance = attackRange * 0.8f;
    }
    
    private void Update()
    {
        if (player == null) return;
        
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        UpdateBearState();
    }
    
    private void UpdateBearState()
    {
        switch (currentState)
        {
            case BearState.Sleeping:
                if (distanceToPlayer <= sleepingDetectionRange)
                {
                    Debug.Log($"Bear detected player at distance: {distanceToPlayer}. Waking up!");
                    ChangeState(BearState.WakingUp);
                }
                break;
                
            case BearState.WakingUp:
                // Force transition to combat after animation
                if (Time.time - stateEnterTime > 1.5f)
                {
                    ChangeState(BearState.Combat);
                }
                break;
                
            case BearState.Combat:
                UpdateCombatState();
                break;
        }
    }
    
    private void UpdateCombatState()
    {
        // Look at player
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, 
            Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            
        if (distanceToPlayer > detectionRange)
        {
            ResetAllTriggers();
            ChangeState(BearState.Sleeping);
        }
        else if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            ResetAllTriggers();
            // Randomly choose between Attack1 and Attack2
            animator.SetTrigger(Random.value > 0.5f ? Attack1Trigger : Attack2Trigger);
            nextAttackTime = Time.time + attackCooldown;
        }
        else if (distanceToPlayer > attackRange)
        {
            ResetAllTriggers();
            animator.SetTrigger(RunForwardTrigger);
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }
    
    private void ChangeState(BearState newState)
    {
        Debug.Log($"Bear changing state from {currentState} to {newState}");
        currentState = newState;
        stateEnterTime = Time.time;
        
        switch (newState)
        {
            case BearState.Sleeping:
                agent.isStopped = true;
                ResetAllTriggers();
                animator.SetTrigger(SleepTrigger);
                agent.speed = patrolMovementSpeed;
                break;
                
            case BearState.WakingUp:
                agent.isStopped = true;
                ResetAllTriggers();
                animator.SetTrigger(IdleTrigger);
                break;
                
            case BearState.Combat:
                agent.isStopped = false;
                ResetAllTriggers();
                animator.SetTrigger(RunForwardTrigger);
                agent.speed = combatMovementSpeed;
                break;
        }
    }
    
    private void ResetAllTriggers()
    {
        animator.ResetTrigger(IdleTrigger);
        animator.ResetTrigger(RunForwardTrigger);
        animator.ResetTrigger(Attack1Trigger);
        animator.ResetTrigger(Attack2Trigger);
        animator.ResetTrigger(SleepTrigger);
        animator.ResetTrigger(GetHitTrigger);
    }
    
    public void TakeDamage()
    {
        animator.SetTrigger(GetHitTrigger);
        if (currentState == BearState.Sleeping)
        {
            ChangeState(BearState.WakingUp);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sleepingDetectionRange);
    }
}

public enum BearState
{
    Sleeping,
    WakingUp,
    Combat
}