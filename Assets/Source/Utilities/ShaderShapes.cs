using JetBrains.Annotations;
using UnityEngine;

namespace Source.Utilities
{
    /// <summary>
    /// A mirrored Material structure as defined in Source/Shaders/UtilityShaders/UTIL_Shapes.cginc
    /// </summary>
    public struct ShaderMaterial
    {
        [UsedImplicitly] public Vector3 Albedo;
        [UsedImplicitly] public Vector3 Specular;
        [UsedImplicitly] public Vector3 Emissive;
        [UsedImplicitly] public float Roughness;
    };

    /// <summary>
    /// A mirrored Sphere structure as defined in Source/Shaders/UtilityShaders/UTIL_Shapes.cginc
    /// </summary>
    public struct ShaderSphere
    {
        [UsedImplicitly] public Vector3 Pos;
        [UsedImplicitly] public float Rad;
        [UsedImplicitly] public ShaderMaterial Mat;
    };

    /// <summary>
    /// A mirrored Mesh structure as defined in Source/Shaders/UtilityShaders/UTIL_Shapes.cginc
    /// </summary>
    public struct ShaderMesh
    {
        [UsedImplicitly] public Matrix4x4 LocalToWorld;
        [UsedImplicitly] public int IndicesOffset;
        [UsedImplicitly] public int IndicesCount;
        [UsedImplicitly] public ShaderMaterial Mat;
    };
}
