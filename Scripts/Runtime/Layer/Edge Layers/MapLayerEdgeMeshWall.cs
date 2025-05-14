using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class MapLayerEdgeMeshWall : MapLayerEdge
{
    [SerializeField] private float wallHeight = 3f;
    [SerializeField] private float wallWidth = 0.25f;
    
    [SerializeField] private int subwallCount = 1;
    [SerializeField] private List<SubwallConfig> subwalls = new List<SubwallConfig>();
    [SerializeField] private List<Tool> _tools;

    public override float Height => wallHeight;

    public override void DrawEditorGUI()
    {
        base.DrawEditorGUI();

        DrawEditorGUIHeader("Wall Settings");
        DrawEditorGUILine(() =>
            wallHeight = EditorGUILayout.FloatField("Wall Height", wallHeight));
        DrawEditorGUILine(() =>
            wallWidth = EditorGUILayout.FloatField("Wall Width", wallWidth));

        DrawEditorGUILine(() => { });

        DrawEditorGUILine(() =>
            subwallCount = Mathf.Max(1, EditorGUILayout.IntField("Subwalls Count", subwallCount)));

        while (subwalls.Count > subwallCount)
        {
            subwalls.RemoveAt(subwalls.Count - 1);
        }

        for (int i = 0; i < subwallCount; i++)
        {
            if (subwalls.Count <= i)
            {
                subwalls.Add(null);
            }

            DrawEditorGUILine(() =>
                subwalls[i] = (SubwallConfig)EditorGUILayout.ObjectField(subwalls[i], typeof(SubwallConfig), false));
        }
    }

    public override IReadOnlyCollection<Tool> GetTools()
    {
        _tools ??= new List<Tool>
        {
            new ToolBrush(),
            new ToolEraser(),
            new ToolRect()
        };
        return _tools;
    }


    public override void Build()
    {
        GameObject go = new GameObject(Name);
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        List<Material> materials = new List<Material>();
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateMesh(materials);

        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.materials = materials.ToArray();

        MeshCollider meshCollider = go.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.sharedMesh;

        SaveMesh(meshFilter.sharedMesh);

        go.isStatic = true;
    }

    private Mesh CreateMesh(List<Material> materials)
    {
        Mesh mesh = new Mesh();
        mesh.name = $"{Name}";

        MeshData meshData = new MeshData
        {
            Verts = new List<Vector3>(),
            Tris = new List<int>(),
            Uvs = new List<Vector2>(),
            Colors = new List<Color>(),
            SubMeshes = new List<SubMeshDescriptor>(),
            Materials = materials
        };

        List<Tile> tiles = GetOptimizedTiles(GetFixedTiles(FilledTiles));
        foreach (SubwallConfig subwall in subwalls)
        {
            int trisStart = meshData.Tris.Count;

            foreach (Tile tile in tiles)
            {
                CreateTileMesh(subwall, tile, tiles, meshData);
            }

            SubMeshDescriptor subMesh = new SubMeshDescriptor(trisStart, meshData.Tris.Count - trisStart);
            meshData.SubMeshes.Add(subMesh);
            meshData.Materials.Add(subwall.Material);
        }
        
        mesh.vertices = meshData.Verts.ToArray();
        mesh.triangles = meshData.Tris.ToArray();
        mesh.uv = meshData.Uvs.ToArray();
        mesh.colors = meshData.Colors.ToArray();

        mesh.SetSubMeshes(meshData.SubMeshes.ToArray());

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        Unwrapping.GenerateSecondaryUVSet(mesh);

        return mesh;
    }

    #region Tiles

    private List<Tile> GetFixedTiles(List<Tile> tiles)
    {
        List<Tile> fixedTiles = new List<Tile>();
        fixedTiles.AddRange(tiles);

        for (int i = 0; i < fixedTiles.Count; i++)
        {
            Tile tile = fixedTiles[i];
            if (tile.From.x > tile.To.x || tile.From.y > tile.To.y)
            {
                (tile.From, tile.To) = (tile.To, tile.From);
                fixedTiles[i] = tile;
            }
        }

        return fixedTiles;
    }

    private List<Tile> GetOptimizedTiles(List<Tile> tiles)
    {
        List<Tile> optimizedTiles = new List<Tile>();
        optimizedTiles.AddRange(tiles);

        bool changed = true;
        while (changed)
        {
            changed = false;

            for (int i = 0; i < optimizedTiles.Count; i++)
            {
                Tile tile = optimizedTiles[i];
                for (int j = i + 1; j < optimizedTiles.Count; j++)
                {
                    Tile other = optimizedTiles[j];

                    if (tile.TouchWith(other, out Vector2 selfDirection, out Vector2 otherDirection))
                    {
                        if (selfDirection == tile.Forward && otherDirection == tile.Forward)
                        {
                            bool touchAnother = optimizedTiles.Any(x => !x.Equals(tile) && !x.Equals(other)
                                && tile.TouchWith(x, out Vector2 selfDirection, out Vector2 otherDirection)
                                && selfDirection == tile.Forward);

                            if (!touchAnother)
                            {
                                Tile newTile = new Tile(tile.From, other.To);
                                optimizedTiles.Remove(tile);
                                optimizedTiles.Remove(other);
                                optimizedTiles.Add(newTile);
                                changed = true;
                                break;
                            }
                        }

                        if (selfDirection == tile.Backward && otherDirection == tile.Backward)
                        {
                            bool touchAnother = optimizedTiles.Any(x => !x.Equals(tile) && !x.Equals(other)
                                && tile.TouchWith(x, out Vector2 selfDirection, out Vector2 otherDirection)
                                && selfDirection == tile.Backward);

                            if (!touchAnother)
                            {
                                Tile newTile = new Tile(other.From, tile.To);
                                optimizedTiles.Remove(tile);
                                optimizedTiles.Remove(other);
                                optimizedTiles.Add(newTile);
                                changed = true;
                                break;
                            }
                        }
                    }
                }

                if (changed)
                {
                    break;
                }
            }
        }

        return optimizedTiles;
    }

    private void GetToOffsets(Tile tile, List<Tile> otherTiles, out Vector3 right, out Vector3 left)
    {
        right = Vector3.zero;
        left = Vector3.zero;

        bool hasToRight = otherTiles.Any(x =>
            tile.TouchWith(x, out Vector2 selfDirection, out Vector2 otherDirection)
            && selfDirection == tile.Forward && otherDirection == tile.Right);

        bool hasToLeft = otherTiles.Any(x =>
            tile.TouchWith(x, out Vector2 selfDirection, out Vector2 otherDirection)
            && selfDirection == tile.Forward && otherDirection == tile.Left);

        bool hasToForward = otherTiles.Any(x =>
            tile.TouchWith(x, out Vector2 selfDirection, out Vector2 otherDirection)
            && selfDirection == tile.Forward && otherDirection == tile.Forward);


        if (hasToRight && hasToLeft)
        {
            right = new Vector3(tile.Backward.x, 0, tile.Backward.y) * (wallWidth * 0.5f);
            left = new Vector3(tile.Backward.x, 0, tile.Backward.y) * (wallWidth * 0.5f);
        }
        else if (hasToLeft)
        {
            right = new Vector3(tile.Backward.x, 0, tile.Backward.y) * (wallWidth * 0.5f);
            if (!hasToForward)
            {
                left = new Vector3(tile.Forward.x, 0, tile.Forward.y) * (wallWidth * 0.5f);
            }
        }
        else if (hasToRight)
        {
            if (!hasToForward)
            {
                right = new Vector3(tile.Forward.x, 0, tile.Forward.y) * (wallWidth * 0.5f);
            }

            left = new Vector3(tile.Backward.x, 0, tile.Backward.y) * (wallWidth * 0.5f);
        }
    }

    private void GetFromOffsets(Tile tile, List<Tile> otherTiles, out Vector3 right, out Vector3 left)
    {
        right = Vector3.zero;
        left = Vector3.zero;

        bool hasFromRight = otherTiles.Any(x =>
            tile.TouchWith(x, out Vector2 selfDirection, out Vector2 otherDirection)
            && selfDirection == tile.Backward && otherDirection == tile.Right);

        bool hasFromLeft = otherTiles.Any(x =>
            tile.TouchWith(x, out Vector2 selfDirection, out Vector2 otherDirection)
            && selfDirection == tile.Backward && otherDirection == tile.Left);

        bool hasFromForward = otherTiles.Any(x =>
            tile.TouchWith(x, out Vector2 selfDirection, out Vector2 otherDirection)
            && selfDirection == tile.Backward && otherDirection == tile.Backward);


        if (hasFromRight && hasFromLeft)
        {
            right = new Vector3(tile.Forward.x, 0, tile.Forward.y) * (wallWidth * 0.5f);
            left = new Vector3(tile.Forward.x, 0, tile.Forward.y) * (wallWidth * 0.5f);
        }
        else if (hasFromLeft)
        {
            right = new Vector3(tile.Forward.x, 0, tile.Forward.y) * (wallWidth * 0.5f);
            if (!hasFromForward)
            {
                left = new Vector3(tile.Backward.x, 0, tile.Backward.y) * (wallWidth * 0.5f);
            }
        }
        else if (hasFromRight)
        {
            if (!hasFromForward)
            {
                right = new Vector3(tile.Backward.x, 0, tile.Backward.y) * (wallWidth * 0.5f);
            }

            left = new Vector3(tile.Forward.x, 0, tile.Forward.y) * (wallWidth * 0.5f);
        }
    }

    #endregion

    #region Mesh

    private void CreateTileMesh(SubwallConfig subwall, Tile tile, List<Tile> otherTiles, MeshData meshData)
    {
        Vector3 direction = TileToLocal(tile.To) - TileToLocal(tile.From);
        Vector3 perpendicular = new Vector3(-direction.z, 0, direction.x).normalized * (wallWidth * 0.5f);

        Vector3 from = TileToLocal(tile.From);
        Vector3 to = TileToLocal(tile.To);

        GetToOffsets(tile, otherTiles, out Vector3 toRight, out Vector3 toLeft);
        GetFromOffsets(tile, otherTiles, out Vector3 fromRight, out Vector3 fromLeft);

        CreateSideMesh(subwall, from, perpendicular + fromRight, to, perpendicular + toRight, true, meshData);
        CreateSideMesh(subwall, from, -perpendicular + fromLeft, to, -perpendicular + toLeft, false, meshData);

        if (!otherTiles.Any(x =>
                tile.TouchWith(x, out Vector2 selfDirection, out Vector2 otherDirection) &&
                selfDirection == tile.Forward))
        {
            CreateSideMesh(subwall, to, perpendicular + toRight, to, -perpendicular + toLeft, true, meshData);
        }

        if (!otherTiles.Any(x =>
                tile.TouchWith(x, out Vector2 selfDirection, out Vector2 otherDirection) &&
                selfDirection == tile.Backward))
        {
            CreateSideMesh(subwall, from, perpendicular + fromRight, from, -perpendicular + fromLeft, false, meshData);
        }
    }

    private void CreateSideMesh(SubwallConfig subwall, Vector3 from, Vector3 fromDir, Vector3 to, Vector3 toDir,
        bool lookForward, MeshData meshData)
    {
        float step = 1f / (float)subwall.Steps;
        for (float percent = 0f; percent <= 1f; percent += step)
        {
            float nextPercent = percent + step;

            GetSubwallPointsAtPercent(from, fromDir, to, toDir, percent, subwall, out Vector3 fromBottom,
                out Vector3 toBottom);
            GetSubwallPointsAtPercent(from, fromDir, to, toDir, nextPercent, subwall, out Vector3 fromTop,
                out Vector3 toTop);

            PlaneData data = new PlaneData
            {
                FromBottom = fromBottom,
                ToBottom = toBottom,
                FromTop = fromTop,
                ToTop = toTop,
                
                FromBottomColor = subwall.VertexColorGradient.Evaluate(percent),
                ToBottomColor = subwall.VertexColorGradient.Evaluate(percent),
                FromTopColor = subwall.VertexColorGradient.Evaluate(nextPercent),
                ToTopColor = subwall.VertexColorGradient.Evaluate(nextPercent)
            };

            bool smooth = percent > 0f && subwall.SmoothSteps;
            CreatePlaneMesh(data, lookForward, smooth, subwall.UVScale, meshData);
        }
    }

    private void GetSubwallPointsAtPercent(Vector3 from, Vector3 fromDir, Vector3 to, Vector3 toDir,
        float percent, SubwallConfig subwall, out Vector3 fromRes, out Vector3 toRes)
    {
        float height = Mathf.Lerp(subwall.HeightPercentMin, subwall.HeightPercentMax, percent) * wallHeight;

        Vector3 fromOffset = fromDir + fromDir.normalized * subwall.WidthCurve.Evaluate(percent);
        Vector3 toOffset = toDir + toDir.normalized * subwall.WidthCurve.Evaluate(percent);

        fromRes = from + fromOffset + Vector3.up * height;
        toRes = to + toOffset + Vector3.up * height;
    }

    private void CreatePlaneMesh(PlaneData data, bool lookForward, bool smooth, float uvScale, MeshData meshData)
    {
        List<Vector3> positions = new List<Vector3>();
        List<Color> colors = new List<Color>();

        if (smooth)
        {
            positions.AddRange(new[] { data.FromTop, data.ToTop });
            colors.AddRange(new[] { data.FromTopColor, data.ToTopColor });
        }
        else
        {
            positions.AddRange(new[] { data.FromBottom, data.ToBottom, data.FromTop, data.ToTop });
            colors.AddRange(new[] { data.FromBottomColor, data.ToBottomColor, data.FromTopColor, data.ToTopColor });
        }

        positions.ForEach(position =>
        {
            meshData.Verts.Add(position);
            meshData.Colors.Add(colors[positions.IndexOf(position)]);

            Vector2 uv = new Vector2(Mathf.Approximately(data.FromBottom.x, data.ToBottom.x)
                ? position.z
                : position.x, position.y);
            
            meshData.Uvs.Add(uv * uvScale);
        });

        int index = meshData.Verts.Count - 4;

        if (lookForward)
        {
            meshData.Tris.AddRange(new[]
            {
                index + 0, index + 1, index + 2,
                index + 2, index + 1, index + 3,
            });
        }
        else
        {
            meshData.Tris.AddRange(new[]
            {
                index + 0, index + 2, index + 1,
                index + 2, index + 3, index + 1,
            });
        }
    }

    private struct MeshData
    {
        public List<Vector3> Verts;
        public List<int> Tris;
        public List<Vector2> Uvs;
        public List<Color> Colors;
        public List<SubMeshDescriptor> SubMeshes;
        public List<Material> Materials;
    }

    private struct PlaneData
    {
        public Vector3 FromBottom;
        public Vector3 ToBottom;
        public Vector3 FromTop;
        public Vector3 ToTop;
        
        public Color FromBottomColor;
        public Color ToBottomColor;
        public Color FromTopColor;
        public Color ToTopColor;
    }

    #endregion
}
