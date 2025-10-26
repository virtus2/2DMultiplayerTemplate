using UnityEngine;

public class AIBrain_DefaultMelee : AIBrain
{
    [Header("AIBrain_DefaultMelee")]
    [SerializeField] private float elapsedTime = 0;
    [SerializeField] private float pathUpdateTime = 1f;
    [SerializeField] private ServerCharacter target;

    public override void UpdateAI()
    {
        if (target == null)
        {
            target = FindTargetPlayer();
            return;
        }

        bool isTargetInNeighborChunk = ServerChunkLoader.IsNeighborChunk(serverCharacter.ChunkPosition, target.ChunkPosition);
        if(!isTargetInNeighborChunk)
        {
            target = FindTargetPlayer();
            return;
        }

        if (target.IsDead)
        {
            target = FindTargetPlayer();
            return;
        }

        if (elapsedTime >= pathUpdateTime)
        {
            agent.SetDestination(target.transform.position);
            elapsedTime = 0;
        }

        float remaining = agent.remainingDistance / clientCharacter.MovementSpeed;

        Vector2 toTarget = serverCharacter.transform.position - target.transform.position;
        float sqr = toTarget.sqrMagnitude;

        elapsedTime += Time.deltaTime;
    }
}
