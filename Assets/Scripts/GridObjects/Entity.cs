using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : GridObject
{
    public List<Vector2Int> moveDirections { get; private set; } = new List<Vector2Int> // All possible directions that entity can move in
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    [SerializeField] ParticleSystem deathParticle;

    protected Entity target;
    protected ObjectType entityType;
    protected EntityMoveEvent lastMoveEvent;
    Animation moveAnimation;
    
    protected List<Vector2Int> blockedDirections = new List<Vector2Int>();
    protected List<Vector2Int> path = new List<Vector2Int>();

    Vector3 startPosition;
    Vector3 targetPosition;
    [SerializeField] float moveAlpha = 0;

    protected bool isAlive = true;

    new void Start()
    {
        target = FindObjectOfType<Player>();

        targetPosition = transform.position;
        moveAnimation = GetComponent<Animation>();
        
        base.Start();
    }

    void Update()
    {

        if (moveAnimation.isPlaying)
        {
            Vector3 dir = targetPosition - startPosition;
            transform.position = startPosition + dir * moveAlpha;
        }
        else
        {
            targetPosition = transform.position;
            moveAlpha = 0;
        }

    }

    public override void GameStart()
    {
        path = PathFinder.FindPath(gridManager.MakeObstructionGrid(), GetPosition(), target.GetPosition()); // TODO: refactor index 
        if (path.Count > 2)
        {
             Vector2Int direction = GetPosition() - path[path.Count - 2];

             Debug.Log("Direction: " + direction);
        }
        Debug.Log(path.Count);
       
    }

    public override Vector2Int GetPosition()
    {
        return gridManager.GetGrid().WorldToCellPosition(targetPosition);
    }

    public override ObjectType GetObjectType()
    {
        return ObjectType.MOVER;
    }

    // Request entity to execute its turn, must be called every frame until succesful (returns true)
    public virtual bool ExecuteTurn()
    {
        if (moveAnimation.isPlaying) return false;

        if (MoveToCell(path[path.Count - 2]))
        {
            path = PathFinder.FindPath(gridManager.MakeObstructionGrid(), GetPosition(), target.GetPosition()); // TODO: refactor index 
            Vector2Int direction = GetPosition() - path[path.Count - 2];

            if (direction == Vector2Int.up) transform.Find("Canvas").transform.rotation = Quaternion.Euler(0, 0, 90);
            if (direction == Vector2Int.right) transform.Find("Canvas").transform.rotation = Quaternion.Euler(0, 0, 0);
            if (direction == Vector2Int.down) transform.Find("Canvas").transform.rotation = Quaternion.Euler(0, 0, -90);
            if (direction == Vector2Int.left) transform.Find("Canvas").transform.rotation = Quaternion.Euler(0, 0, 180);

            Debug.Log("Direction" + direction);
            return true;
        }

        return false;
    }

    // Moves entity to given cell position on the grid
    protected override bool MoveToCell(Vector2Int position)
    {
        if (!CanMoveToCell(position)) return false;
        
        lastMoveEvent = new EntityMoveEvent(this, position);
        GameEvents.GetInstance().entityMoveEvent.Invoke(lastMoveEvent);

        targetPosition = gridManager.GetGrid().CellToWorldPosition(new Vector2Int(position.x, position.y));
        startPosition = transform.position;
        
        moveAnimation.Play();

        return true;
    }

    // Moves entity into given direction to a cell position on the grid
    protected virtual bool Move(Vector2Int direction)
    {
        return MoveToCell(GetPosition() + direction);
    }

    // Checks if entity is able to move to given cell position
    protected virtual bool CanMoveToCell(Vector2Int position)
    {
        Vector2Int direction = position - GetPosition();

        return (gridManager.GetGrid().IsValidPosition(position) && !gridManager.HasWall(position));
    }

    // Kills the entity
    public void Kill()
    {
        isAlive = false;

        EntityDeathEvent deathEvent = new EntityDeathEvent(this);
        GameEvents.GetInstance().entityDeathEvent.Invoke(deathEvent);

      
        
        

        Destroy(gameObject);
    }

    // Force given direction for the most imminent turn (undos all other blocked directions)
    public void ForceDirection(Vector2Int direction)
    {
        blockedDirections.Clear();

        for (int i = 0; i < moveDirections.Count; i++)
        {
            if (moveDirections[i] != direction)
                blockedDirections.Add(moveDirections[i]);
        }
    }

    public void ForceDirections(List<Vector2Int> directions)
    {
        if (directions.Count == moveDirections.Count)
        {
            Debug.LogWarning("Cannot block directions, there must always be one direction that entity can move in");
            return;
        }

        blockedDirections.Clear();

        for (int i = 0; i < moveDirections.Count; i++)
        {
            if (!directions.Contains(moveDirections[i]))
                blockedDirections.Add(moveDirections[i]);
        }
    }

    // Blocks given directions for the most imminent turn
    public void BlockDirection(Vector2Int direction)
    {
        if (blockedDirections.Contains(direction)) return; // Prevents duplicates
        if (blockedDirections.Count > moveDirections.Count - 2)
        {
            Debug.LogWarning("Cannot block more direction, there must always be one direction that entity can move in");
            return;
        }

        blockedDirections.Add(direction);
    }

    // Unblock a previously blocked direction
    public void UnblockDirection(Vector2Int direction)
    {
        blockedDirections.Remove(direction);
    }

    // Returns the most recent move event invoked by entity
    public EntityMoveEvent GetMoveEvent()
    {
        return lastMoveEvent;
    }

    // Returns current path towards its target
    public List<Vector2Int> GetPath()
    {
        return path;
    }

    public bool IsAlive()
    {
        return isAlive;
    }
}
