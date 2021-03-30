#pragma once

struct Material {
    float3 Albedo;
    float3 Specular;
};

Material Material_Construct() {
    Material mat;
    mat.Albedo = UNI_FlOAT3(0.0f);
    mat.Specular = UNI_FlOAT3(0.0f);
    return mat;
}

Material Material_Construct(float3 a, float3 s) {
    Material mat;
    mat.Albedo = a;
    mat.Specular = s;
    return mat;
}

float3x3 GetTangentSpace(float3 normal) {
    // Choose a helper vector for the cross product
    float3 helper = float3(1, 0, 0);
    if (abs(normal.x) > 0.99f)
        helper = float3(0, 0, 1);
    
    // Generate vectors
    float3 tangent = normalize(cross(normal, helper));
    float3 binormal = normalize(cross(normal, tangent));
    return float3x3(tangent, binormal, normal);
}

float3 SampleHemisphere(float3 normal) {
    // uniformly sample the hemisphere direction
    float cos_theta = Rand();
    float sin_theta = sqrt(max(0, 1.0f - cos_theta * cos_theta));
    float phi = 2.0f * PI * Rand();
    float3 tangent_space_dir = float3(cos(phi) * sin_theta, sin(phi) * sin_theta, cos_theta);

    // transform the direction into world space
    return mul(tangent_space_dir, GetTangentSpace(normal));
}