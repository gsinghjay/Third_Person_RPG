public class DieState : AnimationStateBase
{
    public DieState(PlayerAnimator animator) : base(animator) { }

    public override void Enter()
    {
        base.Enter();
        GameLogger.LogAnimation("Playing death animation");
        playerAnimator.SetAnimationState(AnimationState.Die);
    }

    public override void HandleInput()
    {
        // No transitions from death state
        GameLogger.LogState("DieState - ignoring input handling", LogType.Log);
    }

    public override void Exit()
    {
        GameLogger.LogState("Exiting DieState - This shouldn't happen!", LogType.Warning);
    }
}