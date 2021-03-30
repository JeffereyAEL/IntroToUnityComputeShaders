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

/// INTERSECTIONS
void IntersectSphere(Ray r, inout Hit best, Sphere s) {
    // calc the distance along the ray where the sphere is intersected
    float3 d = r.Origin - s.Pos;
    float p1 = -dot(r.Dir, d);
    float p2sqr = p1 * p1 - dot(d,d) + s.Rad * s.Rad;
    if (p2sqr < 0)
        return;
    float p2 = sqrt(p2sqr);
    float t = p1 - p2 > 0 ? p1 - p2 : p1 + p2;
    if (t > 0 && t < best.Dist)
    {
        best = Hit_Construct(r, t);
        best.Norm = normalize(best.Pos - s.Pos);
        best.Mat = s.Mat;
    }
}