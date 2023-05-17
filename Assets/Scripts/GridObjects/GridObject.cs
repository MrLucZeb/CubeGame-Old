using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridObject : MonoBehaviour
{
    public enum ObjectType
    {
        WALL = 1,
        MOVER = 2,
        PLAYER = 3
    }

    protected GridManager gridManager;
    public AnimationClip spawnAnimation;

    void OnEnable()
    {
        Reposition();
    }

    protected void Start()
    {
        if (!gameObject.GetComponentInParent<Game>())
        {
            Debug.LogWarning(gameObject.name + ": Requires a parent object with script 'Game' attached to it. Destroying object...");
            Destroy(gameObject);

            return;
        }

        gridManager = GridManager.GetInstance();
    }

    // Called after game level has been succesfully loaded
    public virtual void GameStart()
    {
       
    }

    public static GameObject[] LoadPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("*", new[] { "Assets/GridObjects" });
        GameObject[] prefabs = new GameObject[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);

            prefabs[i] = (GameObject) AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
        }

        return prefabs;
    }

    // Position object to center of current closest grid cell
    public void Reposition()
    {
        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0);
    }

    public virtual ObjectType GetObjectType()
    {
        return ObjectType.WALL;
    }

    // Returns the cell position of entity
    public virtual Vector2Int GetPosition()
    {
        return gridManager.GetGrid().WorldToCellPosition(transform.position);
    }

    // Moves entity to given cell position on the grid
    protected virtual bool MoveToCell(Vector2Int position)
    {
        Vector3 target = gridManager.GetGrid().CellToWorldPosition(new Vector2Int(position.x, position.y));
        transform.position = target;

        return true;
    }
}
