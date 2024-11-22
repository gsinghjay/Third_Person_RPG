public abstract class AnimationStateBase : IAnimationState
{
    protected readonly PlayerAnimator playerAnimator;

    protected AnimationStateBase(PlayerAnimator animator)
    {
        GameLogger.LogState($"Initializing {GetType().Name}");
        playerAnimator = animator;
    }

    public virtual void Enter()
    {
        GameLogger.LogState($"Entering {GetType().Name}");
    }

    public virtual void HandleInput()
    {
        // Base input handling if needed
    }

    public virtual void UpdateState()
    {
        // Base update logic if needed
    }

    public virtual void Exit()
    {
        GameLogger.LogState($"Exiting {GetType().Name}");
    }

    protected void TransitionToState(IAnimationState newState)
    {
        GameLogger.LogState($"{GetType().Name} - Transitioning to {newState.GetType().Name}");
        playerAnimator.ChangeState(newState);
    }
}