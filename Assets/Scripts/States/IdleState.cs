public class IdleState : AnimationStateBase
{
    public IdleState(PlayerAnimator animator) : base(animator) { }

    public override void Enter()
    {
        base.Enter();
        playerAnimator.SetIsMoving(false);
    }

    public override void HandleInput()
    {
        if (playerAnimator.IsMoving)
        {
            TransitionToState(new MoveState(playerAnimator));
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
}