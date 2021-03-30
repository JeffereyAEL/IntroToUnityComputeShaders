using JetBrains.Annotations;
using UnityEngine;

namespace Source.Utilities
{
    /// <summary>
    /// A mirrored ShaderMaterial structure as defined in Source/Shaders/UtilityShaders/UTIL_Shapes.cginc
    /// </summary>
    public struct ShaderMaterial
    {
        [UsedImplicitly] public Vector3 Albedo;
        [UsedImplicitly] public Vector3 Specular;
        [UsedImplicitly] public Vector3 Emission;
        [UsedImplicitly] public float Roughness;
    };

    /// <summary>
    /// A mirrored sphere structure as defined in Source/Shaders/UtilityShaders/UTIL_Shapes.cginc
    /// </summary>
    public struct ShaderSphere
    {
        [UsedImplicitly] public Vector3 Pos;
        [UsedImplicitly] public float Rad;
        [UsedImplicitly] public ShaderMaterial Mat;
    };

    /// <summary>
    /// A mirrored ShaderMesh structure as defined in Source/Shaders/UtilityShaders/UTIL_Shapes.cginc
    /// </summary>
    public struct ShaderMesh
    {
        [UsedImplicitly] public Matrix4x4 LocalToWorld;
        [UsedImplicitly] public int IndicesOffset;
        [UsedImplicitly] public int IndicesCount;
    };
}
