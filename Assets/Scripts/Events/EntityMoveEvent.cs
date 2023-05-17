using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EntityMoveEvent : UnityEvent<EntityMoveEvent> 
{
    float invocationTime;
    Entity entity;
    Vector2Int movePosition;

    public EntityMoveEvent()
    {
    }

    public EntityMoveEvent(Entity entity, Vector2Int movePosition)
    {
        this.entity = entity;
        this.movePosition = movePosition;

        this.invocationTime = Time.time;
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

    // Cell position of entity before event execution
    public Vector2Int GetPosition()
    {
        return entity.GetPosition();
    }

    // Cell position of entity after event execution
    public Vector2Int GetMovePosition()
    {
        return movePosition;
    }
}
