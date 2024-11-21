using UnityEngine;

public class MoveState : IAnimationState
{
    private PlayerAnimator playerAnimator;

    public MoveState(PlayerAnimator animator)
    {
        playerAnimator = animator;
    }

    public void Enter()
    {
        playerAnimator.SetIsMoving(true);
    }

    public void HandleInput()
    {
        if (!playerAnimator.IsMoving)
        {
            playerAnimator.ChangeState(new IdleState(playerAnimator));
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
        playerAnimator.SetMovementSpeed(playerAnimator.CurrentSpeed);
    }

    public void Exit()
    {
        // Cleanup when exiting Move State
    }
}