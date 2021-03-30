// INCLUDES
#pragma once
#include "UTIL_Generics.cginc"
#include "UTIL_Ray.cginc"

// EXTERNS
extern float _PhongAlpha;

// FUNCTIONS
/// Converts a given normal into tangent space
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

/// Randomly samples from the hemisphere of a given normal
float3 SampleHemisphere(float3 normal) {
    // uniformly sample the hemisphere direction
    float cos_theta = Rand();
    float sin_theta = sqrt(max(0.0f, 1.0f - cos_theta * cos_theta));
    float phi = 2 * PI * Rand();
    float3 tangent_space_dir = float3(cos(phi) * sin_theta, sin(phi) * sin_theta, cos_theta);

    // transform the direction into world space
    return mul(tangent_space_dir, GetTangentSpace(normal));
}

/// Randomly samples from the hemisphere of a given normal w/ a deterministic alpha
float3 SampleHemisphere(float3 normal, float alpha) {
    // sample the hemisphere where the alpha determines the kind of the sampling
    float cos_theta = pow(Rand(), 1.0f / (alpha + 1.0f));
    float sin_theta = sqrt(1.0f - cos_theta * cos_theta);
    float phi = 2 * PI * Rand();
    float3 tangent_space_dir = float3(cos(phi) * sin_theta, sin(phi) * sin_theta, cos_theta);

    // transform the direction into world space
    return mul(tangent_space_dir, GetTangentSpace(normal));
}

/// Converts Roughness into a Phong Alpha
float RoughnessToPhongAlpha(float r) {
    return pow(1000.0f, (1-r) * (1-r));
}

/// Simple Diffuse shading
void LambertDiffuse(inout Ray r, Hit h) {
    r.Origin = h.Pos + h.Norm * 0.001f;
    r.Dir = SampleHemisphere(h.Norm);
    r.ColorWeight *= 2 * h.Mat.Albedo * sdot(h.Norm, r.Dir);
}

/// Simple Phone Specular
void PhongSpecular(inout Ray r, Hit h) {
    r.Origin = h.Pos + h.Norm * 0.001f;
    float3 reflected = reflect(r.Dir, h.Norm);
    r.Dir = SampleHemisphere(h.Norm);
    float3 diffuse = 2.0f * min(float3(1.0f, 1.0f, 1.0f) - h.Mat.Specular, h.Mat.Albedo);
    float3 specular = h.Mat.Specular * (_PhongAlpha + 2) * pow(sdot(r.Dir, reflected), _PhongAlpha);
    r.ColorWeight *= (diffuse + specular) * sdot(h.Norm, r.Dir);
}

/// Generalized Sample Based Diffuse and Specular
void ChanceDiffuseSpecular(inout Ray r, Hit h) {
    // Calc chances of diffuse and specular reflection
    h.Mat.Albedo = min(1.0f - h.Mat.Specular, h.Mat.Albedo);
    float spec_chance = Energy(h.Mat.Specular);
    float diff_chance = Energy(h.Mat.Albedo);
    float sum = spec_chance + diff_chance;
    spec_chance /= sum;
    diff_chance /= sum;

    // roulette-select the ray's path
    float roulette = Rand();

    
    r.Origin = h.Pos + h.Norm * 0.001f;
    if (roulette < spec_chance)
    {
        // specular reflection
        _PhongAlpha = RoughnessToPhongAlpha(h.Mat.Roughness);
        r.Dir = SampleHemisphere(reflect(r.Dir, h.Norm), _PhongAlpha);
        float f = (_PhongAlpha + 2) / (_PhongAlpha + 1);
        r.ColorWeight *= (1.0f / spec_chance) * h.Mat.Specular * sdot(h.Norm, r.Dir, f);
    }
    else
    {
        // diffuse reflection
        r.Dir = SampleHemisphere(h.Norm, 1.0f);
        //r.ColorWeight *= (1.0f / diff_chance) * 2 * h.Mat.Albedo * sdot(h.Norm, r.Dir);
        r.ColorWeight *= (1.0f / diff_chance) * h.Mat.Albedo;
    }
}
