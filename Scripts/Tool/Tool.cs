using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Tool
{
    protected Texture2D _icon;
    
    public abstract Texture2D Icon { get; }
    
    public abstract void Process(Map map);

    public abstract void DrawEditorGUI();
}
