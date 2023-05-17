using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Called whenever two entities overlap on the grid
public class EntityOverlapEvent : UnityEvent<EntityOverlapEvent>
{
    Entity subject;
    Entity overlapper;
    EntityMoveEvent moveEvent;

    public EntityOverlapEvent()
    {
    }

    // a = already present entity, b = overlapping entity (moving entity)
    public EntityOverlapEvent(Entity subject, Entity overlapper, EntityMoveEvent moveEvent)
    {
        this.subject = subject;
        this.overlapper = overlapper;
        this.moveEvent = moveEvent;
    }

    // Entity that is being overlapped
    public Entity GetSubject()
    {
        return subject;
    }

    // Entity that is overlapping the subject
    public Entity GetOverlapper()
    {
        return overlapper;
    }

    // Returns the move event that caused the overlap
    public EntityMoveEvent GetCause()
    {
        return moveEvent;
    }
}
