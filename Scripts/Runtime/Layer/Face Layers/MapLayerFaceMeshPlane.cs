using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class MapLayerFaceMeshPlane : MapLayerFace
{
    [SerializeField] private bool lookBackwards = false;
    [SerializeField] private Vector2 uvScale = new Vector2(1f, 1f);
    [SerializeField] private Material material;
    [SerializeField] private Color vertexColor;
    
    [SerializeField] private List<Tool> tools;
    
    public override IReadOnlyCollection<Tool> GetTools()
    {
        tools ??= new List<Tool>
        {
            new ToolBrush(),
            new ToolEraser(),
            new ToolRect()
        };
        return tools;
    }

    public override void DrawEditorGUI()
    {
        #if UNITY_EDITOR
        
        base.DrawEditorGUI();
        
        DrawEditorGUIHeader("Plane Settings");
        DrawEditorGUILine(() => 
            lookBackwards = UnityEditor.EditorGUILayout.Toggle("Look Backwards", lookBackwards));
        DrawEditorGUILine(() => 
            uvScale = UnityEditor.EditorGUILayout.Vector2Field("UV Scale", uvScale));
        DrawEditorGUILine(() => 
            material = UnityEditor.EditorGUILayout.ObjectField("Material", material, typeof(Material), false) as Material);
        DrawEditorGUILine(() => 
            vertexColor = UnityEditor.EditorGUILayout.ColorField("Vertex Color", vertexColor));
        
        #endif
    }

    public override void Build()
    {
        GameObject go = new GameObject(Name);
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateMesh();
        
        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        
        MeshCollider meshCollider = go.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.sharedMesh;
        
        SaveMesh(meshFilter.sharedMesh);

        go.isStatic = true;
    }
    
    private List<Tile> GetOptimizedTiles()
    {
        List<Tile> tiles = new List<Tile>();
        tiles.AddRange(FilledTiles);

        tiles = OptimizeTiles(tiles.ToArray(), true).ToList();
        tiles = OptimizeTiles(tiles.ToArray(), false).ToList();

        return tiles;
    }

    private Tile[] OptimizeTiles(Tile[] input, bool directionHorizontal)
    {
        List<Tile> tiles = input.ToList();
        
        bool changed = true;
        while (changed)
        {
            changed = false;
            
            for (int i = 0; i < tiles.Count; i++)
            {
                Tile tile = tiles[i];
                for (int j = 0; j < tiles.Count; j++)
                {
                    Tile other = tiles[j];
                    
                    if (tile.From == other.From && tile.To == other.To)
                    {
                        continue;
                    }
                    
                    bool merge = directionHorizontal
                        ? (tile.To.x == other.From.x && tile.From.y == other.From.y && tile.To.y == other.To.y)
                        : (tile.To.y == other.From.y && tile.From.x == other.From.x && tile.To.x == other.To.x);
                    
                    if (merge)
                    {
                        tile.To = other.To;
                        tiles[i] = tile;
                        tiles.RemoveAt(j);
                        
                        changed = true;
                        break;
                    }
                }
                
                if (changed)
                {
                    break;
                }
            }
        }

        return tiles.ToArray();
    }
    
    private Mesh CreateMesh()
    {
        #if UNITY_EDITOR
        
        Mesh mesh = new Mesh();
        mesh.name = $"{Name}";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        GetOptimizedTiles().ForEach(tile => 
            CreateTileMesh(tile, vertices, triangles, uvs));
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = Enumerable.Repeat(vertexColor, vertices.Count).ToArray();
        
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        UnityEditor.Unwrapping.GenerateSecondaryUVSet(mesh);
        
        return mesh;
        
        #endif

        return null;
    }
    
    private void CreateTileMesh(Tile tile, List<Vector3> verts, List<int> tris, List<Vector2> uvs)
    {
        List<Vector3> positions = new List<Vector3>
        {
            TileToLocal(tile.From),
            TileToLocal(new Vector2(tile.To.x, tile.From.y)),
            TileToLocal(tile.To),
            TileToLocal(new Vector2(tile.From.x, tile.To.y))
        };
        
        positions.ForEach(position =>
        {
            verts.Add(position);
            uvs.Add(new Vector2(position.x, position.z) * uvScale);
        });
        
        int index = verts.Count - 4;

        if (lookBackwards)
        {
            tris.Add(index);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index);
            tris.Add(index + 2);
            tris.Add(index + 3);
        }
        else
        {
            tris.Add(index);
            tris.Add(index + 2);
            tris.Add(index + 1);
            tris.Add(index);
            tris.Add(index + 3);
            tris.Add(index + 2);
        }
    }
}
