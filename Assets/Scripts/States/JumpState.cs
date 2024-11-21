using UnityEngine;

public class JumpState : IAnimationState
{
    private PlayerAnimator playerAnimator;

    public JumpState(PlayerAnimator animator)
    {
        playerAnimator = animator;
    }

    public void Enter()
    {
        playerAnimator.SetAnimationState(AnimationState.Jumping);
    }

    public void HandleInput()
    {
        if (!playerAnimator.IsJumping)
        {
            playerAnimator.ChangeState(new IdleState(playerAnimator));
        }
        else if (playerAnimator.IsDead)
        {
            playerAnimator.ChangeState(new DieState(playerAnimator));
        }
        else if (playerAnimator.IsVictorious)
        {
            playerAnimator.ChangeState(new VictoryState(playerAnimator));
        }
    }

    public void UpdateState()
    {
        // Jump-specific update logic if needed
    }

    public void Exit()
    {
        // Cleanup when exiting Jump State
    }
}