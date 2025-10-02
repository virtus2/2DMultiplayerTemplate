using UnityEngine;

public interface IState
{
    void OnEnter();
    void OnUpdate(ref Vector2 movementVector);
    void OnExit();
}
