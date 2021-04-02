#pragma once
#include <UnityShaderVariables.cginc>
#include "UTIL_Generics.cginc"
#include "UTIL_Shapes.cginc"

/// RAY
struct Ray {
    float3 Origin;
    float3 Dir;
    float3 ColorWeight;
};

inline Ray Ray_Construct(float3 o, float3 d) {
    Ray ray;
    ray.Origin = o;
    ray.Dir = d;
    ray.ColorWeight = float3(1.0f, 1.0f, 1.0f);
    return ray;
}

inline Ray Ray_FromCamera(float2 uv) {
    // Transform the camera origin to world space
    float3 origin = mul(unity_CameraToWorld, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;

    // invert the perspective projection of the view-space position
    float3 direction = mul(_CameraInverseProjection, float4(uv, 0.0f, 1.0f)).xyz;

    // Transform the direction from camera to world space and normalize;
    direction = mul(unity_CameraToWorld, float4(direction, 0.0f)).xyz;
    direction = normalize(direction);

    return Ray_Construct(origin, direction);
}

/// RAY HIT
struct Hit {
    float3 Pos;
    float Dist;
    float3 Norm;
    Material Mat;
};

inline Hit Hit_Construct() {
    Hit h;
    h.Pos = float3(0.0f, 0.0f, 0.0f);
    h.Dist = INF;
    h.Norm = float3(0.0f, 0.0f, 0.0f);
    h.Mat = Material_Construct();
    return h;
}

inline Hit Hit_Construct(Ray r, float d) {
    Hit h;
    h.Pos = r.Origin + d * r.Dir;
    h.Dist = d;
    h.Norm = float3(0.0f, 1.0f, 0.0f);
    h.Mat = Material_Construct();
    return h;
}

inline bool Collision(Ray r, AABox b) {
    float3 inv_ray_d = 1/r.Dir;
    float3 t1 = (b.Min - r.Origin) * inv_ray_d;
    float3 t2 = (b.Max - r.Origin) * inv_ray_d;
    float start = 0, end = INF;
    
    for (uint i = 0; i < 3; ++i)
    {
        if (r.Dir[i] == 0 &&
            (r.Origin[i] < t1[i] || r.Origin[i] > t2[i]))
                continue;
        if (t1[i] > start)
            start = t1[i];
        if (t2[i] < end)
            end = t2[i];
    }
    return start < end && start > EPSILON && end > EPSILON;
}