using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager
{
    private static TurnManager instance = null;
    private static bool initialized = false;

    Game game;

    List<Entity> queue;
    int turnIndex;

    bool requestTurn = true;
    float turnCooldown = 0.2f;

    private TurnManager()
    {
        GameEvents.GetInstance().entityMoveEvent.AddListener(OnEntityMove);
        GameEvents.GetInstance().entityDeathEvent.AddListener(OnEntityDeath);
    }

    // Initializes TurnManager and creates instance if no instance exists
    public static TurnManager Init(Game game)
    {
        if (IsInitialized()) { Debug.LogError("TurnManager is already initialized"); return instance; }
       
        TurnManager turnManager = GetInstance();
        turnManager.game = game;
        turnManager.LoadQueue();
    
        initialized = true;
        return turnManager;
    }

    public void LoadQueue()
    {
        queue = game.GetEntities();

        
        turnIndex = 0;

        if (queue[0].GetObjectType() != GridObject.ObjectType.PLAYER)
        {
            for (int i = 1; i < queue.Count; i++)
            {
                if (queue[i].GetObjectType() != GridObject.ObjectType.PLAYER) continue;

                Entity player = queue[i];
                queue[i] = queue[0];
                queue[0] = player;
            }
        }
    }

    public void Update()
    {
        if (!IsInitialized()) { Debug.LogWarning("TurnManager is not initialized"); return; }
        if (turnIndex >= queue.Count) { turnIndex = 0; }

        if (requestTurn)
        {
            Entity entity = queue[turnIndex];

            if (!entity.IsAlive()) { turnIndex++; }
            else
            {
                if (entity.ExecuteTurn())
                {
                    TurnEvent turnEvent = new TurnEvent(this, entity, turnIndex, turnIndex + 1);
                    GameEvents.GetInstance().turnEvent.Invoke(turnEvent);

                    if (!turnEvent.GetRepeatTurn())
                        turnIndex = turnEvent.GetNextTurn();

                    game.StartCoroutine(CooldownTimer());
                }
            }
        }
    }

    public static bool IsInitialized()
    {
        return initialized;
    }

    public static TurnManager GetInstance()
    {
        if (instance == null)
        {
            instance = new TurnManager();
        }

        return instance;
    }

    void OnEntityMove(EntityMoveEvent e)
    {
        
    }

    void OnEntityDeath(EntityDeathEvent e)
    {
        if (queue.Contains(e.GetEntity()))
            queue.Remove(e.GetEntity());
    }


    IEnumerator CooldownTimer()
    {
        requestTurn = false;

        yield return new WaitForSeconds(turnCooldown);

        requestTurn = true;
    }

    public List<Entity> GetQueue()
    {
        if (!IsInitialized()) { Debug.LogError("TurnManager is not initialized"); }
        return queue;
    }
}
