using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ToolRect : Tool
{
    [SerializeField] private bool filled;
    [SerializeField] private bool isPointerDown;
    [SerializeField] private Tile pointerDownTile;

    public override Texture2D Icon => _icon ??= Resources.Load<Texture2D>("Icons/ToolRect");

    public override void DrawEditorGUI()
    {
        #if UNITY_EDITOR
        
        GUILayout.Label("Rect options");
        filled = UnityEditor.EditorGUILayout.Toggle("Filled", filled);
        
        #endif
    }

    public override void Process(Map map)
    {
        Tile pointerTile = GetPointerTile(map);

        Vector2 currentPosition = pointerTile.From;
        Vector2 pointerDownPosition = pointerDownTile.From;
        
         List<Tile> tiles = map.CurrentLayer.GetTilesInRect(pointerDownPosition, currentPosition, filled).ToList();

        OnPointerHover(map, pointerTile, tiles);
        OnPointerDown(map, pointerTile);
        OnPointerUp(map, tiles);
    }

    private void OnPointerHover(Map map, Tile pointerTile, List<Tile> tiles)
    {
        MapLayer layer = map.CurrentLayer;
        if (tiles.Count > 1 && isPointerDown)
        {
            tiles.ForEach(tile => layer.DrawTile(tile, MapEditorConfig.Hover));
        }
        else
        {
            layer.DrawTile(pointerTile, MapEditorConfig.Hover);
        }
    }

    private void OnPointerDown(Map map, Tile pointerTile)
    {
        if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
        {
            isPointerDown = true;
            pointerDownTile = pointerTile;
        }
    }

    private void OnPointerUp(Map map, List<Tile> tiles)
    {
        if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
        {
            isPointerDown = false;
            if (tiles.Count > 1)
            {
                foreach (Tile tile in tiles)
                {
                    if (!map.CurrentLayer.FilledTiles.Contains(tile))
                    {
                        map.CurrentLayer.FilledTiles.Add(tile);
                    }
                }
            }
        }
    }

    private Tile GetPointerTile(Map map)
    {
        if (map.CurrentLayer.TryRaycastMousePosition(out Vector3 worldPosition))
        {
            return map.CurrentLayer.WorldPointToTile(worldPosition);
        }

        return new Tile();
    }

    private List<Tile> GetTilesInRect(Tile from, Tile to)
    {
        List<Tile> tiles = new List<Tile>();
        
        int minX = Mathf.Min(from.From.x, to.To.x);
        int maxX = Mathf.Max(from.From.x, to.To.x);
        int minY = Mathf.Min(from.From.y, to.To.y);
        int maxY = Mathf.Max(from.From.y, to.To.y);
        
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                tiles.Add(new Tile(new Vector2Int(x, y), new Vector2Int(x, y)));
            }
        }

        return tiles;
    }
}
