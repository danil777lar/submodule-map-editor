using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public abstract class MapLayerFace : MapLayer
{
    public abstract override IReadOnlyCollection<Tool> GetTools();
    
    public abstract override void Build();

    public override Tile WorldPointToTile(Vector3 worldPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        Vector2Int from = new Vector2Int(
            Mathf.FloorToInt(localPoint.x / tileSize.x), 
            Mathf.FloorToInt(localPoint.z / tileSize.y));

        return new Tile(from, from + Vector2Int.one);
    }

    public override Tile WorldPointToTile(Vector3 worldPoint, Tile pointerDownTile)
    {
        return WorldPointToTile(worldPoint);
    }
    
    public override IReadOnlyCollection<Tile> GetTilesInRadius(Vector2 center, int radius)
    {
        radius = radius - 1;
        
        Vector2Int flooredCenter = new Vector2Int(
            Mathf.FloorToInt(center.x), 
            Mathf.FloorToInt(center.y));
        
        List<Tile> tiles = new List<Tile>();
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int from = flooredCenter + new Vector2Int(x, y);
                Vector2Int to = from + Vector2Int.one;
                tiles.Add(new Tile(from, to));
            }
        }

        return tiles;
    }

    public override IReadOnlyCollection<Tile> GetTilesInRect(Vector2 from, Vector2 to, bool filled)
    {
        Vector2Int flooredFrom = new Vector2Int(
            Mathf.FloorToInt(Mathf.Min(from.x, to.x)), 
            Mathf.FloorToInt(Mathf.Min(from.y, to.y)));
        
        Vector2Int flooredTo = new Vector2Int(
            Mathf.FloorToInt(Mathf.Max(to.x, from.x)), 
            Mathf.FloorToInt(Mathf.Max(to.y, from.y)));
        
        List<Tile> tiles = new List<Tile>();
        for (int x = flooredFrom.x; x <= flooredTo.x; x++)
        {
            for (int y = flooredFrom.y; y <= flooredTo.y; y++)
            {
                Vector2Int tileFrom = new Vector2Int(x, y);
                Vector2Int tileTo = tileFrom + Vector2Int.one;
                
                if (filled || x == flooredFrom.x || x == flooredTo.x || y == flooredFrom.y || y == flooredTo.y)
                {
                    tiles.Add(new Tile(tileFrom, tileTo));
                }
            }
        }

        return tiles;
    }
}
