using UnityEngine;

public class JumpState : IAnimationState
{
    private PlayerAnimator playerAnimator;
    private PlayerMovement playerMovement;

    public JumpState(PlayerAnimator animator)
    {
        Debug.Log("Entering JumpState constructor");
        playerAnimator = animator;
        playerMovement = animator.GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement component not found!");
        }
    }

    public void Enter()
    {
        GameLogger.LogState("Entering JumpState");
        playerAnimator.SetAnimationState(AnimationState.Jumping);
    }

    public void HandleInput()
    {
        if (playerMovement == null)
        {
            Debug.LogError("HandleInput - playerMovement is null");
            return;
        }

        // Check if we've landed
        if (playerMovement.IsGrounded())
        {
            Debug.Log("JumpState HandleInput - Player has landed");
            playerAnimator.TriggerLand();  // Reset IsJumping
            
            // Transition to appropriate state based on movement
            if (playerAnimator.IsMoving)
            {
                playerAnimator.ChangeState(new MoveState(playerAnimator));
            }
            else
            {
                playerAnimator.ChangeState(new IdleState(playerAnimator));
            }

            if (playerAnimator.IsDead)
            {
                Debug.Log("JumpState HandleInput - Transitioning to DieState");
                playerAnimator.ChangeState(new DieState(playerAnimator));
            }
            else if (playerAnimator.IsVictorious)
            {
                Debug.Log("JumpState HandleInput - Transitioning to VictoryState");
                playerAnimator.ChangeState(new VictoryState(playerAnimator));
            }
            return;
        }
    }
    public void UpdateState()
    {
        // Update movement speed during jump
        if (playerAnimator.IsMoving)
        {
            playerAnimator.SetMovementSpeed(playerAnimator.CurrentSpeed);
        }
    }

    public void Exit()
    {
        GameLogger.LogState("Exiting JumpState");
    }
}