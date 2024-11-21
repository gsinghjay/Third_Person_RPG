using UnityEngine;

public interface IAnimationState
{
    void Enter();
    void HandleInput();
    void UpdateState();
    void Exit();
}