using UnityEngine;

public interface IState
{
    public void OnEnter();
    public void OnServerUpdate();
    public void OnClientUpdate();
    public void CheckTransitions();
    public void OnExit();
}
