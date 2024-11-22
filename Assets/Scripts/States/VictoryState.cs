public class VictoryState : AnimationStateBase
{
    public VictoryState(PlayerAnimator animator) : base(animator) { }

    public override void Enter()
    {
        base.Enter();
        GameLogger.LogAnimation("Playing victory animation");
        playerAnimator.SetAnimationState(AnimationState.Victory);
    }

    public override void HandleInput()
    {
        // Victory state doesn't transition unless resetting
        GameLogger.LogState("VictoryState - ignoring input handling", LogType.Log);
    }
}