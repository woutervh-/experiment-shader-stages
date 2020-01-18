#ifndef ATMOSPHERE_TEST_1_INCLUDED
#define ATMOSPHERE_TEST_1_INCLUDED

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

float _AtmosphereRadius;
float _PlanetRadius;
float3 _PlanetPosition;

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

float4 Fragment(Varyings input) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float3 lightDirection = normalize(_MainLightPosition.xyz);
    float3 rayOriginWS = _WorldSpaceCameraPos.xyz;
    float3 rayDirectionWS = normalize(input.positionWS - _WorldSpaceCameraPos.xyz);

    float tA;
    float tB;
    if (!raySphereIntersect(rayOriginWS, rayDirectionWS, _PlanetPosition, _AtmosphereRadius, tA, tB)) {
        return float4(0, 1, 0, 1);
    }
 
    // Is the ray passing through the planet core?
    float pA, pB;
    if (raySphereIntersect(rayOriginWS, rayDirectionWS, _PlanetPosition, _PlanetRadius, pA, pB)) {
        tB = pA;
        return float4(0, 0, 1, 1);
    }

    return half4(1, 0, 0, 1);
}

#endif
