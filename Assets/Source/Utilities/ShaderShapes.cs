using System.Collections.Generic;
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
    /// A mirrored Axis Aligned Box structure as defined in Source/Shaders/UtilityShaders/UTIL_Shapes.cginc
    /// </summary>
    public struct ShaderAABox
    {
        [UsedImplicitly] public Vector3 Min;
        [UsedImplicitly] public Vector3 Max;
    }
    
    /// <summary>
    /// A mirrored Mesh structure as defined in Source/Shaders/UtilityShaders/UTIL_Shapes.cginc
    /// </summary>
    public struct ShaderMesh
    {
        [UsedImplicitly] public ShaderAABox Bounds;
        [UsedImplicitly] public Matrix4x4 LocalToWorld;
        [UsedImplicitly] public int IndicesOffset;
        [UsedImplicitly] public int IndicesCount;
        [UsedImplicitly] public ShaderMaterial Mat;
    };
  
    public struct ShaderKdTreeNode
    {
        [UsedImplicitly] public int LChild, RChild; // indx in buffer
        [UsedImplicitly] public int SplitIdx; // either x, y, or z 
        [UsedImplicitly] public float SplitDist;
        [UsedImplicitly] public int LeafNum;
        [UsedImplicitly] public int Leaves;
    }

    public static class Helpers
    {
        // private void SplitKdSpace();
        //
        // public delegate int Comparison<in ShaderKdTree>(ShaderKdTree X, ShaderKdTree Y) {
        //     return 
        // }
        // public static List<ShaderKdTreeNode> GenerateKDTreeMap(List<Vector3> Points)
        // {
        //     Points.Sort();
        // }
    }
}
