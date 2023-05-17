using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelEditor : EditorWindow
{
    Grid level;

    GridObject[] gridObjects;
    Dictionary<int, Color> objectColors;
    Dictionary<int, string> objectNames;

    GridObject selectedObject;
    int width;
    int height;
    int size;

    bool gridResetVerification = false;
    bool displayNamesToggle = true;

    [MenuItem("Window/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow(typeof(LevelEditor));
    }

    void OnEnable()
    {
        LoadGridObjects();
        LoadObjectInfo();

        gridResetVerification = false;
    }

    void OnGUI()
    {
        LevelSelectionField();

        if (level != null)
        {
            EditorGUILayout.Space(5);

            LevelWidthField();
            LevelHeightField();
            LevelSizeField();
            ResizeLevelButton();

            EditorGUILayout.Space(15);

            ObjectSelectionMenu();
            DisplayNamesToggle();

            EditorGUILayout.Space(5);

            GridEditor();

            EditorUtility.SetDirty(level);

            ResetLevelButton();
        }
    }

    void LoadGridObjects()
    {
        GameObject[] prefabs = GridObject.LoadPrefabs();
        gridObjects = new GridObject[prefabs.Length];

        for (int i = 0; i < prefabs.Length; i++)
        {
            gridObjects[i] = prefabs[i].GetComponent<GridObject>();
        }
        
    }
     
    void LoadObjectInfo()
    {
        objectColors = new Dictionary<int, Color>(gridObjects.Length + 1);
        objectNames = new Dictionary<int, string>(gridObjects.Length);

        Material background = (Material) AssetDatabase.LoadAssetAtPath("Assets/Materials/Background.mat", typeof(Material));
        objectColors.Add(0, background.color);

        foreach (GridObject gridObject in gridObjects)
        {
            Color color = gridObject.GetComponent<Renderer>().sharedMaterial.color;
            string name = gridObject.name;
            int type = (int)gridObject.GetObjectType();

            objectColors.Add(type, color);
            objectNames.Add(type, name);
        }
    }

    Color GetObjectColor(int gridValue)
    {
        if (objectColors.ContainsKey(gridValue))
            return objectColors[gridValue];
        else 
            return Color.clear;
    }

    Vector2Int FindPlayerPosition()
    {
        for (int x = 0; x < level.GetWidth(); x++)
        {
            for (int y = 0; y < level.GetHeight(); y++)
            {
                if (level.Get(x, y) == (int)GridObject.ObjectType.PLAYER)
                    return new Vector2Int(x, y);
            }
        }

        return new Vector2Int(-1, -1);
    }

    string GetObjectName(int gridValue)
    {
        if (objectNames.ContainsKey(gridValue))
            return objectNames[gridValue];
        else
            return "";
    }

    void OnObjectSelection(object gridObject)
    {
        selectedObject = (GridObject)gridObject;
    }

    void GridEditor()
    {
        int width = level.GetWidth();
        int height = level.GetHeight();
        int type = (selectedObject == null) ? 0 : (int)selectedObject.GetObjectType();
        float yMax = (position.height - 140 - GUI.skin.button.margin.vertical * height) / height;
        float xMax = (position.width - GUI.skin.button.margin.horizontal * width) / width;
        float buttonSize = (xMax <= yMax) ? xMax : yMax;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 10;

        for (int y = height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
           
            for (int x = 0; x < width; x++)
            {
                GUI.backgroundColor = GetObjectColor(level.Get(x, y));

                string buttonText = (displayNamesToggle) ? GetObjectName(level.Get(x, y)) : "";
                if (GUILayout.Button(buttonText, buttonStyle, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize)))
                { 
                    if (level.Get(x, y) == type) continue;
                  
                    // Reposition player to prevent duplicate
                    if (type == (int)GridObject.ObjectType.PLAYER)
                    {
                        Vector2Int pos = FindPlayerPosition();
                        if (level.IsValidPosition(pos))
                            level.Set(pos.x, pos.y, 0);
                    }

                    level.Set(x, y, type);
                }
            }
         
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
    }

    static bool IsMouseOver() 
    {
        return Event.current.type == EventType.MouseDown && 
            GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
    }


    void ObjectSelectionMenu()
    {
        string objectName = (selectedObject == null) ? "None" : selectedObject.name;
        if (GUILayout.Button(objectName, GUILayout.MaxWidth(75)))
        {
            GenericMenu dropdown = new GenericMenu();

            dropdown.AddItem(new GUIContent("None"), false, OnObjectSelection, null);
     
            foreach (GridObject gridObject in gridObjects)
            {
                dropdown.AddItem(new GUIContent(gridObject.name), false, OnObjectSelection, gridObject);
            }

            dropdown.ShowAsContext();
        }
    }

    void ResizeLevelButton() 
    { 
        if ((level.GetWidth() != width || level.GetHeight() != height || level.size != size) && GUILayout.Button("Apply (resize grid)", GUILayout.MaxWidth(125)))
        {
            level.Resize(width, height);
            level.size = size;
        }
    }

    void ResetLevelButton()
    {
        EditorGUILayout.BeginHorizontal();

        string text = (level == null) ? "Initialize Level Grid" : (gridResetVerification) ? "Are you sure?" : "Reset Level Grid";
        if (GUILayout.Button(text, GUILayout.MaxWidth(150)))
        {
            if (gridResetVerification || level == null)
            {
                level.Reset();
                gridResetVerification = false;
            }
            else if (!gridResetVerification)
            {
                gridResetVerification = true;
            }
        }

        if (gridResetVerification && (GUILayout.Button("Cancel", GUILayout.MaxWidth(50))))
        {
            gridResetVerification = false;
        }

        EditorGUILayout.EndHorizontal();
    }

    void LevelSelectionField()
    {
        Grid previous = level;
        level = (Grid) EditorGUILayout.ObjectField(level, typeof(Grid), false, GUILayout.MaxWidth(125));

        if (level != previous && level != null)
        {
            width = level.GetWidth();
            height = level.GetHeight();
        }
    }

    void LevelWidthField()
    {
        EditorGUILayout.BeginHorizontal();

        width = EditorGUILayout.IntField(width, GUILayout.MaxWidth(75));
        EditorGUILayout.HelpBox("current width: " + level.GetWidth(), MessageType.None);

        EditorGUILayout.EndHorizontal();
    }

    void LevelHeightField()
    {
        EditorGUILayout.BeginHorizontal();

        height = EditorGUILayout.IntField(height, GUILayout.MaxWidth(75));
        EditorGUILayout.HelpBox("current height: " + level.GetHeight(), MessageType.None);

        EditorGUILayout.EndHorizontal();
    }

    void LevelSizeField()
    {
        EditorGUILayout.BeginHorizontal();

        size = EditorGUILayout.IntField(size, GUILayout.MaxWidth(75));
        EditorGUILayout.HelpBox("current size: " + level.size, MessageType.None);

        EditorGUILayout.EndHorizontal();
    }

    void DisplayNamesToggle()
    {
        displayNamesToggle = EditorGUILayout.Toggle("Display object names", displayNamesToggle);
    }
}
