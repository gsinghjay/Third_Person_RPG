using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    // Animation parameter hashes
    private readonly int stateHash = Animator.StringToHash("State");
    private readonly int moveSpeedHash = Animator.StringToHash("MoveSpeed");

    private bool isDead = false;
    private bool isVictorious = false;
    private bool isJumping = false;

    // Public properties
    public bool IsDead => isDead;
    public bool IsVictorious => isVictorious;
    public bool IsJumping => isJumping;
    public bool IsMoving { get; private set; }
    public float CurrentSpeed { get; set; } = 1f;

    // State management
    private IAnimationState currentState;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponentInParent<Rigidbody>();
        animator.applyRootMotion = false;
        
        // Store original transform data
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        
        ChangeState(new IdleState(this));
    }

    private void Update()
    {
        currentState.HandleInput();
        currentState.UpdateState();
    }

    private void LateUpdate()
    {
        // Keep local transform fixed during jump
        if (isJumping)
        {
            transform.localPosition = originalLocalPosition;
            transform.localRotation = originalLocalRotation;
        }
    }

    private void OnAnimatorMove()
    {
        if (!animator || isDead || isVictorious || !rb) return;

        // Only apply root motion for non-jumping animations
        if (!isJumping)
        {
            Vector3 velocity = animator.deltaPosition;
            velocity.y = rb.velocity.y;

            if (animator.deltaPosition.magnitude > 0)
            {
                rb.velocity = velocity / Time.deltaTime;
            }

            if (animator.deltaRotation != Quaternion.identity)
            {
                rb.MoveRotation(rb.rotation * animator.deltaRotation);
            }
        }
    }

    public void ChangeState(IAnimationState newState)
    {
        if (currentState != null && currentState.GetType() == newState.GetType())
            return; // Prevent changing to the same state

        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    public void TriggerJump()
    {
        if (!isDead && !isVictorious)
        {
            isJumping = true;
            SetAnimationState(AnimationState.Jumping);
            ChangeState(new JumpState(this));
        }
    }

    public void TriggerLand()
    {
        if (isJumping)
        {
            isJumping = false;
            // Reset local transform
            transform.localPosition = originalLocalPosition;
            transform.localRotation = originalLocalRotation;
            
            SetAnimationState(AnimationState.Idle);
            ChangeState(new IdleState(this));
        }
    }

    public void SetIsMoving(bool isMoving)
    {
        if (!isDead && !isVictorious)
        {
            IsMoving = isMoving;
            if (isMoving)
            {
                SetAnimationState(AnimationState.Moving);
                ChangeState(new MoveState(this));
            }
            else
            {
                SetAnimationState(AnimationState.Idle);
                ChangeState(new IdleState(this));
            }
        }
    }

    public void SetMovementSpeed(float speed)
    {
        if (!isDead && !isVictorious)
        {
            CurrentSpeed = speed;
            animator.SetFloat(moveSpeedHash, speed);
        }
    }

    public void TriggerVictory()
    {
        if (!isDead && !isVictorious)
        {
            isVictorious = true;
            SetAnimationState(AnimationState.Victory);
            ChangeState(new VictoryState(this));
        }
    }

    public void TriggerDie()
    {
        if (!isDead && !isVictorious)
        {
            isDead = true;
            SetAnimationState(AnimationState.Die);
            ChangeState(new DieState(this));
        }
    }

    public void SetAnimationState(AnimationState state)
    {
        animator.SetInteger(stateHash, (int)state);
    }
}

public enum AnimationState
{
    Idle = 0,
    Moving = 1,
    Jumping = 2,
    Victory = 3,
    Die = 4
}