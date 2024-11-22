public class VictoryState : IAnimationState
{
    private PlayerAnimator playerAnimator;

    public VictoryState(PlayerAnimator animator)
    {
        GameLogger.LogState("Initializing VictoryState");
        playerAnimator = animator;
    }

    public void Enter()
    {
        GameLogger.LogState("Entering VictoryState");
        GameLogger.LogAnimation("Playing victory animation");
        playerAnimator.SetAnimationState(AnimationState.Victory);
    }

    public void HandleInput()
    {
        // Victory state doesn't transition unless resetting
        GameLogger.LogState("VictoryState - ignoring input handling", LogType.Log);
    }

    public void UpdateState()
    {
        // Victory-specific update logic if needed
    }

    public void Exit()
    {
        GameLogger.LogState("Exiting VictoryState");
    }
}