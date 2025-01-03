using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class MapLayerEdge : MapLayer
{
    public abstract override IReadOnlyCollection<Tool> GetTools();
    
    public abstract override void Build();
    
    public override Tile WorldPointToTile(Vector3 worldPoint, Tile pointerDownTile)
    {
        Vector2 direction = ((Vector2)pointerDownTile.To - pointerDownTile.From).normalized;
        return WorldRayToTile(worldPoint, direction);
    }
    
    public override Tile WorldPointToTile(Vector3 worldPoint)
    {
        return WorldRayToTile(worldPoint, Vector2.zero);
    }

    public override IReadOnlyCollection<Tile> GetTilesInRadius(Vector2 center, int radius)
    {
        radius = radius - 1;
        Vector2Int flooredCenter = new Vector2Int(Mathf.FloorToInt(center.x), Mathf.FloorToInt(center.y));

        List<Tile> tilesInQuad = new List<Tile>();
        tilesInQuad.Add(new Tile(flooredCenter, flooredCenter + Vector2Int.up));
        tilesInQuad.Add(new Tile(flooredCenter + Vector2Int.up, flooredCenter + Vector2Int.one));
        tilesInQuad.Add(new Tile(flooredCenter + Vector2Int.one, flooredCenter + Vector2Int.right));
        tilesInQuad.Add(new Tile(flooredCenter + Vector2Int.right, flooredCenter));
        
        Tile closest = tilesInQuad.OrderBy(tile => Vector2.Distance(center, tile.GetCenter())).First();
        Vector2Int from = closest.From;
        Vector2Int delta = closest.From - closest.To;
        
        List<Tile> tiles = new List<Tile>();
        for (int i = -radius - 1; i < radius; i++)
        {
            Vector2Int to = from + delta;
            tiles.Add(new Tile(from + delta * i, to + delta * i));
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
                if (x == flooredFrom.x)
                {
                    tiles.Add(new Tile(tileFrom, tileFrom + Vector2Int.up));
                }
                
                if (x == flooredTo.x)
                {
                    tiles.Add(new Tile(tileFrom + Vector2Int.right, tileFrom + Vector2Int.one));
                }
                
                if (y == flooredFrom.y)
                {
                    tiles.Add(new Tile(tileFrom, tileFrom + Vector2Int.right));
                }
                
                if (y == flooredTo.y)
                {
                    tiles.Add(new Tile(tileFrom + Vector2Int.up, tileFrom + Vector2Int.one));
                }
            }
        }

        return tiles;
    }

    public Tile WorldRayToTile(Vector3 worldFrom, Vector2 direction)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldFrom);
        Vector2Int from = new Vector2Int(
            Mathf.FloorToInt(localPoint.x / tileSize.x), 
            Mathf.FloorToInt(localPoint.z / tileSize.y));
        
        List<Tile> tiles = new List<Tile>();
        tiles.Add(new Tile(from, from + Vector2Int.up));
        tiles.Add(new Tile(from + Vector2Int.up, from + Vector2Int.one));
        tiles.Add(new Tile(from + Vector2Int.one, from + Vector2Int.right));
        tiles.Add(new Tile(from + Vector2Int.right, from));

        if (direction != Vector2.zero)
        {
            tiles = tiles.FindAll(x =>
            {
                Vector2 xDirection = ((Vector2)x.To - x.From).normalized; 
                return xDirection == direction || xDirection == -direction;
            });   
        }
        tiles = tiles.OrderBy(tile => Vector3.Distance(worldFrom, TileToWorld(tile.GetCenter()))).ToList();
        return tiles.First();
    }
}
