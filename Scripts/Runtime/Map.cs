using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using UnityEngine;

public class Map : MonoBehaviour
{
    public MapLayer CurrentLayer;
    
    [SerializeReference] public List<MapLayer> Layers = new List<MapLayer>();

    public void Build()
    {
        gameObject.isStatic = true;
        
        List<Map> mapsToRebuild = new List<Map>();
        List<GameObject> objectsToDestroy = new List<GameObject>();
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out Map map))
            {
                mapsToRebuild.Add(map);
            }
            else
            {
                objectsToDestroy.Add(child.gameObject);
            }
        }
        
        objectsToDestroy.ForEach(DestroyImmediate);
        mapsToRebuild.ForEach(x => x.Build());
        
        Layers.ForEach(x =>
        {
            if (x.FilledTiles.Count > 0)
            {
                x.Build();
            }
        });
    }

    public void DetachLayer(MapLayer layer)
    {
        if (Layers.Contains(layer))
        {
            Map map = new GameObject(layer.Name).AddComponent<Map>();
            map.transform.SetParent(transform);
            map.transform.localPosition = Vector3.zero;
            map.transform.localRotation = Quaternion.identity;
            map.transform.localScale = Vector3.one;
            
            map.Layers.Add(layer);
            Layers.Remove(layer);
        }
    }
}
