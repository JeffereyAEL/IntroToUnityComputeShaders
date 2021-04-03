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
    uint Ref;
};

/// Defining a Mesh in data
struct Mesh {
    AABox Bounds;
    float4x4 LocalToWorld;
    uint IOffset;
    uint ICount;
    Material Mat;
};

#endif