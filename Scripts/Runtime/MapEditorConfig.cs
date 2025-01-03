using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapEditorConfig", menuName = "Larje/Map Editor/MapEditorConfig")]
public class MapEditorConfig : ScriptableObject
{
    private static MapEditorConfig _instance;
    private static MapEditorConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                try
                {
                    _instance = Resources.Load<MapEditorConfig>("MapEditorConfig");
                }
                catch { }
            }

            return _instance;
        }
    }

    public static Color Hover => Instance != null ? Instance.hoverColor : Color.black;
    public static Color GridEbabled => Instance != null ? Instance.gridEnabled : Color.black;
    public static Color[] FilledTile => Instance != null ? Instance.filledTileColors : new []{Color.black};
    
    
    [SerializeField] private Color hoverColor = Color.yellow.SetAlpha(0.5f);
    
    [SerializeField] private Color gridEnabled = Color.white;

    [SerializeField] private Color[] filledTileColors;

}
