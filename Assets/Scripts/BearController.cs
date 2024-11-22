using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class BearController : MonoBehaviour
{
    [System.Serializable]
    private class BearAnimation
    {
        public string name;
        public AnimationType type;
        public int hash; // Store the hashed ID

        public BearAnimation(string name, AnimationType type)
        {
            this.name = name;
            this.type = type;
            this.hash = Animator.StringToHash(name);
        }
    }

    private enum AnimationType
    {
        Trigger,
        Bool
    }

    // Replace individual hash fields with organized collection
    private readonly BearAnimation[] bearAnimations = new[]
    {
        new BearAnimation("Idle", AnimationType.Trigger),
        new BearAnimation("Run Forward", AnimationType.Trigger),
        new BearAnimation("Attack1", AnimationType.Trigger),
        new BearAnimation("Attack2", AnimationType.Trigger),
        new BearAnimation("Sleep", AnimationType.Trigger),
        new BearAnimation("Get Hit Front", AnimationType.Trigger)
    };
    
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

    [Header("Spawn Settings")]
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool isReturningToSpawn;
    
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
        
        // Store spawn position and rotation
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        
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

    private void PlayAnimation(string animationName)
    {
        var animation = System.Array.Find(bearAnimations, a => a.name == animationName);
        if (animation == null) return;

        ResetAllAnimations();

        if (animation.type == AnimationType.Bool)
        {
            animator.SetBool(animation.hash, true);
        }
        else
        {
            animator.SetTrigger(animation.hash);
        }
    }

    private void ResetAllAnimations()
    {
        foreach (var animation in bearAnimations)
        {
            if (animation.type == AnimationType.Bool)
            {
                animator.SetBool(animation.hash, false);
            }
            else
            {
                animator.ResetTrigger(animation.hash);
            }
        }
    }

    private void ChangeState(BearState newState)
    {
        GameLogger.LogState($"Bear changing state from {currentState} to {newState}");
        currentState = newState;
        stateEnterTime = Time.time;
        
        switch (newState)
        {
            case BearState.Sleeping:
                PlayAnimation("Sleep");
                agent.isStopped = true;
                agent.speed = patrolMovementSpeed;
                break;
                
            case BearState.WakingUp:
                PlayAnimation("Idle");
                agent.isStopped = true;
                break;
                
            case BearState.Combat:
                agent.speed = combatMovementSpeed;
                agent.isStopped = false;
                break;
                
            case BearState.ReturningToSpawn:
                PlayAnimation("Run Forward");
                agent.speed = patrolMovementSpeed;
                agent.isStopped = false;
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
                
            case BearState.ReturningToSpawn:
                UpdateReturningToSpawnState();
                break;
        }
    }
    
    private void UpdateCombatState()
    {
        if (distanceToPlayer > detectionRange)
        {
            GameLogger.LogCombat($"Bear leaving combat - distance to player: {distanceToPlayer:F2}");
            ChangeState(BearState.ReturningToSpawn);
            return;
        }
    
        targetPosition = playerTransform.position;
        
        // Only update rotation if we're not too close to avoid jittering
        if (distanceToPlayer > stoppingDistance)
        {
            agent.SetDestination(targetPosition);
            agent.isStopped = false;
            PlayAnimation("Run Forward");
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
            PlayAnimation(Random.value > 0.5f ? "Attack1" : "Attack2");
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void UpdateReturningToSpawnState()
    {
        float distanceToSpawn = Vector3.Distance(transform.position, spawnPosition);
        
        if (distanceToSpawn <= stoppingDistance)
        {
            // We've reached the spawn point
            transform.rotation = spawnRotation;
            ChangeState(BearState.Sleeping);
            return;
        }

        // Check if player comes back in range while returning
        if (distanceToPlayer <= detectionRange)
        {
            ChangeState(BearState.Combat);
            return;
        }

        // Move towards spawn point
        agent.SetDestination(spawnPosition);
        agent.isStopped = false;
        PlayAnimation("Run Forward");
    }
    
    public void TakeDamage()
    {
        PlayAnimation("Get Hit Front");
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
    Combat,
    ReturningToSpawn
}