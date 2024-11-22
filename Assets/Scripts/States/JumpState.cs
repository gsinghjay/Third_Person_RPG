public class JumpState : AnimationStateBase
{
    private readonly PlayerMovement playerMovement;

    public JumpState(PlayerAnimator animator) : base(animator)
    {
        playerMovement = animator.GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
        {
            GameLogger.LogState("PlayerMovement component not found!", LogType.Error);
        }
    }

    public override void Enter()
    {
        base.Enter();
        playerAnimator.SetAnimationState(AnimationState.Jumping);
    }

    public override void HandleInput()
    {
        if (playerMovement == null)
        {
            GameLogger.LogState("HandleInput - playerMovement is null", LogType.Error);
            return;
        }

        if (playerMovement.IsGrounded())
        {
            GameLogger.LogState("JumpState HandleInput - Player has landed");
            playerAnimator.TriggerLand();  // Reset IsJumping
            
            if (playerAnimator.IsDead)
            {
                TransitionToState(new DieState(playerAnimator));
            }
            else if (playerAnimator.IsVictorious)
            {
                TransitionToState(new VictoryState(playerAnimator));
            }
            else if (playerAnimator.IsMoving)
            {
                TransitionToState(new MoveState(playerAnimator));
            } 
            else
            {
                TransitionToState(new IdleState(playerAnimator));
            }
        }
    }

    public override void UpdateState()
    {
        if (playerAnimator.IsMoving)
        {
            playerAnimator.SetMovementSpeed(playerAnimator.CurrentSpeed);
        }
    }
}