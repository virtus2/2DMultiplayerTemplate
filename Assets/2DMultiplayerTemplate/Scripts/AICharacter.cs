using System.Collections.Generic;
using UnityEngine;

public class AICharacter : Character
{
    [SerializeField] protected CharacterControlInput aiInput;

    public static List<AICharacter> spawnedAICharacters = new List<AICharacter>();

    private float findNewPositionDelay = 2f;
    private float elapsedTime = 0f;
    private Vector2 targetPosition = Vector2.zero;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        name = $"{nameof(AICharacter)} - {spawnedAICharacters.Count}";
        spawnedAICharacters.Add(this);
        if (HasAuthority)
        {
            InitializeAI();
        }
    }

    protected override void InitializeStateMachine()
    {
        // TODO: Add custom state
        stateMachine.AddState(ECharacterState.Idle, new IdleState(this, stateMachine));
        stateMachine.AddState(ECharacterState.Walk, new WalkState(this, stateMachine));
    }

    protected virtual void InitializeAI()
    {
        elapsedTime = findNewPositionDelay;
    }

    protected override void HandleInput()
    {
        if (HasAuthority)
        {
            ProcessAI();

            Input = aiInput;
        }
    }

    protected virtual void ProcessAI()
    {
        // Default example
        if (elapsedTime >= findNewPositionDelay)
        {
            elapsedTime = 0f;
            targetPosition = (Vector2)transform.position + Random.insideUnitCircle * 2f;
        }

        elapsedTime += Time.deltaTime;

        Vector2 toTarget = targetPosition - (Vector2)transform.position;
        toTarget.Normalize();
        aiInput.Move = toTarget;
    }
}
