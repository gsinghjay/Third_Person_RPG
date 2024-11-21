using UnityEngine;

public class DieState : IAnimationState
{
    private PlayerAnimator playerAnimator;

    public DieState(PlayerAnimator animator)
    {
        playerAnimator = animator;
    }

    public void Enter()
    {
        playerAnimator.SetAnimationState(AnimationState.Die);
    }

    public void HandleInput()
    {
        // Typically, Die state doesn't transition unless restarting
    }

    public void UpdateState()
    {
        // Die-specific update logic if needed
    }

    public void Exit()
    {
        // Cleanup when exiting Die State
    }
}