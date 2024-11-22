public class IdleState : IAnimationState
{
    private PlayerAnimator playerAnimator;

    public IdleState(PlayerAnimator animator)
    {
        GameLogger.LogState("Initializing IdleState");
        playerAnimator = animator;
    }

    public void Enter()
    {
        GameLogger.LogState("Entering IdleState");
        playerAnimator.SetIsMoving(false);
    }

    public void HandleInput()
    {
        if (playerAnimator.IsMoving)
        {
            GameLogger.LogState("IdleState - Transitioning to MoveState");
            playerAnimator.ChangeState(new MoveState(playerAnimator));
        }
        else if (playerAnimator.IsJumping)
        {
            GameLogger.LogState("IdleState - Transitioning to JumpState");
            playerAnimator.ChangeState(new JumpState(playerAnimator));
        }
        else if (playerAnimator.IsVictorious)
        {
            GameLogger.LogState("IdleState - Transitioning to VictoryState");
            playerAnimator.ChangeState(new VictoryState(playerAnimator));
        }
        else if (playerAnimator.IsDead)
        {
            GameLogger.LogState("IdleState - Transitioning to DieState");
            playerAnimator.ChangeState(new DieState(playerAnimator));
        }
    }

    public void UpdateState()
    {
        // Idle state specific updates if needed
    }

    public void Exit()
    {
        GameLogger.LogState("Exiting IdleState");
    }
}