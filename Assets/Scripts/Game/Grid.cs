using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Level", menuName = "Grid Level")]
public class Grid : ScriptableObject
{
    public Vector3 origin { get; private set; }
    [SerializeField] private int width;
    [SerializeField] private int height;
    public int cellSize { get; private set; } = 1;
    [SerializeField] private int[] arr;
    public int size = 9; // TODO refactor grid scriptable object to level class

    public Grid()
    {
    }

    public Grid(Vector3 origin, int width, int height, int cellSize, int value = 0)
    {
        this.origin = origin;
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.arr = new int[this.width * this.height];
    }

    public Grid(Grid copy)
    {
        width = copy.width;
        height = copy.height;
        cellSize = copy.cellSize;
        origin = copy.origin;
        arr = new int[width * height];

        copy.arr.CopyTo(arr, 0);
    }

    public Grid Clone()
    {
        return new Grid(this);
    }

    public void Reset() 
    {
        arr = new int[width * height];
    }

    public Vector3 GetOrigin()
    {
        return origin;
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void Resize(int width, int height)
    {
        if (this.width == width && this.height == height) return;

        Grid old = new Grid(this);

        this.width = width;
        this.height = height;
        this.arr = new int[this.width * this.height];

        int xMax = (this.width <= old.width) ? this.width : old.width;
        int yMax = (this.height <= old.height) ? this.height : old.height;
        for (int x = 0; x < xMax; x++)
        {
            for (int y = 0; y < yMax; y++)
            {
                Set(x, y, old.Get(x, y));
            }
        }
    }
     
    public Vector3 CellToWorldPosition(Vector2Int position)
    {
        return CellToWorldPosition(position, origin, cellSize);
    }

    public Vector2Int WorldToCellPosition(Vector3 position)
    {
        return WorldToCellPosition(position, origin, cellSize);
    }

    public static Vector3 CellToWorldPosition(Vector2Int position, Vector3 gridOrigin, float gridCellSize)
    {
        return gridOrigin + new Vector3(position.x, position.y, 0) * gridCellSize;
    }

    public static Vector2Int WorldToCellPosition(Vector3 position, Vector3 gridOrigin, float gridCellSize)
    {
        Vector3 cellPosition = new Vector3(position.x, position.y, 0) / gridCellSize - gridOrigin;
        return new Vector2Int(Mathf.RoundToInt(cellPosition.x), Mathf.RoundToInt(cellPosition.y));
    }

    public int Get(Vector2Int position)
    {
        return arr[position.x + position.y * width];
    }

    public void Set(Vector2Int position, int value)
    {
        if (IsValidPosition(position))
            arr[position.x + position.y * width] = value;
    }

    public int Get(int x, int y)
    {
        return Get(new Vector2Int(x, y));
    }

    public void Set(int x, int y, int value)
    {
        Set(new Vector2Int(x, y), value);
    }

    public bool IsValid()
    {
        return width > 0 && height > 0;
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.y >= 0 && position.x < width && position.y < height;
    }

    public bool IsValidPosition(int x, int y)
    {
        return IsValidPosition(new Vector2Int(x, y));
    }

    public int ValueCount(int value)
    {
        int count = 0;

        for (int i = 0; i < width * height; i++)
        {
            if (this.arr[i] == value)
                count++;
        }

        return count;
    }

    public override string ToString() 
    {
        string str = "grid: \n";

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                str += " " + Get(x, y);
            }

            str += " | " + (height - y - 1) + "\n";
        }

        return str;
    }
}
