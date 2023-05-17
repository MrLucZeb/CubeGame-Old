using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEvents
{
    private static GameEvents instance = null;

    public UnityEvent<EntityMoveEvent> entityMoveEvent { get; private set; }
    public UnityEvent<TurnEvent> turnEvent { get; private set; }
    public UnityEvent<EntityOverlapEvent> entityOverlapEvent { get; private set; }
    public UnityEvent<EntityDeathEvent> entityDeathEvent { get; private set; }

    private GameEvents()
    {
        entityMoveEvent = new EntityMoveEvent();
        turnEvent = new TurnEvent();
        entityOverlapEvent = new EntityOverlapEvent();
        entityDeathEvent = new EntityDeathEvent();
    }

    public static GameEvents GetInstance()
    {
        if (instance == null)
        {
            instance = new GameEvents();
        }

        return instance;
    }
}
