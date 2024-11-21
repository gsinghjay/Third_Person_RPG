using UnityEngine;

public class IdleState : IAnimationState
{
    private PlayerAnimator playerAnimator;

    public IdleState(PlayerAnimator animator)
    {
        playerAnimator = animator;
    }

    public void Enter()
    {
        // Since we're already entering IdleState, no need to set it again
        playerAnimator.SetIsMoving(false);
    }

    public void HandleInput()
    {
        if (playerAnimator.IsMoving)
        {
            playerAnimator.ChangeState(new MoveState(playerAnimator));
        }
        else if (playerAnimator.IsJumping)
        {
            playerAnimator.ChangeState(new JumpState(playerAnimator));
        }
        else if (playerAnimator.IsVictorious)
        {
            playerAnimator.ChangeState(new VictoryState(playerAnimator));
        }
        else if (playerAnimator.IsDead)
        {
            playerAnimator.ChangeState(new DieState(playerAnimator));
        }
    }

    public void UpdateState()
    {
        // Idle-specific update logic if needed
    }

    public void Exit()
    {
        // Cleanup when exiting Idle State
    }
}