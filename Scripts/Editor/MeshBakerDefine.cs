using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

[InitializeOnLoad]
public static class MeshBakerDefine
{
    private const string DEFINE = "MESH_BAKER";

    static MeshBakerDefine()
    {
        bool hasMeshBaker = Type.GetType("MB3_MeshBaker, Assembly-CSharp") != null
                         || Type.GetType("MB3_MeshBaker, DigitalOpus.MB.Core") != null;

        NamedBuildTarget target = NamedBuildTarget.FromBuildTargetGroup(
            BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

        string defines = PlayerSettings.GetScriptingDefineSymbols(target);
        bool hasDefined = defines.Split(';').Contains(DEFINE);

        if (hasMeshBaker && !hasDefined)
        {
            PlayerSettings.SetScriptingDefineSymbols(target, defines + ";" + DEFINE);
        }
        else if (!hasMeshBaker && hasDefined)
        {
            string updated = string.Join(";", defines.Split(';').Where(d => d != DEFINE));
            PlayerSettings.SetScriptingDefineSymbols(target, updated);
        }
    }
}
