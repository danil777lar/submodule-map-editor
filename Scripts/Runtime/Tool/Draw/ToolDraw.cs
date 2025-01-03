using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public abstract class ToolDraw : Tool
{
    [SerializeField] private int radius = 1;

    private bool _pointerDown;
    private Tile _pointerDownTile;
    
    public abstract override Texture2D Icon { get; }
    
    protected abstract void OnDraw(Map map, Tile tile);
    
    public override void DrawEditorGUI()
    {
        GUILayout.Label("Options:");
        radius = Mathf.Max(EditorGUILayout.IntField("Radius", radius), 1);
    }
    
    public override void Process(Map map)
    {
        List<Tile> tiles = GetCurrentTiles(map);
        
        UpdatePointerState();
        
        DrawTiles(map, tiles);
        TryFillTiles(map, tiles);
    }

    private void UpdatePointerState()
    {
        if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
        {
            _pointerDown = true;
        }
        else if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
        {
            _pointerDown = false;
        }
    }

    private List<Tile> GetCurrentTiles(Map map)
    {
        List<Tile> tiles = new List<Tile>();

        if (map.CurrentLayer != null)
        {
            if (map.CurrentLayer.TryRaycastMousePosition(out Vector3 position))
            {
                Tile centerTile = _pointerDown ? 
                    map.CurrentLayer.WorldPointToTile(position, _pointerDownTile) : 
                    map.CurrentLayer.WorldPointToTile(position);
                
                tiles = map.CurrentLayer.GetTilesInRadius(centerTile.GetCenter(), radius).ToList();

                if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
                {
                    _pointerDownTile = centerTile;
                }
            }
        }

        return tiles;
    } 

    private void DrawTiles(Map map, List<Tile> tiles)
    {
        MapLayer layer = map.CurrentLayer;
        tiles.ForEach(tile => layer.DrawTile(tile, MapEditorConfig.Hover));
    }
    
    private void TryFillTiles(Map map, List<Tile> tiles)
    {
        bool pressed = Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag;
        pressed &= Event.current.button == 0;
        
        if (pressed)
        {
            foreach (Tile tile in tiles)
            {
                OnDraw(map, tile);
            }
        }
    }
}
