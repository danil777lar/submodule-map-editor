using UnityEngine;

[CreateAssetMenu(fileName = "Subwall Config", menuName = "Map Editor/Subwall Config")]
public class SubwallConfig : ScriptableObject
{
    [field: Header("Main")]
    [field: SerializeField, Range(0f, 1f)] public float AnchorBottom { get; private set; } = 0f;
    [field: SerializeField, Range(0f, 1f)] public float AnchorTop { get; private set; } = 1f;
    [field: Space]
    [field: SerializeField] public float OffsetBottom { get; private set; } = 0f;
    [field: SerializeField] public float OffsetTop { get; private set; } = 0f;
    [field: Space] 
    [field: SerializeField] public bool SmoothSteps { get; private set; } = true;
    [field: SerializeField, Min(1)] public int Steps { get; private set; } = 1;
    [field: SerializeField] public AnimationCurve WidthCurve { get; private set; } = AnimationCurve.Linear(0, 0, 1, 0);

    [field: Header("Visuals")]
    [field: SerializeField] public float UVScale { get; private set; } = 1f;
    [field: SerializeField] public Gradient VertexColorGradient { get; private set; } = new Gradient();
    [field: SerializeField] public Material Material { get; private set; }
}
