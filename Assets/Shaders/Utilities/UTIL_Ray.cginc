#ifndef __RAY__
#define __RAY__

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
    float3 inv_dir = float3(1.0f / r.Dir.x, 1.0f / r.Dir.y, 1.0f / r.Dir.z);
    float3 t1, t2;
    uint i;
    float t_min = -INF, t_max = INF;
    for (i = 0; i < 3; ++i)
    {
        t1[i] = (b.Min[i] - r.Origin[i]) * inv_dir[i];
        t2[i] = (b.Max[i] - r.Origin[i]) * inv_dir[i];
    }
    t_min = max(max(min(t1.x, t2.x), min(t1.y, t2.y)), min(t1.z, t2.z));
    t_max = min(min(max(t1.x, t2.x), max(t1.y, t2.y)), max(t1.z, t2.z));

    return t_min < t_max && t_max > EPSILON;
}

void DebugCollision(inout Ray r, inout Hit best, AABox b) {
    float3 inv_dir = float3(1.0f / r.Dir.x, 1.0f / r.Dir.y, 1.0f / r.Dir.z);
    float3 t1, t2;
    uint i;
    float t_min = -INF, t_max = INF;
    for (i = 0; i < 3; ++i)
    {
        t1[i] = (b.Min[i] - r.Origin[i]) * inv_dir[i];
        t2[i] = (b.Max[i] - r.Origin[i]) * inv_dir[i];
    }
    t_min = max(max(min(t1.x, t2.x), min(t1.y, t2.y)), min(t1.z, t2.z));
    t_max = min(min(max(t1.x, t2.x), max(t1.y, t2.y)), max(t1.z, t2.z));

    if (t_min < t_max && t_max > EPSILON)
    {
        //r.ColorWeight = FLOAT3(0.0f);
        best = Hit_Construct(r, t_min);
        // best.Norm = normalize(best.Pos - r.Origin);
        best.Norm = FLOAT3(0.0f);
        if (Equal(best.Pos.x, b.Min.x))
        {
            best.Norm.x = -1.0f;
        }
        else if (Equal(best.Pos.x, b.Max.x))
        {
            best.Norm.x = 1.0f;
        }
        else if (Equal(best.Pos.y, b.Min.y))
        {
            best.Norm.y = -1.0f;
        }
        else if (Equal(best.Pos.y, b.Max.y))
        {
            best.Norm.y = 1.0f;
        }
        else if (Equal(best.Pos.z, b.Min.z))
        {
            best.Norm.z = -1.0f;
        }
        else if (Equal(best.Pos.z, b.Max.z))
        {
            best.Norm.z = 1.0f;
        }
        best.Mat = Material_Construct(float3(1,0,0), FLOAT3(.2), FLOAT3(0.0f), .2f);
    }
}
#endif