using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private bool isDead = false;
    private bool isVictorious = false;

    // Animator parameter hashes for performance
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int victoryHash = Animator.StringToHash("Victory");
    private readonly int dieHash = Animator.StringToHash("Die");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetIsMoving(bool isMoving)
    {
        // Only update movement if we're not dead or victorious
        if (!isDead && !isVictorious)
        {
            animator.SetBool(isMovingHash, isMoving);
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