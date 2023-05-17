using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager
{
    static GridManager instance = null;
    static bool initialized = false;

    Game game;
    Grid grid;

    private GridManager()
    {
        GameEvents events = GameEvents.GetInstance();
        events.entityMoveEvent.AddListener(OnEntityMove);
    }

    public static GridManager GetInstance()
    {
        if (instance == null)
        {
            instance = new GridManager();
        }

        return instance;
    }

    // Initializes GridManager and creates instance if no instance exists
    public static GridManager Init(Grid grid, Game game)
    {
        if (IsInitialized()) { Debug.LogError("GridManager is already initialized"); return instance; }

        GridManager gridManager = GetInstance();
        gridManager.game = game;
        gridManager.LoadGrid(grid);

        initialized = true;

        return gridManager;
    }

    public static bool IsInitialized()
    {
        return initialized;
    }

    public void LoadGrid(Grid grid)
    {
        this.grid = grid;

        game.GetWalls().ForEach(w => grid.Set(w.GetPosition(), (int)w.GetObjectType()));
        //game.GetObjects().ForEach(o => grid.Set(o.GetPosition(), (int)o.GetObjectType()));
    }

    public Grid MakeObstructionGrid()
    {
        Grid obstructionGrid = new Grid(grid);

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                if (grid.Get(x, y) == (int) GridObject.ObjectType.WALL)
                {
                    obstructionGrid.Set(x, y, 1);
                }
                   
            }
        }

        return obstructionGrid;
    }
    
    void OnEntityMove(EntityMoveEvent e)
    {
        Debug.Log("mov" + grid + " | " + e.GetMovePosition());
        if (HasObject(e.GetMovePosition()))
        {
           
            EntityOverlapEvent overlapEvent = new EntityOverlapEvent(GetEntity(e.GetMovePosition()), e.GetEntity(), e);
            GameEvents.GetInstance().entityOverlapEvent.Invoke(overlapEvent);
        }

        grid.Set(e.GetPosition(), 0);
        grid.Set(e.GetMovePosition(), (int) e.GetEntity().GetObjectType());
    }

    // Refactor, also possibly broken. detects enemies when already dead
    public List<Entity> GetSurroundingEntites(Vector2Int cell, bool allowDiagonals, List<Entity.ObjectType> filter = null)
    {
        List<Entity> entities = new List<Entity>();
        
        foreach (Entity entity in game.GetEntities())
        {
            if (filter != null && !filter.Contains(entity.GetObjectType())) continue;
            if (Vector2Int.Distance(cell, entity.GetPosition()) > ((allowDiagonals) ? 2 : 1)) continue; // TODO refactor grid to always have cell size of 1 to prevent error
            if (cell.x == entity.GetPosition().x && cell.y == entity.GetPosition().y) continue;

            entities.Add(entity);
        }

        return entities;
    }

    public bool HasSurroundingEntitites(Vector2Int cell, bool allowDiagonals, List<Entity.ObjectType> filter = null)
    {
        for (int x = cell.x - 1; x < cell.x + 2; x++)
        {
            for (int y = cell.y - 1; y < cell.y + 2; y++)
            {
                if (!grid.IsValidPosition(x, y)) continue;
                if (grid.Get(x, y) == 0) continue; // If not entity
                if (cell.x == x && cell.y == y) continue; // If same cell as parameter
                if (!allowDiagonals && (cell.x != x && cell.y != y)) continue; // If diagonal
                if (filter != null && !filter.Contains((Entity.ObjectType) grid.Get(x, y))) continue; // Filter entity type

                return true;
            }
        }

        return false;
    }

    // Finds entity on given cell position, return null if no entity is found
    public Entity GetEntity(Vector2Int cell)
    {
        if (HasObject(cell))
        {
            foreach (Entity entity in game.GetEntities())
            {
                if (entity.GetPosition() == cell)
                    return entity;
            }
        }

        return null;
    }

    // Check if a cell contains an entity
    public bool HasObject(Vector2Int cell)
    {
        
       
        return grid.IsValidPosition(cell) && grid.Get(cell.x, cell.y) != 0;
    }

    public bool HasWall(Vector2Int cell)
    {
        return grid.IsValidPosition(cell) && grid.Get(cell.x, cell.y) == (int)GridObject.ObjectType.WALL;
    }

    public bool HasEntity(Vector2Int cell)
    {
        return HasObject(cell) && !HasWall(cell);
    }

    public Grid GetGrid()
    {
        return grid;
    }
}
