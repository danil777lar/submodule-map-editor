using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;

[Serializable]
public struct Tile
{
    public bool Initizlized;
    
    public Vector2Int From;
    public Vector2Int To;
    public Vector2 Forward => ((Vector2)To - (Vector2)From).normalized;
    public Vector2 Backward => -Forward;
    public Vector2 Right => new Vector2(Forward.y, -Forward.x);
    public Vector2 Left => -Right;
    
    public Tile(Vector2Int from, Vector2Int to)
    {
        Initizlized = true;
        
        From = from;
        To = to;
    }
    
    
    public bool Equals(Tile other)
    {
        return From.Equals(other.From) && To.Equals(other.To);
    }
    
    public Vector2 GetCenter()
    {
        return ((Vector2)From + (Vector2)To) * 0.5f;
    }
    
    public void Draw(Color color, float height, Func<Vector2, Vector3> tileToWorld)
    {
        Vector2[] verts = GetRectVerts(out Vector2[] vertical);
        Vector3[] worldVerts = new Vector3[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            worldVerts[i] = tileToWorld.Invoke(verts[i]);
        }
        
        Handles.DrawSolidRectangleWithOutline(worldVerts, color, Color.clear);
        
        if (height > 0f)
        {
            for (int i = 0; i < vertical.Length; i++)
            {
                vertical[i] = vertical[i];
            }

            Vector3[] verticalRect = new []
            {
                tileToWorld.Invoke(vertical[0]) + Vector3.up * height,
                tileToWorld.Invoke(vertical[1]) + Vector3.up * height,
                tileToWorld.Invoke(vertical[1]),
                tileToWorld.Invoke(vertical[0])
            }; 
            
            Handles.DrawSolidRectangleWithOutline(verticalRect, color, Color.clear);
        }
    }

    public bool TouchWith(Tile other, out Vector2 selfDirection, out Vector2 otherDirection)
    {
        if (Equals(other))
        {
            selfDirection = Vector2.zero;
            otherDirection = Vector2.zero;
            return false;
        }
        
        if (From == other.From)
        {
            selfDirection = Backward;
            otherDirection = other.Forward;
            return true;
        }
        
        if (From == other.To)
        {
            selfDirection = Backward;
            otherDirection = other.Backward;
            return true;
        }
        
        if (To == other.From)
        {
            selfDirection = Forward;
            otherDirection = other.Forward;
            return true;
        }
        
        if (To == other.To)
        {
            selfDirection = Forward;
            otherDirection = other.Backward;
            return true;
        }

        selfDirection = Vector2.zero;
        otherDirection = Vector2.zero;
        return false;
    }

    private Vector2[] GetRectVerts(out Vector2[] vertical)
    {
        vertical = new Vector2[2];
     
        Vector2 center = GetCenter();
        Vector2 size = new Vector2(1f, 1f) * 0.9f;

        if (From.x == To.x)
        {
            size.x *= 0.1f;
            vertical[0] = new Vector2(size.x, -size.y) * 0.5f + GetCenter();
            vertical[1] = new Vector2(size.x, size.y) * 0.5f + GetCenter();
        }
         
        if (From.y == To.y)
        {
            size.y *= 0.1f;
            vertical[0] = new Vector2(-size.x, size.y) * 0.5f + GetCenter();
            vertical[1] = new Vector2(size.x, size.y) * 0.5f + GetCenter();
        }
        
        return new[]
        {
            new Vector2(-size.x, -size.y) * 0.5f + GetCenter(),
            new Vector2(size.x, -size.y) * 0.5f + GetCenter(),
            new Vector2(size.x, size.y) * 0.5f + GetCenter(),
            new Vector2(-size.x, size.y) * 0.5f + GetCenter()
        };   
    }
}
