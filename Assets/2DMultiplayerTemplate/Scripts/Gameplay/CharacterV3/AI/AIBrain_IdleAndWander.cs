using UnityEngine;

public class AIBrain_IdleAndWander : AIBrain
{
    [SerializeField] private float elapsedTime = 0;
    [SerializeField] private float idleTime = 1.5f;
    [SerializeField] private float wanderTime = 3f;
    [SerializeField] private bool isIdle = false;

    public override void UpdateAI()
    {
        if (isIdle)
        {
            if(elapsedTime >= idleTime)
            {
                isIdle = false;
                elapsedTime = 0f;
                agent.destination = GetRandomPosition(serverCharacter.transform.position, 2f);
            }
        }
        else
        {
            if (elapsedTime >= wanderTime)
            {
                isIdle = true;
                elapsedTime = 0f;
            }
        }

        elapsedTime += Time.deltaTime;
    }
}
