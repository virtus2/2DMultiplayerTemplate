using UnityEngine;

public class AIBrain_Idle : AIBrain
{
    [SerializeField] private float elapsedTime = 0;

    public override void UpdateAI()
    {
        elapsedTime += Time.deltaTime;
    }
}
