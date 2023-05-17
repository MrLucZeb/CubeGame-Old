using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Game : MonoBehaviour
{
    TurnManager turnManager;
    GridManager gridManager;

    [SerializeField] GameObject entityDeathParticle;

    //Player player;
    List<Entity> entities;
    List<GridObject> walls;

    [SerializeField] LevelCollection levels;

    Dictionary<int, GameObject> gridObjectPrefabs;

    bool pauseGame = false;
    bool debugDrawPathToggle = true;

    [Header("Grid")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public int gridCellSize = 1;

    void OnEnable()
    {
        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0);
    }

    void Start()
    {
        Init();
        
        turnManager = TurnManager.Init(this);
        gridManager = GridManager.Init(new Grid(transform.position, gridWidth, gridHeight, gridCellSize), this);
        
        DebugDrawPaths();
        Vector2Int a = new Vector2Int(15, 2);
        Vector3 b = gridManager.GetGrid().CellToWorldPosition(a);
        Vector2Int c = gridManager.GetGrid().WorldToCellPosition(b);

        entities.ForEach(entity => entity.GameStart());
        walls.ForEach(entity => entity.GameStart());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log(gridManager.GetGrid());
          
        }

        if (!pauseGame)
        {
            turnManager.Update();
        }

        UpdateDebug();

        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadLevel(levels.GetLevel(0));
        }
    }

    void Init()
    {
        RegisterListeners();
        RegisterObjects();
        LoadGridObjectPrefabs();
    }

    void RegisterListeners()
    {
        GameEvents events = GameEvents.GetInstance();
        //events.entityMoveEvent.AddListener(OnEntityMove);
        events.turnEvent.AddListener(OnTurn);
        events.entityOverlapEvent.AddListener(OnEntityOverlap);
        events.entityDeathEvent.AddListener(OnEntityDeath);
    }

    void RegisterObjects()
    {
        entities = new List<Entity>();
        walls = new List<GridObject>();

        // Entities
        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Entity"))
        {
            entities.Add(gameObject.GetComponent<Entity>());
        }
        
        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Wall"))
        {
            walls.Add(gameObject.GetComponent<GridObject>());
        }
    }

    void RegisterObject(GridObject gridObject)
    {
        switch (gridObject.GetObjectType())
        {
            case GridObject.ObjectType.WALL:
                walls.Add(gridObject);
                break;
            default:
                entities.Add(gridObject.GetComponent<Entity>());
                break;
        }
    }

    void LoadGridObjectPrefabs()
    {
        GameObject[] prefabs = GridObject.LoadPrefabs();
        gridObjectPrefabs = new Dictionary<int, GameObject>(prefabs.Length);

        foreach (GameObject prefab in GridObject.LoadPrefabs())
        {
            int type = (int)prefab.GetComponent<GridObject>().GetObjectType();
            gridObjectPrefabs.Add(type, prefab);
        }
    }

    void LoadLevel(Grid level)
    {
        level.SetOrigin(transform.position);

        for (int i = entities.Count - 1; i >= 0; i--)
        {
            entities[i].Kill();
        }

        for (int i = walls.Count - 1; i >= 0; i--)
        {
            Destroy(walls[i].gameObject);
        }

        walls.Clear();
        entities.Clear();

        for (int x = 0; x < level.GetWidth(); x++)
        {
            for (int y = 0; y < level.GetHeight(); y++)
            {
                int type = level.Get(x, y);
               
                if (gridObjectPrefabs.ContainsKey(type))
                {
                    GameObject prefab = gridObjectPrefabs[type];
                    Vector3 position = level.CellToWorldPosition(new Vector2Int(x, y));

                    GridObject gridObject = Instantiate(prefab, position, prefab.transform.rotation, transform).GetComponent<GridObject>();
                    RegisterObject(gridObject);
                }
            }
        }

        pauseGame = false;

        gridManager.LoadGrid(level);
        turnManager.LoadQueue();

        Camera.main.orthographicSize = level.size * 0.6f;

        entities.ForEach(entity => entity.GameStart());
        walls.ForEach(entity => entity.GameStart());
    }

    void OnTurn(TurnEvent e)
    {
        List<Entity.ObjectType> filter = (e.GetEntity().GetObjectType() != Entity.ObjectType.PLAYER) ? new List<Entity.ObjectType> { Entity.ObjectType.PLAYER } : null;
        List <Entity> surrounding = gridManager.GetSurroundingEntites(e.GetEntity().GetPosition(), false, filter);

        //if (gridManager.HasSurroundingEntitites(e.GetEntity().GetPosition(), false, filter))
        if (surrounding.Count > 0)
        {
            List<Vector2Int> directions = new List<Vector2Int>();

            foreach (Entity entity in surrounding) 
            {
                directions.Add(entity.GetPosition() - e.GetEntity().GetPosition());
            }

            if (e.GetEntity().GetObjectType() == GridObject.ObjectType.PLAYER)
            {
                Player player = (Player) e.GetEntity();
                player.SetCanDoubleMove(true);
            }

            e.GetEntity().ForceDirections(directions);
            e.SetRepeatTurn(true);
        }
    }

    void OnEntityDeath(EntityDeathEvent e)
    {
        

        if (e.IsPlayer() || entities.Count < 3)
        {
            entities.Remove(e.GetEntity());
            pauseGame = true;
        }
    }
     
    void OnEntityOverlap(EntityOverlapEvent e)
    {
        Debug.Log(e.GetSubject());
        e.GetSubject().Kill();

        ParticleSystem particle = Instantiate(entityDeathParticle, e.GetSubject().transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        ParticleSystem.VelocityOverLifetimeModule velocity = particle.velocityOverLifetime;
        particle.startColor = e.GetSubject().GetComponent<Renderer>().material.color;
        Vector2Int dir = e.GetCause().GetMovePosition() - e.GetCause().GetPosition();
        velocity.x = dir.x * 5;
        velocity.x = dir.y * 5;
       
    }

    // TODO move to seperate class
    void UpdateDebug() 
    {
        if (Input.GetKeyDown(KeyCode.P)) debugDrawPathToggle = !debugDrawPathToggle;

        if (debugDrawPathToggle) DebugDrawPaths();
    }

    void DebugDrawPaths()
    {
        foreach (Entity entity in entities)
        {
            List<Vector2Int> path = entity.GetPath();
            for (int i = 0; i < entity.GetPath().Count - 2; i++)
            {
                Vector3 a = gridManager.GetGrid().CellToWorldPosition(path[i]);
                Vector3 b = gridManager.GetGrid().CellToWorldPosition(path[i + 1]);

                Debug.DrawLine(a, b, Color.green);
            }
        }
    }

    public List<GridObject> GetObjects()
    {
        List<GridObject> objects = new List<GridObject>(walls.Count + entities.Count);
        objects.AddRange(walls);
        objects.AddRange(entities);

        return objects;
    }

    public List<Entity> GetEntities()
    {
        return entities;
    }

    public List<GridObject> GetWalls()
    {
        return walls;
    }

    public Grid GetGrid()
    {
        if (GridManager.IsInitialized())
            return gridManager.GetGrid();
        else
            return GetComponent<Grid>();
    }

    public GameObject GetEntityDeathParticle()
    {
        
        return entityDeathParticle;
    }

    public void OnDrawGizmos()
    {
        Vector3 origin = transform.position;
        int width = gridWidth;
        int height = gridHeight;
        float cellSize = gridCellSize;

        Vector3 offset = new Vector3((float) cellSize / 2, (float) cellSize / 2, 0);

        for (int x = 0; x <= width; x++)
        {
            Gizmos.color = (x == 0 || x == width) ? Color.white : Color.grey;

            Vector3 a = Grid.CellToWorldPosition(new Vector2Int(x, 0), origin, cellSize) - offset;
            Vector3 b = Grid.CellToWorldPosition(new Vector2Int(x, height), origin, cellSize) - offset;

            Gizmos.DrawLine(a, b);
        }

        for (int y = 0; y <= height; y++)
        {
            Gizmos.color = (y == 0 || y == height) ? Color.white : Color.grey;

            Vector3 a = Grid.CellToWorldPosition(new Vector2Int(0, y), origin, cellSize) - offset;
            Vector3 b = Grid.CellToWorldPosition(new Vector2Int(width, y), origin, cellSize) - offset;

            Gizmos.DrawLine(a, b);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gizmos.DrawSphere(Grid.CellToWorldPosition(new Vector2Int(x, y), origin, cellSize), 0.1f);
            }
        }
    }
}
