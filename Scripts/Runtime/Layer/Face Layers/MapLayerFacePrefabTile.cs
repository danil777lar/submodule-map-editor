using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalOpus.MB.Core;
using MoreMountains.Tools;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

[Serializable]
public class MapLayerFacePrefabTile : MapLayerFace
{
    [SerializeField] private float verticalScale = 1f;
    
    [SerializeField] private bool bake;
    [SerializeField] private bool optimizeMesh = false;
    [SerializeField] private bool convexCollider = false;
    [SerializeField] private float uvScale = 1f;
    
    [SerializeField] private bool lookBackward;
    [SerializeField] private TilePrefab tilePrefab;
    [Space]
    [SerializeField] private bool useVertexColor;
    [SerializeField] private VerticalGradientVertexColorer vertexColorer = new VerticalGradientVertexColorer();
    
    [SerializeField] private List<Tool> tools;

    public override void DrawEditorGUI()
    {
        #if UNITY_EDITOR
        
        base.DrawEditorGUI();
        
        DrawEditorGUIHeader("Tile Base");
        DrawEditorGUILine(() => 
            verticalScale = UnityEditor.EditorGUILayout.FloatField("Vertical Scale", verticalScale));
        
        DrawEditorGUILine(() => 
            lookBackward = UnityEditor.EditorGUILayout.Toggle("Look Backward", lookBackward));
        DrawEditorGUILine(() => 
            tilePrefab = (TilePrefab) UnityEditor.EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(TilePrefab), false));
        
        UnityEditor.EditorGUILayout.Space();
        DrawEditorGUIHeader("Tile Bake");
        DrawEditorGUILine(() => bake = UnityEditor.EditorGUILayout.Toggle("Bake", bake));
        if (bake)
        {
            DrawEditorGUILine(() => optimizeMesh = UnityEditor.EditorGUILayout.Toggle("Optimize Mesh", optimizeMesh));
            DrawEditorGUILine(() => convexCollider = UnityEditor.EditorGUILayout.Toggle("Convex Collider", convexCollider));
            DrawEditorGUILine(() => uvScale = UnityEditor.EditorGUILayout.FloatField("UV Scale", uvScale));
        }

        if (bake)
        {
            UnityEditor.EditorGUILayout.Space();
            DrawEditorGUIHeader("Tile Vertex Color");
            DrawEditorGUILine(() =>
                useVertexColor = UnityEditor.EditorGUILayout.Toggle("Use Vertex Color", useVertexColor));
            if (useVertexColor)
            {
                foreach (Action line in vertexColorer.GetEditorGUIProperties())
                {
                    DrawEditorGUILine(() => line.Invoke());
                }
            }
        }

        UnityEditor.EditorGUILayout.Space(20f);
        
        #endif
    }

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

    public override void Build()
    {
        GameObject root = new GameObject(Name);
        root.transform.SetParent(transform);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        
        List<MeshRenderer> meshes = SpawnTiles(root.transform);
        if (bake)
        {
            
            Quaternion rotation = transform.rotation;
            transform.rotation = Quaternion.identity;
            
            root = Bake(root, meshes.Select(x => x.gameObject).ToList(), out Mesh mesh);
            transform.rotation = rotation;
            
            if (useVertexColor)
            {
                vertexColorer.ColorMesh(mesh);
            }
        }

        root.isStatic = true;
    }

    private List<MeshRenderer> SpawnTiles(Transform root)
    {
        List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        
        foreach (Tile tile in FilledTiles)
        {
            TilePrefab tileObject = GameObject.Instantiate(tilePrefab, root.transform);
            tileObject.Initialize();

            Vector3 tileFrom = TileToLocal(tile.From);
            Vector3 tileTo = TileToLocal(tile.To);

            Vector3 scale = tileObject.transform.localScale;
            scale *= Vector3.Distance(tileFrom, tileTo) / Vector3.Distance(tileObject.From.position, tileObject.To.position);
            scale = new Vector3(scale.x, scale.y * verticalScale, scale.z);
            tileObject.transform.localScale = scale;
            
            if (lookBackward)
            {
                tileObject.transform.localScale = tileObject.transform.localScale
                    .MMSetY(tileObject.transform.localScale.y * -1);
            }

            Vector3 tileObjectFrom = root.InverseTransformPoint(tileObject.From.position);
            Vector3 tileObjectTo = root.InverseTransformPoint(tileObject.To.position);

            Vector3 tileObjectCenter = (tileObjectFrom + tileObjectTo) / 2;
            Vector3 tileCenter = (tileFrom + tileTo) / 2;
            tileObject.transform.localPosition = tileCenter - tileObjectCenter;
            
            meshRenderers.AddRange(tileObject.GetComponentsInChildren<MeshRenderer>());
        }
        
        return meshRenderers;
    }

    private GameObject Bake(GameObject root, List<GameObject> objectsToBake, out Mesh outMesh)
    {
        MB3_MeshBaker baker = new GameObject("Baker").AddComponent<MB3_MeshBaker>();
        
        baker.transform.position = root.transform.position;
        baker.transform.rotation = root.transform.rotation;
        
        outMesh = null;
        
        baker.ClearMesh();
        baker.meshCombiner.renderType = MB_RenderType.meshRenderer;
        baker.meshCombiner.doUV = true;
        baker.meshCombiner.doUV1 = true;
        baker.meshCombiner.lightmapOption = MB2_LightmapOptions.copy_UV2_unchanged_to_separate_rects;
        baker.meshCombiner.doNorm = true;
        baker.meshCombiner.doTan = true;
        baker.meshCombiner.doCol = true;
        baker.meshCombiner.optimizeAfterBake = false;

        if (objectsToBake.Count > 0)
        {
            baker.AddDeleteGameObjects(objectsToBake.ToArray(), null, true);
            baker.meshCombiner.pivotLocationType = MB_MeshPivotLocation.customLocation;
            baker.meshCombiner.pivotLocation = root.transform.position;
            baker.meshCombiner.optimizeAfterBake = optimizeMesh;
            baker.Apply();
            
            MeshRenderer result = (MeshRenderer)baker.meshCombiner.targetRenderer;
            result.gameObject.name = Name;
            result.shadowCastingMode = ShadowCastingMode.On;
            
            root.transform.MMDestroyAllChildren();
            Object.DestroyImmediate(baker.gameObject);

            GameObject defaultParent = result.transform.parent.gameObject;  
            result.transform.SetParent(root.transform.parent);
            
            Object.DestroyImmediate(defaultParent);
            Object.DestroyImmediate(root);

            Mesh mesh = result.GetComponent<MeshFilter>().sharedMesh;
            
            Vector2[] uv = mesh.uv;
            for (int i = 0; i < uv.Length; i++)
            {
                uv[i] *= uvScale;
            }
            mesh.uv = uv;
            
            SaveMesh(mesh);
            outMesh = mesh;

            MeshCollider collider = result.AddComponent<MeshCollider>();
            collider.convex = convexCollider;
            collider.sharedMesh = mesh;

            return result.gameObject;
        }

        return root;
    }
}