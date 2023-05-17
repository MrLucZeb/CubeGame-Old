using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelCollection", menuName = "Level Collection")]
public class LevelCollection : ScriptableObject
{
    [SerializeField] private List<Grid> levels;

    public Grid GetLevel(int index)
    {
        if (index < 0 || index > levels.Count) {
            throw new System.Exception("Level of index '" + index + "' does not exist");
        }

        return levels[0].Clone();
    }

    public int GetLevelCount()
    {
        return levels.Count;
    }
}
