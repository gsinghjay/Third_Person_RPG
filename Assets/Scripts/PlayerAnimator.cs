using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    private bool isDead = false;
    private bool isVictorious = false;

    // Public properties for state checking
    public bool IsDead => isDead;
    public bool IsVictorious => isVictorious;

    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int victoryHash = Animator.StringToHash("Victory");
    private readonly int dieHash = Animator.StringToHash("Die");
    private readonly int moveSpeedHash = Animator.StringToHash("MoveSpeed");

    // Add new parameter hashes
    private readonly int isJumpingHash = Animator.StringToHash("IsJumping");
    private readonly int jumpHash = Animator.StringToHash("Jump");
    private readonly int doubleJumpHash = Animator.StringToHash("DoubleJump");
    private readonly int landHash = Animator.StringToHash("Land");
    private bool isJumping = false;

    public void TriggerJump()
    {
        if (!isDead && !isVictorious)
        {
            isJumping = true;
            animator.SetBool(isJumpingHash, true);
            animator.SetTrigger(jumpHash);
        }
    }

    public void TriggerDoubleJump()
    {
        if (!isDead && !isVictorious && isJumping)
        {
            animator.SetTrigger(doubleJumpHash);
        }
    }

    public void TriggerLand()
    {
        if (isJumping)
        {
            isJumping = false;
            animator.SetBool(isJumpingHash, false);
            animator.SetTrigger(landHash);
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator component missing on " + gameObject.name);
            
        // Get Rigidbody from parent (Player object)
        rb = GetComponentInParent<Rigidbody>();
        if (rb == null)
            Debug.LogError("Rigidbody component missing on parent of " + gameObject.name);
    }

    private void OnAnimatorMove()
    {
        if (!animator || isDead || isVictorious || !rb) return;

        // Get the root motion delta from the animator
        Vector3 velocity = animator.deltaPosition;
        
        // Preserve the Y velocity for jumps/gravity
        velocity.y = rb.velocity.y;
        
        // Apply the velocity
        if (animator.deltaPosition.magnitude > 0)
        {
            rb.velocity = velocity / Time.deltaTime;
        }

        // Apply rotation if we have any rotation delta
        if (animator.deltaRotation != Quaternion.identity)
        {
            rb.MoveRotation(rb.rotation * animator.deltaRotation);
        }
    }

    public void SetIsMoving(bool isMoving)
    {
        if (!isDead && !isVictorious)
        {
            animator.SetBool(isMovingHash, isMoving);
            // Debug log to verify the animation state changes
            Debug.Log($"Setting IsMoving to: {isMoving}");
        }
    }

    public void SetMovementSpeed(float speed)
    {
        if (!isDead && !isVictorious)
        {
            animator.SetFloat(moveSpeedHash, speed);
        }
    }

    public void TriggerVictory()
    {
        if (!isDead && !isVictorious)
        {
            isVictorious = true;
            animator.SetTrigger(victoryHash);
        }
    }

    public void TriggerDie()
    {
        if (!isDead && !isVictorious)
        {
            isDead = true;
            animator.SetTrigger(dieHash);
        }
    }

    // Optional: Method to reset states if needed
    public void ResetStates()
    {
        isDead = false;
        isVictorious = false;
    }
}