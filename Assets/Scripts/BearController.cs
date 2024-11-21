using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class BearController : MonoBehaviour
{
    // Cache string hashes for better WebGL performance
    private static readonly int HASH_IDLE = Animator.StringToHash("Idle");
    private static readonly int HASH_RUN = Animator.StringToHash("Run Forward");
    private static readonly int HASH_ATTACK1 = Animator.StringToHash("Attack1");
    private static readonly int HASH_ATTACK2 = Animator.StringToHash("Attack2");
    private static readonly int HASH_SLEEP = Animator.StringToHash("Sleep");
    private static readonly int HASH_HIT = Animator.StringToHash("Get Hit Front");
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float sleepingDetectionRange = 5f;
    
    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float combatMovementSpeed = 5f;
    [SerializeField] private float patrolMovementSpeed = 3f;
    
    [Header("NavMesh Settings")]
    [SerializeField] private float avoidanceRadius = 1f;
    [SerializeField] private float stoppingDistance = 1.5f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float angularSpeed = 120f;
    
    // Cache components for better performance
    private NavMeshAgent agent;
    private Animator animator;
    private Transform playerTransform;
    private Transform bearTransform;
    private Vector3 targetPosition;
    private BearState currentState;
    private float distanceToPlayer;
    private float nextAttackTime;
    private float stateEnterTime;
    private bool isInitialized;

    private void Awake()
    {
        // Cache components in Awake for better initialization
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        bearTransform = transform;
        
        ConfigureNavMeshAgent();
    }
    
    private void Start()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }
    
    private void Initialize()
    {
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            
        agent.speed = patrolMovementSpeed;
        agent.stoppingDistance = attackRange * 0.8f;
        
        ChangeState(BearState.Sleeping);
        isInitialized = true;
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
                animator.SetTrigger(HASH_SLEEP);
                agent.speed = patrolMovementSpeed;
                break;
                
            case BearState.WakingUp:
                agent.isStopped = true;
                ResetAllTriggers();
                animator.SetTrigger(HASH_IDLE);
                break;
                
            case BearState.Combat:
                agent.isStopped = false;
                ResetAllTriggers();
                animator.SetTrigger(HASH_RUN);
                agent.speed = combatMovementSpeed;
                break;
        }
    }
    
    private void ConfigureNavMeshAgent()
    {
        if (agent != null)
        {
            agent.radius = avoidanceRadius;
            agent.stoppingDistance = stoppingDistance;
            agent.acceleration = acceleration;
            agent.angularSpeed = angularSpeed;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = Random.Range(0, 99); // Randomize priority to prevent deadlocks
            agent.autoTraverseOffMeshLink = true;
        }
    }

    private void Update()
    {
        if (!isInitialized || playerTransform == null) return;
        
        distanceToPlayer = Vector3.Distance(bearTransform.position, playerTransform.position);
        UpdateBearState();
    }
    
    private void UpdateBearState()
    {
        switch (currentState)
        {
            case BearState.Sleeping:
                if (distanceToPlayer <= sleepingDetectionRange)
                {
                    ChangeState(BearState.WakingUp);
                }
                break;
                
            case BearState.WakingUp:
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
        if (distanceToPlayer > detectionRange)
        {
            ResetAllTriggers();
            ChangeState(BearState.Sleeping);
            return;
        }
        
        targetPosition = playerTransform.position;
        
        // Only update rotation if we're not too close to avoid jittering
        if (distanceToPlayer > stoppingDistance)
        {
            agent.SetDestination(targetPosition);
            agent.isStopped = false;
            animator.SetTrigger(HASH_RUN);
        }
        else
        {
            agent.isStopped = true;
            
            // Look at target
            Vector3 direction = (targetPosition - bearTransform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                bearTransform.rotation = Quaternion.Slerp(bearTransform.rotation, lookRotation, Time.deltaTime * angularSpeed);
            }
        }
        
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            ResetAllTriggers();
            animator.SetTrigger(Random.value > 0.5f ? HASH_ATTACK1 : HASH_ATTACK2);
            nextAttackTime = Time.time + attackCooldown;
        }
    }
    
    private void ResetAllTriggers()
    {
        animator.ResetTrigger(HASH_IDLE);
        animator.ResetTrigger(HASH_RUN);
        animator.ResetTrigger(HASH_ATTACK1);
        animator.ResetTrigger(HASH_ATTACK2);
        animator.ResetTrigger(HASH_SLEEP);
        animator.ResetTrigger(HASH_HIT);
    }
    
    public void TakeDamage()
    {
        animator.SetTrigger(HASH_HIT);
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