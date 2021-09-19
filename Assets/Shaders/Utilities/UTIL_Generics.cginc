#ifndef __GENERICS__
#define __GENERICS__

// ReSharper disable CppUnusedIncludeDirective
#include "UnityShaderVariables.cginc"
// ReSharper restore CppUnusedIncludeDirective

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
/// EPSILON
static const float EPSILON = 1e-8;

// MACROS
/// Constructs a uniform float3
#define FLOAT3(value) float3(value, value, value)

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

/// returns a blend of A and B based on the alpha
inline float Blend(float a, float b, float alpha) {
    return a * alpha + b * (1 - alpha);
}
/// returns a blend of A and B based on the alpha
inline float Blend(float2 a, float2 b, float alpha) {
    return a * alpha + b * (1 - alpha);
}
/// returns a blend of A and B based on the alpha
inline float Blend(float3 a, float3 b, float alpha) {
    return a * alpha + b * (1 - alpha);
}
/// returns a blend of A and B based on the alpha
inline float Blend(float4 a, float4 b, float alpha) {
    return a * alpha + b * (1 - alpha);
}

/// returns a if is_a else returns b
inline float IfBlend(int a, int b, uint is_a) {
    return a * is_a + b * (1 - is_a);
}
/// returns a if is_a else returns b
inline float IfBlend(uint a, uint b, uint is_a) {
    return a * is_a + b * (1 - is_a);
}
/// returns a if is_a else returns b
inline float IfBlend(float a, float b, uint is_a) {
    return a * is_a + b * (1 - is_a);
}
/// returns a if is_a else returns b
inline float IfBlend(float2 a, float2 b, uint is_a) {
    return a * is_a + b * (1 - is_a);
}
/// returns a if is_a else returns b
inline float IfBlend(float3 a, float3 b, uint is_a) {
    return a * is_a + b * (1 - is_a);
}
/// returns a if is_a else returns b
inline float IfBlend(float4 a, float4 b, uint is_a) {
    return a * is_a + b * (1 - is_a);
}

/// Compares floats given a margin of error epsilon
inline bool Equal(float a, float b) {
    return a > b - EPSILON && a < b + EPSILON;
}
/// Compares floats given a margin of error
inline bool Equal(float a, float b, float margin) {
    return a > b - margin && a < b + margin;
}

/// reduces a give value to a specified range (doesn't clamp)
inline float Unitize(float value, float min, float max) {
    return (value - min) / max;
}

/// returns a value that is "alpha" percent between min and max
inline float Lerp(float alpha, float min, float max) {
    return (max - min) * alpha + min;
}
#endif