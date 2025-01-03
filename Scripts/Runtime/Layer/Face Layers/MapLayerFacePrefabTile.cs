using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DigitalOpus.MB.Core;
using MoreMountains.Tools;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

[Serializable]
public class MapLayerFacePrefabTile : MapLayerFace
{
    [SerializeField] private bool bake;
    [SerializeField] private bool lookBackward;
    [SerializeField] private TilePrefab tilePrefab;
    
    [SerializeField] private List<Tool> tools;

    public override void DrawEditorGUI()
    {
        base.DrawEditorGUI();
        
        DrawEditorGUIHeader("Tile Settings");
        DrawEditorGUILine(() => 
            bake = EditorGUILayout.Toggle("Bake", bake));
        DrawEditorGUILine(() => 
            lookBackward = EditorGUILayout.Toggle("Look Backward", lookBackward));
        DrawEditorGUILine(() => 
            tilePrefab = (TilePrefab) EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(TilePrefab), false));
        
        EditorGUILayout.Space(20f);
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
            root = Bake(root, meshes.Select(x => x.gameObject).ToList());
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

            tileObject.transform.localScale *= Vector3.Distance(tileFrom, tileTo)
                                               / Vector3.Distance(tileObject.From.position, tileObject.To.position);

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

    private GameObject Bake(GameObject root, List<GameObject> objectsToBake)
    {
        MB3_MeshBaker baker = new GameObject("Baker").AddComponent<MB3_MeshBaker>();
        
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
            SaveMesh(mesh);

            return result.gameObject;
        }

        return root;
    }
}