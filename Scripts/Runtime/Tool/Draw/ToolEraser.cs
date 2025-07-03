using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ToolEraser : ToolDraw
{
    public override Texture2D Icon => _icon ??= Resources.Load<Texture2D>("Icons/ToolEraser");
    
    protected override void OnDraw(Map map, Tile tile)
    {
        if (!TryRemoveTile(map, tile))
        {
            (tile.From, tile.To) = (tile.To, tile.From);
            TryRemoveTile(map, tile);
        }
    }

    private bool TryRemoveTile(Map map, Tile tile)
    {
        #if UNITY_EDITOR
        
        if (map.CurrentLayer.FilledTiles.Contains(tile))
        {
            map.CurrentLayer.FilledTiles.Remove(tile);
            UnityEditor.EditorUtility.SetDirty(map);

            return true;
        }
        
        #endif

        return false;
    }
}
