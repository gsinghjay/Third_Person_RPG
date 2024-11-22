public class DieState : IAnimationState
{
    private PlayerAnimator playerAnimator;

    public DieState(PlayerAnimator animator)
    {
        GameLogger.LogState("Initializing DieState");
        playerAnimator = animator;
    }

    public void Enter()
    {
        GameLogger.LogState("Entering DieState");
        GameLogger.LogAnimation("Playing death animation");
        playerAnimator.SetAnimationState(AnimationState.Die);
    }

    public void HandleInput()
    {
        // No transitions from death state
        GameLogger.LogState("DieState - ignoring input handling", LogType.Log);
    }

    public void UpdateState()
    {
        // No updates needed in death state
    }

    public void Exit()
    {
        GameLogger.LogState("Exiting DieState - This shouldn't happen!", LogType.Warning);
    }
}