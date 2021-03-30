#pragma once

/// UNIFORMS
extern float4x4 _CameraInverseProjection;
extern float2 _Pixels;
extern float _Seed;

/// CONSTANTS
static const float PI = 3.14158265f;
static const float INF = 1. / 0.;

/// MACROS
#define UNI_FlOAT3(value) float3(value, value, value)

/// FUNCTIONS
float Rand() {
    return frac(sin(_Seed / 100.0f * dot(_Pixels, float2(12.9898f, 78.233f))) * 43758.5453f);
}

float sdot(float3 x, float3 y, float f = 1.0f)
{
    return saturate(dot(x, y) * f);
}