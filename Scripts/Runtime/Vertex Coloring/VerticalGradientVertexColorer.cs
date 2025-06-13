using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VerticalGradientVertexColorer : VertexColorer
{
    [SerializeField] private float minHeight = 0f;
    [SerializeField] private float maxHeight = 1f;
    [SerializeField] private Vector3 direction = Vector3.up;
    [SerializeField] private Gradient gradient = new Gradient();
    
    public override void ColorMesh(Mesh mesh)
    {
        if (mesh == null)
        {
            Debug.LogError("Mesh is null, cannot apply vertex coloring.");
            return;
        }

        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            float height = Vector3.Dot(vertices[i], direction);
            float t = Mathf.InverseLerp(minHeight, maxHeight, height);
            colors[i] = gradient.Evaluate(t);
        }

        mesh.colors = colors;
    }
    
    public override void DrawEditorGUIProperties()
    {
        foreach (Action guiLine in GetEditorGUIProperties())
        {
            guiLine.Invoke();
        }
    }

    public override List<Action> GetEditorGUIProperties()
    {
        List<Action> actions = new List<Action>();
        #if UNITY_EDITOR
        
        actions.Add(() => minHeight = UnityEditor.EditorGUILayout.FloatField("Min Height", minHeight));
        actions.Add(() => maxHeight = UnityEditor.EditorGUILayout.FloatField("Max Height", maxHeight));
        actions.Add(() => direction = UnityEditor.EditorGUILayout.Vector3Field("Direction", direction));
        actions.Add(() => gradient = UnityEditor.EditorGUILayout.GradientField("Gradient", gradient));
        
        #endif
        return actions;
    }
}
