#ifndef PRECOMPUTED_RENDERER_INCLUDED
#define PRECOMPUTED_RENDERER_INCLUDED

#include "Packages/com.unity.render-pipelines.lightweight/Shaders/UnlitInput.hlsl"

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

float3 GetIntegral(float altitude, float mu, float nu) {
    float3 origin = _PlanetPosition + float3(0, altitude, 0);
    float xz = sqrt((1.0 - mu * mu) / 2.0);
    float3 direction = normalize(float3(xz, -mu, xz));

    float a0, a1;
    raySphereIntersect(origin, direction, _PlanetPosition, _AtmosphereRadius, a0, a1);

    a0 = max(0, a0);
    float t = a0;
    float dt = (a1 - a0) / 32;

    float opticalDepth = 0;
    float integral = 0;
    for (int i=0; i<32; i++) {
        float3 p = origin + direction * (t + dt * 0.5);
        float height = distance(_PlanetPosition, p);
        float h = exp(-height / (_AtmosphereRadius / 4.0)) * dt;
        opticalDepth += h;

        float opticalDepthLight = 0;
        float l0, l1;
        raySphereIntersect(p, );

        float tau = opticalDepth + opticalDepthLight;
        float transmittance = exp(-tau);
        integral += h * transmittance;

        t += dt;
    }
    return integral.xxx;
}

float4 Fragment(Varyings input) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float3 lightDirection = normalize(_MainLightPosition.xyz);
    float3 rayOriginWS = _WorldSpaceCameraPos.xyz;
    float3 rayDirectionWS = normalize(input.positionWS - _WorldSpaceCameraPos.xyz);

    float a0, a1;
    bool atmosphereHit = raySphereIntersect(rayOriginWS, rayDirectionWS, _PlanetPosition, _AtmosphereRadius, a0, a1);

    if (!atmosphereHit || a1 < 0) {
        return float4(0, 0, 0, 1);
    }

    a0 = max(0, a0);
    rayOriginWS = rayOriginWS + rayDirectionWS * a0;
    float altitude = distance(rayOriginWS, _PlanetPosition);
    float mu = dot(normalize(_PlanetPosition - rayOriginWS), rayDirectionWS);
    float nu = dot(lightDirection, rayDirectionWS);
    float3 integral = GetIntegral(altitude, mu, nu);

    return float4(integral, 1);
}

#endif
