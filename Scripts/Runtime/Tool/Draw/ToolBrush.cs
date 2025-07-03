using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ToolBrush : ToolDraw
{
    public override Texture2D Icon => _icon ??= Resources.Load<Texture2D>("Icons/ToolPen");
    
    protected override void OnDraw(Map map, Tile tile)
    {
        #if UNITY_EDITOR
        
        if (!map.CurrentLayer.FilledTiles.Contains(tile))
        {
            map.CurrentLayer.FilledTiles.Add(tile);
            UnityEditor.EditorUtility.SetDirty(map);
        }    
        
        #endif
    }
}
