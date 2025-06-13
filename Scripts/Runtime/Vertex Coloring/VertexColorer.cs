using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class VertexColorer
{
    public abstract void ColorMesh(Mesh mesh);
    public abstract void DrawEditorGUIProperties();
    public abstract List<Action> GetEditorGUIProperties();
}
