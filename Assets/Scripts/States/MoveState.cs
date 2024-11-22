public class MoveState : IAnimationState
{
    private PlayerAnimator playerAnimator;

    public MoveState(PlayerAnimator animator)
    {
        GameLogger.LogState("Initializing MoveState");
        playerAnimator = animator;
    }

    public void Enter()
    {
        GameLogger.LogState("Entering MoveState");
        GameLogger.LogAnimation("Setting movement animation");
        playerAnimator.SetIsMoving(true);
    }

    public void HandleInput()
    {
        if (!playerAnimator.IsMoving)
        {
            GameLogger.LogState("MoveState - Transitioning to IdleState");
            playerAnimator.ChangeState(new IdleState(playerAnimator));
        }
        else if (playerAnimator.IsJumping)
        {
            GameLogger.LogState("MoveState - Transitioning to JumpState");
            playerAnimator.ChangeState(new JumpState(playerAnimator));
        }
        else if (playerAnimator.IsVictorious)
        {
            GameLogger.LogState("MoveState - Transitioning to VictoryState");
            playerAnimator.ChangeState(new VictoryState(playerAnimator));
        }
        else if (playerAnimator.IsDead)
        {
            GameLogger.LogState("MoveState - Transitioning to DieState");
            playerAnimator.ChangeState(new DieState(playerAnimator));
        }
    }

    public void UpdateState()
    {
        GameLogger.LogAnimation($"Updating movement speed to {playerAnimator.CurrentSpeed:F2}");
        playerAnimator.SetMovementSpeed(playerAnimator.CurrentSpeed);
    }

    public void Exit()
    {
        GameLogger.LogState("Exiting MoveState");
    }
}