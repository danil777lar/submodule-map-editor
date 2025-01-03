using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor 
{
    private Map _map;
    private bool _foldoutTools = true;
    private bool _foldoutLayers = true;
    
    private void OnEnable()
    {
        _map = (Map)target;
    }

    public override void OnInspectorGUI()
    {
        DrawToolsGroup();
        DrawLayersGroup();
        DrawBuildGroup();
    }

    private void DrawToolsGroup()
    {
        _foldoutTools = EditorGUILayout.Foldout(_foldoutTools, "TOOLS");
        if (_foldoutTools) 
        {
            EditorGUI.indentLevel++;
            GUILayout.Space(10);

            if (_map.CurrentLayer != null)
            {
                EditorGUILayout.BeginHorizontal();
                
                foreach (Tool tool in _map.CurrentLayer.GetTools())
                {
                    GUILayoutOption[] options = GetToolButtonOptions();
                    GUIStyle style = _map.CurrentLayer.CurrentTool == tool ? 
                        GetToolButtonSelectedStyle() : GetToolButtonStyle();
                    
                    if (GUILayout.Button(tool.Icon, style, options))
                    {
                        _map.CurrentLayer.CurrentTool = _map.CurrentLayer.CurrentTool == tool ? null : tool;
                    }
                    GUILayout.Space(10);
                }
                
                EditorGUILayout.EndHorizontal();
                
                if (_map.CurrentLayer.CurrentTool != null)
                {
                    GUILayout.Space(10);
                    _map.CurrentLayer.CurrentTool.DrawEditorGUI();
                }
            }
            else
            {
                GUILayout.Label("Select a layer to use tools");
            }
            
            GUILayout.Space(30);
            EditorGUI.indentLevel--;
        }
    }

    private void DrawLayersGroup()
    {
        _foldoutLayers = EditorGUILayout.Foldout(_foldoutLayers, "LAYERS");
        if (_foldoutLayers) 
        {
            EditorGUI.indentLevel++;
           
            GUILayout.Space(10f);

            List<string> layerNames = new List<string>();
            for (int i = 0; i < _map.Layers.Count; i++)
            { 
                DrawLayer(_map.Layers[i], layerNames, out bool removed);
                if (removed)
                {
                    break;
                }   
            }
            
            GUILayout.Space(20f);
            if (GUILayout.Button("Add Layer: Surface"))
            {
                _map.Layers.Add(new MapLayerFaceMeshPlane()
                {
                    Color = MapEditorConfig.FilledTile[Random.Range(0, MapEditorConfig.FilledTile.Length)]
                });
            }
            if (GUILayout.Button("Add Layer: Wall"))
            {
                _map.Layers.Add(new MapLayerEdgeMeshWall()
                {
                    Color = MapEditorConfig.FilledTile[Random.Range(0, MapEditorConfig.FilledTile.Length)]
                });
            }
            if (GUILayout.Button("Add Layer: Tile"))
            {
                _map.Layers.Add(new MapLayerFacePrefabTile()
                {
                    Color = MapEditorConfig.FilledTile[Random.Range(0, MapEditorConfig.FilledTile.Length)]
                });
            }
            
            
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawBuildGroup()
    {
        GUILayout.Space(20);
        if (GUILayout.Button("Build"))
        {
            _map.Build();
        }
    }

    private void DrawLayer(MapLayer layer, List<string> names, out bool removed)
    {
        removed = false;
        
        EditorGUILayout.BeginHorizontal();
        if (_map.CurrentLayer == layer)
        {
            if (GUILayout.Button("=>", GUILayout.Width(60)))
            {
                _map.CurrentLayer = null;
            }
        }
        else
        {
            if (GUILayout.Button("SELECT", GUILayout.Width(60)))
            {
                _map.CurrentLayer = layer;
            }
        }
        
        string name = EditorGUILayout.TextField(layer.Name);
        while (names.Contains(name))
        {
            name = layer.Name + Random.Range(0, 10);
        }
        layer.Name = name;
        names.Add(layer.Name);
        
        layer.Color = EditorGUILayout.ColorField(layer.Color, GUILayout.Width(75));
        
        GUILayout.Space(5f);
        if (GUILayout.Button("X", GUILayout.Width(20)))
        { 
            _map.Layers.Remove(layer);
            removed = true;
        }
        EditorGUILayout.EndHorizontal();
        
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("DETACH", GUILayout.Width(60)))
        {
            _map.DetachLayer(layer);
            removed = true;
        }
        EditorGUILayout.EndHorizontal();
        
        
        GUILayout.Space(10f);
        
        layer.DrawEditorGUI();
        
        GUILayout.Space(20f);
    }
    
    private GUILayoutOption[] GetToolButtonOptions()
    {
        int size = 30;
        return new GUILayoutOption[]
        {
            GUILayout.Height(size),
            GUILayout.Width(size),
            GUILayout.ExpandHeight(true),
            GUILayout.ExpandWidth(true)
        };
    }
    
    private GUIStyle GetToolButtonStyle()
    {
        int padding = 3;
        return new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset(padding, padding, padding, padding),
            normal =
            {
                background = Texture2D.blackTexture,
            }
        };
    }
    
    private GUIStyle GetToolButtonSelectedStyle()
    {
        int padding = 3;
        return new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset(padding, padding, padding, padding),
        };
    }
}
