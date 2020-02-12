#ifndef PRECOMPUTED_RENDERER_INCLUDED
#define PRECOMPUTED_RENDERER_INCLUDED

#include "Packages/com.unity.render-pipelines.lightweight/Shaders/UnlitInput.hlsl"

#define PI 3.1415927410125732
#define g 0.75

struct Attributes {
    float4 positionOS : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings {
    float4 vertex : SV_POSITION;
    float3 positionWS : TEXCOORD0;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

float _PlanetRadius;
float _AtmosphereRadius;
float3 _PlanetPosition;
float _SunIntensity;
float _ScaleFactor;

bool raySphereIntersect(float3 rayOrigin, float3 rayDirection, float3 sphereCenter, float sphereRadius, out float t0, out float t1) {
    float3 L = sphereCenter - rayOrigin;
    float tca = dot(L, rayDirection);
    float r2 = sphereRadius * sphereRadius;
    float d2 = dot(L, L) - tca * tca;
 
    if (d2 > r2) {
        return false;
    }
 
    float thc = sqrt(r2 - d2);
    t0 = tca - thc;
    t1 = tca + thc;

    return true;
}

// Rayleigh phase function γ(θ)
float phaseR(float cosTheta) {
    return 3.0 / (16.0 * PI) * (1.0 + cosTheta * cosTheta);
}

// Henyey-Greenstein phase function γ(θ)
float phaseM(float cosTheta) {
    return (1.0 - g * g) / ((4.0 * PI) * pow(1.0 + g * g - 2.0 * g * cosTheta, 1.5));
}

Varyings Vertex(Attributes input) {
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    output.vertex = vertexInput.positionCS;
    output.positionWS = vertexInput.positionWS;

    return output;
}

float3 GetIntegral(float altitude, float mu) {
    const int steps = 128;

    float3 origin = _PlanetPosition + float3(0, altitude, 0);
    float xz = sqrt((1.0 - mu * mu) / 2.0);
    float3 direction = normalize(float3(xz, -mu, xz));

    float a0, a1;
    raySphereIntersect(origin, direction, _PlanetPosition, _AtmosphereRadius, a0, a1);

    a0 = max(0, a0);
    float t = a0;
    float dt = (a1 - a0) / steps;

    float opticalDepth = 0;
    float integral = 0;
    for (int i=0; i<steps; i++) {
        float3 p = origin + direction * (t + dt * 0.5);
        float height = distance(_PlanetPosition, p);
        float h = exp(-height / (_AtmosphereRadius / 4.0)) * dt;
        opticalDepth += h;

        bool overground = distance(p, _PlanetPosition) >= _PlanetRadius;

        if (overground) {
            float tau = opticalDepth;
            float transmittance = exp(-tau);
            integral += h * transmittance;
        }

        t += dt;
    }
    return integral.xxx;
}

float4 Fragment(Varyings input) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float3 lightDirection = normalize(_MainLightPosition.xyz);
    float3 rayDirectionWS = normalize(input.positionWS - _WorldSpaceCameraPos.xyz);

    float a0, a1;
    bool atmosphereHit = raySphereIntersect(_WorldSpaceCameraPos.xyz, rayDirectionWS, _PlanetPosition, _AtmosphereRadius, a0, a1);
    float p0, p1;
    bool planetHit = raySphereIntersect(_WorldSpaceCameraPos.xyz, rayDirectionWS, _PlanetPosition, _PlanetRadius, p0, p1);

    if (!atmosphereHit || a1 < 0) {
        return float4(0, 0, 0, 1);
    }

    a0 = max(0, a0);
    p0 = max(0, p0);
    // float3 rayOriginWS = _WorldSpaceCameraPos.xyz + rayDirectionWS * a0;
    float3 rayOriginA = _WorldSpaceCameraPos.xyz + rayDirectionWS * a0;
    float3 rayOriginP = _WorldSpaceCameraPos.xyz + rayDirectionWS * p0;
    float3 rayDestinationA = _WorldSpaceCameraPos.xyz + rayDirectionWS * a1;
    float3 rayDestinationP = _WorldSpaceCameraPos.xyz + rayDirectionWS * p1;
    float altitude = distance(rayOriginA, _PlanetPosition);
    float mu = dot(normalize(_PlanetPosition - rayOriginA), rayDirectionWS);
    // float nu = dot(lightDirection, rayDirectionWS);

    // float H = _AtmosphereRadius - dot(rayOriginWS - _PlanetPosition, -lightDirection);
    // float sphericalCapVolume = PI * H * H / 3.0 * (3.0 * _AtmosphereRadius - H);
    // float3 integral = sphericalCapVolume.xxx;

    // float h = dot(rayOriginWS - _PlanetPosition, -lightDirection);
    // float Ha = _AtmosphereRadius - h;
    // float Hp = _PlanetRadius - h;
    // float Va = PI * Ha * Ha / 3.0 * (3.0 * _AtmosphereRadius - Ha);
    // float Vp = PI * Hp * Hp / 3.0 * (3.0 * _PlanetRadius - Hp);
    // float3 integral = (Va - Vp).xxx;

    // float Ha = _AtmosphereRadius - dot(rayOriginWS - _PlanetPosition, -lightDirection);
    // float Aa = _AtmosphereRadius * _AtmosphereRadius * acos(1.0 - Ha / _AtmosphereRadius) - (_AtmosphereRadius - Ha) * sqrt(2.0 * _AtmosphereRadius * Ha - Ha * Ha);
    // float3 integral = Aa.xxx;

    // float3 p0 = _WorldSpaceCameraPos.xyz + rayDirectionWS * a0;
    // float3 p1 = _WorldSpaceCameraPos.xyz + rayDirectionWS * a1;
    // float h = distance(p0, _PlanetPosition) * distance(p1, _PlanetPosition) / distance(p0, p1);

    // float ha = dot(rayOriginA - _PlanetPosition, -lightDirection);
    float ha = dot((rayOriginA + rayDestinationA) / 2.0 - _PlanetPosition, -lightDirection);
    float Ha = _AtmosphereRadius - ha;
    // float Aa = _AtmosphereRadius * _AtmosphereRadius * acos(1.0 - Ha / _AtmosphereRadius) - (_AtmosphereRadius - Ha) * sqrt(2.0 * _AtmosphereRadius * Ha - Ha * Ha);
    // float Va = PI * Ha * Ha / 3.0 * (3.0 * _AtmosphereRadius - Ha);

    float Hp = _PlanetRadius;
    // float Ap = 0.0;
    // float Vp = 0.0;
    if (planetHit) {
        // float hp = dot(rayOriginP - _PlanetPosition, -lightDirection);
        float hp = dot((rayOriginP + rayDestinationP) / 2.0 - _PlanetPosition, -lightDirection);
        float Hp = _PlanetRadius - hp;
        // float Ap = _PlanetRadius * _PlanetRadius * acos(1.0 - Hp / _PlanetRadius) - (_PlanetRadius - Hp) * sqrt(2.0 * _PlanetRadius * Hp - Hp * Hp);
        // float Vp = PI * Hp * Hp / 3.0 * (3.0 * _PlanetRadius - Hp);
    }

    float cosTheta = dot(lightDirection, rayDirectionWS);
    float3 integral = GetIntegral(altitude, mu);
    integral *= (Ha - Hp) / _AtmosphereRadius;
    integral *= _SunIntensity;
    integral = integral * phaseR(cosTheta) + integral * phaseM(cosTheta);

    return float4(integral, 1);
}

#endif
