using NavMeshPlus.Extensions;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static Unity.Multiplayer.Editor.EditorMultiplayerRolesManager.AutomaticSelection;

public abstract class AIBrain : MonoBehaviour
{
    [SerializeField] protected ServerCharacter serverCharacter;
    [SerializeField] protected ClientCharacter clientCharacter;
    [SerializeField] protected NavMeshAgent agent;

    private void Awake()
    {
        // HACK: AgentOverride2d is not working, set updateRotation and updateUpAxis to false manually.
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // TODO: Handle AI weapon equip
        // it's just test code right now...
        clientCharacter.HandleEquipInput(true, 0);
    }

    public abstract void UpdateAI();

    protected Vector2 GetRandomPosition(Vector2 origin, float radius)
    {
        Vector2 randomPosition = origin + Random.insideUnitCircle * radius;

        if(NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return randomPosition;
    }

    protected ServerCharacter FindTargetPlayer()
    {
        foreach (var kvp in GameManager.Instance.PlayerCharacters)
        {
            ServerCharacter target = kvp.Value;
            if (target.IsDead) continue;
            if (!ServerChunkLoader.IsNeighborChunk(serverCharacter.ChunkPosition, target.ChunkPosition)) continue;

            return target;
        }
        return null;
    }
}
