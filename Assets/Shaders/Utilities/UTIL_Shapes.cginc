#ifndef __SHAPES__
#define __SHAPES__

/// A material struct, contains all information for lighting
struct Material {
    float3 Albedo;
    float3 Specular;
    float3 Emissive;
    float Roughness;
};

/// Default constructor for the Material struct
Material Material_Construct() {
    Material mat;
    mat.Albedo = float3(0.0f, 0.0f, 0.0f);
    mat.Specular = float3(0.0f, 0.0f, 0.0f);
    mat.Emissive = float3(0.0f, 0.0f, 0.0f);
    mat.Roughness = 0.0f;
    return mat;
}

/// Setter constructor for the Material struct
Material Material_Construct(float3 a, float3 s, float3 e, float r) {
    Material mat;
    mat.Albedo = a;
    mat.Specular = s;
    mat.Emissive = e;
    mat.Roughness = r;
    return mat;
}

/// Setter constructor for the Material struct
Material Material_Construct(Material m) {
    Material mat;
    mat.Albedo = m.Albedo;
    mat.Specular = m.Specular;
    mat.Emissive = m.Emissive;
    mat.Roughness = m.Roughness;
    return mat;
}

/// Defining a Sphere in data
struct Sphere {
    float3 Pos;
    float Rad;
    Material Mat;
};

/// Defining an Axis Aligned Box in data
struct AABox {
    float3 Min;
    float3 Max;
};

/// Defining a Mesh in data
struct Mesh {
    AABox Bounds;
    float4x4 LocalToWorld;
    uint IOffset;
    uint ICount;
    Material Mat;
};

#ifdef INCLUDE_KD_TREE

#ifndef REDEFINE_KD_TREE_MAX
#define KD_TREE_LEAF_MAX  300
#endif

#define KD_TREE_LEAF_POINTS KD_TREE_LEAF_MAX * 3
struct KdTreeNode {
    uint LChild, RChild;
    int SplitIdx;
    float SplitDist;
    uint LeafNum;
    int Leaves[KD_TREE_LEAF_POINTS];
};
struct KdTreeHitResult {
    uint LeafNum;
    int Leaves[KD_TREE_LEAF_POINTS];
};
KdTreeHitResult KdTreeHitResult_Construct(uint amt, int data[KD_TREE_LEAF_POINTS]) {
    KdTreeHitResult h;
    h.LeafNum = amt;
    for (uint i = 0; i < KD_TREE_LEAF_POINTS; ++i)
        h.Leaves[i] = data[i];
    return h;
}
int EMPTY_KD_TREE_HIT_LEAVES[KD_TREE_LEAF_POINTS];
extern StructuredBuffer<KdTreeNode> _KdTree;
#endif

#endif
