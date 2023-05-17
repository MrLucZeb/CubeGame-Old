using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurnEvent : UnityEvent<TurnEvent>
{
    float invocationTime;
    TurnManager turnManager;
    EntityMoveEvent entityMoveEvent;
    Entity entity;
    int turn;
    int nextTurn;
    bool repeatTurn = false;

    public TurnEvent()
    {
    }

    public TurnEvent(TurnManager turnManager, Entity entity, int turn, int nextTurn)
    {
        this.turnManager = turnManager;
        this.entity = entity;
        this.turn = turn;
        this.nextTurn = nextTurn;
        this.entityMoveEvent = (Time.time == entity.GetMoveEvent().GetInvocationTime()) ? entity.GetMoveEvent() : null;

        this.invocationTime = Time.time;
    }

    // Gives extra turn to entity when true
    public void SetRepeatTurn(bool condition)
    {
        repeatTurn = condition;
    }

    public bool GetRepeatTurn()
    {
        return repeatTurn;
    }

    // Returns the entity move event that was invoked as a result of the entities turn/during the entites turn, returns null if the entity did not move
    public EntityMoveEvent GetEntityMoveEvent()
    {
        return entityMoveEvent;
    }

    public TurnManager GetTurnManager()
    {
        return turnManager;
    }

    // Returns the frame time at which this event was invoked
    public float GetInvocationTime()
    {
        return invocationTime;
    }
        
    public Entity GetEntity()
    {
        return entity;
    }

    public int GetTurn()
    {
        return turn;
    }

    public int GetNextTurn()
    {
        return nextTurn;
    }
}
