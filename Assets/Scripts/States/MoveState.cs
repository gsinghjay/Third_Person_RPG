public class MoveState : AnimationStateBase
{
    public MoveState(PlayerAnimator animator) : base(animator) { }

    public override void Enter()
    {
        base.Enter();
        GameLogger.LogAnimation("Setting movement animation");
        playerAnimator.SetIsMoving(true);
    }

    public override void HandleInput()
    {
        if (!playerAnimator.IsMoving)
        {
            TransitionToState(new IdleState(playerAnimator));
        }
        else if (playerAnimator.IsJumping)
        {
            TransitionToState(new JumpState(playerAnimator));
        }
        else if (playerAnimator.IsVictorious)
        {
            TransitionToState(new VictoryState(playerAnimator));
        }
        else if (playerAnimator.IsDead)
        {
            TransitionToState(new DieState(playerAnimator));
        }
    }

    public override void UpdateState()
    {
        GameLogger.LogAnimation($"Updating movement speed to {playerAnimator.CurrentSpeed:F2}");
        playerAnimator.SetMovementSpeed(playerAnimator.CurrentSpeed);
    }
}