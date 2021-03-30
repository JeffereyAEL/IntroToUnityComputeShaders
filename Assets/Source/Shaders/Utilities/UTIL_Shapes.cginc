#pragma once

/// A material struct, contains all information for lighting
struct Material {
    float3 Albedo;
    float3 Specular;
    float3 Emission;
    float Roughness;
};

/// Default constructor for the Material struct
Material Material_Construct() {
    Material mat;
    mat.Albedo = float3(0.0f, 0.0f, 0.0f);
    mat.Specular = float3(0.0f, 0.0f, 0.0f);
    mat.Emission = float3(0.0f, 0.0f, 0.0f);
    mat.Roughness = 0.0f;
    return mat;
}

/// Setter constructor for the Material struct
Material Material_Construct(float3 a, float3 s, float3 e, float r) {
    Material mat;
    mat.Albedo = a;
    mat.Specular = s;
    mat.Emission = e;
    mat.Roughness = r;
    return mat;
}

/// Defining a Sphere in data
struct Sphere {
    float3 Pos;
    float Rad;
    Material Mat;
};