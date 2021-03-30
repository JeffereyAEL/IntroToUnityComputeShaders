#pragma once
#include <UnityShaderVariables.cginc>
#include "UTIL_Generics.cginc"
#include "UTIL_Shapes.cginc"

extern StructuredBuffer<Mesh> _Meshes;
extern StructuredBuffer<float3> _Vertices;
extern StructuredBuffer<int> _Indices;

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

bool IntersectTriangle(Ray r, Triangle tri, inout float t, inout float u, inout float v) {
    // create two edges;
    float3 edge1 = tri.b - tri.a;
    float3 edge2 = tri.c - tri.a;

    // begin calc determinants
    float3 pvec = cross(r.Dir, edge2);

    // if determinant is near zoer, ray lies in plane of triangle
    float det = dot(edge1, pvec);

    // use backface culling
    if (det < EPSILON)
        return false;
    float inv_det = 1.0f / det;

    // calc distance from tri.a to r.Origin
    float3 tvec = r.Origin - tri.a;

    // calc the U parameter and test bounds
    u = dot(tvec, pvec) * inv_det;
    if (u < 0.0f || u > 1.0f)
        return false;

    // prepare to test v parameter
    float3 qvec = cross(tvec, tri.b);

    // calc v parameter and test bounds
    v = dot(r.Dir, qvec) * inv_det;
    if (v < 0.0f || u + v > 1.0f)
        return false;

    // calc t, ray intersects triangle
    t = dot(edge2, qvec) * inv_det;

    return true;
}

void IntersectMeshObject(Ray r, inout Hit best, Mesh mesh)
{
    uint offset = mesh.IOffset;
    uint count = offset + mesh.ICount;
    for (uint i = offset; i < count; i += 3)
    {
        Triangle tri;
        tri.a = (mul(mesh.LocalToWorld, float4(_Vertices[_Indices[i]], 1))).xyz;
        tri.b = (mul(mesh.LocalToWorld, float4(_Vertices[_Indices[i + 1]], 1))).xyz;
        tri.c = (mul(mesh.LocalToWorld, float4(_Vertices[_Indices[i + 2]], 1))).xyz;
            
        float t, u, v;
        if (IntersectTriangle(r, tri, t, u, v))
        {
            if (t > 0 && t < best.Dist)
            {
                // TODO: replace the position calc with barycentric coordinates instead of t & position
                best = Hit_Construct(r, t);
                best.Norm = normalize(cross(tri.b - tri.a, tri.c - tri.a));
                best.Mat.Albedo = 0.0f;
                best.Mat.Specular = 0.65f;
                best.Mat.Roughness = 0.01f;
                best.Mat.Emission = 0.0f;
            }
        }
    }
}