using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    private bool isDead = false;
    private bool isVictorious = false;

    // Animation parameter hashes
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int victoryHash = Animator.StringToHash("Victory");
    private readonly int dieHash = Animator.StringToHash("Die");
    private readonly int moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private readonly int isJumpingHash = Animator.StringToHash("IsJumping");
    private readonly int jumpHash = Animator.StringToHash("Jump");
    private readonly int landHash = Animator.StringToHash("Land");

    private bool isJumping = false;
    
    // Public properties
    public bool IsDead => isDead;
    public bool IsVictorious => isVictorious;
    public bool IsJumping => isJumping;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponentInParent<Rigidbody>();
    }

    private void OnAnimatorMove()
    {
        if (!animator || isDead || isVictorious || !rb) return;

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

    public void TriggerJump()
    {
        if (!isDead && !isVictorious)
        {
            isJumping = true;
            animator.SetBool(isJumpingHash, true);
            animator.SetTrigger(jumpHash);
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

    public void SetIsMoving(bool isMoving)
    {
        if (!isDead && !isVictorious)
        {
            animator.SetBool(isMovingHash, isMoving);
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
}