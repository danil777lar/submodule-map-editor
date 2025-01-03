using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

[EditorTool("Map Editor Tool", typeof(Map))]
public class MapTool : EditorTool
{
    public override GUIContent toolbarIcon => new GUIContent(Resources.Load<Texture2D>("Icons/MapEditorIcon"));
    
    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView sceneView))
        {
            return;
        }
        
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        
        foreach (Object target in targets)
        {
            if (target is Map map)
            {
                UpdateMap(map);       
            }
        }
    }

    private void UpdateMap(Map map)
    {
        map.Layers.ForEach(layer => layer.SetTransform(map.transform));
        
        DrawGrid(map);
        DrawFilledTiles(map);
        
        UpdateCurrentTool(map);
    }

    private void DrawGrid(Map map)
    {
        foreach (MapLayer layer in map.Layers)
        {
            if (map.CurrentLayer == layer)
            {
                layer.DrawGrid();
            }
        }
    }

    private void DrawFilledTiles(Map map)
    {
        foreach (MapLayer layer in map.Layers)
        {
            layer.DrawFilledTiles(map.CurrentLayer == layer);
        }
    }

    private void UpdateCurrentTool(Map map)
    {
        if (map.CurrentLayer is { CurrentTool: not null })
        {
            map.CurrentLayer.CurrentTool.Process(map);
        }
    }
}
