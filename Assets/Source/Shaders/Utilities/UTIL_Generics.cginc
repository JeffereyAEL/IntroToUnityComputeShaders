#pragma once

/// UNIFORMS
extern float4x4 _CameraInverseProjection;
extern float2 _Pixels;
extern float _Seed;

// CONSTANTS
/// PI to 8 decimal places
static const float PI = 3.14159265f;
/// Infinity
static const float INF = 3.402823466e+38F;//1. / 0.;
/// Negative Infinity
static const float NINF = -INF;
 
// MACROS
/// Constructs a hardcoded uniform float3
#define FlOAT3(value) float3(value, value, value)

// FUNCTIONS
/// Returns a random float
float Rand() {
    float result = frac(sin(_Seed / 100.0f * dot(_Pixels, float2(12.9898f, 78.233f))) * 43758.5453f);
    _Seed += 1.0f;
    return result;
}

/// Saturates the dot of x and y to [0.0f,1.0f] then weights with f
float sdot(float3 x, float3 y, float f = 1.0f)
{
    return saturate(dot(x, y) * f);
}

/// Averages the attributes of a vector 
float Energy(float4 v) {
    return dot(v, 1.0f / 4.0f);
}
/// Averages the attributes of a vector 
float Energy(float3 v) {
    return dot(v, 1.0f / 3.0f);
}
/// Averages the attributes of a vector 
float Energy(float2 v) {
    return dot(v, 1.0f / 2.0f);
}