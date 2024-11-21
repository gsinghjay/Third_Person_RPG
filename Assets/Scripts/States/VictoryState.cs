using UnityEngine;

public class VictoryState : IAnimationState
{
    private PlayerAnimator playerAnimator;

    public VictoryState(PlayerAnimator animator)
    {
        playerAnimator = animator;
    }

    public void Enter()
    {
        playerAnimator.SetAnimationState(AnimationState.Victory);
    }

    public void HandleInput()
    {
        // Typically, Victory state doesn't transition unless resetting
    }

    public void UpdateState()
    {
        // Victory-specific update logic if needed
    }

    public void Exit()
    {
        // Cleanup when exiting Victory State
    }
}