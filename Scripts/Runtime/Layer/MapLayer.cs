using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Random = UnityEngine.Random;

[Serializable]
public abstract class MapLayer
{
    [SerializeField] protected Vector2 tileSize = Vector2.one;
    [SerializeField] protected Vector2 tileOffset = Vector2.zero;
    [SerializeField] protected float VerticalOffset = 0;
    
    [SerializeField] protected Transform transform;
    
    public string Name = "Layer";
    
    public Color Color = Color.black;
    public Tool CurrentTool;
    public List<Tile> FilledTiles = new List<Tile>();
    
    public virtual float Height => 0;

    public abstract IReadOnlyCollection<Tool> GetTools();

    public abstract void Build();

    public abstract Tile WorldPointToTile(Vector3 worldPoint);
    public abstract Tile WorldPointToTile(Vector3 worldPoint, Tile pointerDownTile);

    public abstract IReadOnlyCollection<Tile> GetTilesInRadius(Vector2 center, int radius);
    
    public abstract IReadOnlyCollection<Tile> GetTilesInRect(Vector2 from, Vector2 to, bool filled);

    public virtual void DrawEditorGUI()
    {
        #if UNITY_EDITOR
        DrawEditorGUIHeader("Base Settings");
        DrawEditorGUILine(() =>
            tileOffset = UnityEditor.EditorGUILayout.Vector2Field("Tile Offset", tileOffset));
        DrawEditorGUILine(() =>
            tileSize = UnityEditor.EditorGUILayout.Vector2Field("Tile Size",
                new Vector2(Mathf.Max(0.1f, tileSize.x), Mathf.Max(0.1f, tileSize.y))));
        DrawEditorGUILine(() =>
            VerticalOffset = UnityEditor.EditorGUILayout.FloatField("Vertical Offset", VerticalOffset));
        
        UnityEditor.EditorGUILayout.Space(20f);
        #endif
    }

    protected void DrawEditorGUIHeader(string header)
    {
        #if UNITY_EDITOR
        UnityEditor.EditorGUILayout.LabelField(header, UnityEditor.EditorStyles.boldLabel);
        UnityEditor.EditorGUILayout.Space(5f);
        #endif
    }

    protected void DrawEditorGUILine(Action drawCallback)
    {
        #if UNITY_EDITOR
        UnityEditor.EditorGUILayout.BeginHorizontal();
        UnityEditor.EditorGUILayout.LabelField("|", GUILayout.Width(20f));
        drawCallback.Invoke();
        UnityEditor.EditorGUILayout.EndHorizontal();
        #endif
    }

    public void SetTransform(Transform t)
    {
        transform = t;
    }
    
    public Vector3 TileToLocal(Vector2 tilePoint)
    {
        Vector3 result = new Vector3(tilePoint.x * tileSize.x, VerticalOffset, tilePoint.y * tileSize.y); 
        result += new Vector3(tileOffset.x, 0f, tileOffset.y);
        
        return result;
    }

    public Vector2 LocalToTile(Vector3 localPoint)
    {
        Vector3 offset = new Vector3(tileOffset.x, 0f, tileOffset.y);
        Vector3 point = localPoint - offset;
        return new Vector2(point.x / tileSize.x, point.z / tileSize.y);
    }

    public Vector3 TileToWorld(Vector2 tilePoint)
    {
        return transform.TransformPoint(TileToLocal(tilePoint));
    }

    public Vector3 WorldToTile(Vector3 worldPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        return LocalToTile(localPoint);
    }

    public Vector2 GetTileFromAtLocalPoint(Vector2 point)
    {
        Vector2 tilePoint = LocalToTile(point);
        return new Vector2(Mathf.Floor(tilePoint.x), Mathf.Floor(tilePoint.y));
    }
    
    public bool TryRaycastMousePosition(out Vector3 position)
    {
        #if UNITY_EDITOR
        Vector3 mousePosition = Event.current.mousePosition;
        Ray ray = UnityEditor.HandleUtility.GUIPointToWorldRay(mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.up * VerticalOffset);
        if (plane.Raycast(ray, out float distance))
        {
            position = ray.GetPoint(distance);
            return true;
        }
        
        #endif
        
        position = Vector3.zero;
        return false;
    }

    public void DrawGrid()
    {
        #if UNITY_EDITOR
        GetGridCorners(out Vector2 min, out Vector2 max);
        UnityEditor.Handles.color = MapEditorConfig.GridEbabled;
        
        for (float x = min.x; x <= max.x; x++)
        {
            Vector3 start = TileToWorld(new Vector2(x, min.y));
            Vector3 end = TileToWorld(new Vector2(x, max.y));
            UnityEditor.Handles.DrawLine(start, end);
        }

        for (float y = min.y; y <= max.y; y++)
        {
            Vector3 start = TileToWorld(new Vector2(min.x, y));
            Vector3 end = TileToWorld(new Vector2(max.x, y));
            UnityEditor.Handles.DrawLine(start, end);
        }
        #endif
    }
    
    public void DrawFilledTiles(bool selected)
    {
        Color color = Color.SetAlpha(selected ? Color.a : 0.2f);
        FilledTiles.ForEach(tile => tile.Draw(color, Height, TileToWorld));
    }

    public void DrawTile(Tile tile, Color color)
    {
        tile.Draw(color, Height, TileToWorld);
    }
    
    protected void SaveMesh(Mesh mesh)
    {
        #if UNITY_EDITOR
        mesh.name = transform.GetPath().Replace("/", "_").Replace(" ", "") + "_" + Name;
            
        if (!Directory.Exists("Assets/3D/Autogenerated/Meshes"))
        {
            Directory.CreateDirectory("Assets/3D/Autogenerated/Meshes");
        }

        string assetPath = $"Assets/3D/Autogenerated/Meshes/{mesh.name}.asset";
        if (UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(assetPath) != null)
        {
            UnityEditor.AssetDatabase.DeleteAsset(assetPath);
        }
        
        UnityEditor.AssetDatabase.CreateAsset(mesh, assetPath);
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }
    
    private void GetGridCorners(out Vector2 min, out Vector2 max)
    {
        min = new Vector2(-5, -5);
        max = new Vector2(5, 5);

        foreach (Tile tile in FilledTiles)
        {
            min = Vector2.Min(min, tile.From - Vector2Int.one * 5);
            max = Vector2.Max(max, tile.To + Vector2Int.one * 5);
        }
    }
}
