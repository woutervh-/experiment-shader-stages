#ifndef ATMOSPHERE_TEST_1_INCLUDED
#define ATMOSPHERE_TEST_1_INCLUDED

#include "Packages/com.unity.render-pipelines.lightweight/Shaders/UnlitInput.hlsl"

#define PI 3.1415927410125732
#define g 0.75
#define _AtmosphereRadius _PlanetRadius + 3.0 * _ScaleHeightR / _ScaleFactor

// Rayleigh scattering coefficient β(λ, h) - β(red) β(green) and β(blue) at Earth's sea level (h=0).
#define betaR float3(0.00000519673, 0.0000121427, 0.0000296453) * _ScaleFactor

// Mie scattering coefficient β(λ, h) - β(red) β(green) and β(blue) at Earth's sea level (h=0).
#define betaM float3(0.000021, 0.000021, 0.000021) * _ScaleFactor

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

// float _AtmosphereRadius;
float _PlanetRadius;
float3 _PlanetPosition;
float _SunIntensity;
int _ViewSamples;
int _LightSamples;
float _ScaleFactor;
float _ScaleHeightR;
float _ScaleHeightM;

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

// Rayleight scattering density ratio ρ(h)
float densityR(float h) {
    return exp(-h / (_ScaleHeightR / _ScaleFactor));
}

// Mie scattering density ration ρ(h)
float densityM(float h) {
    return exp(-h / (_ScaleHeightM / _ScaleFactor));
}

bool sampleLight(float3 p, float3 s, float3 c, out float opticalDepthR, out float opticalDepthM) {
    float _;
    float tB;
    raySphereIntersect(p, s, c, _AtmosphereRadius, _, tB);

    float t = 0;
    float dt = distance(p, p + s * tB) / _LightSamples;
    for (int i = 0; i < _LightSamples; i++) {
        float3 q = p + s * (t + dt * 0.5);
        float height = distance(c, q) - _PlanetRadius;

        if (height < 0) {
            return false;
        }

        opticalDepthR += densityR(height) * dt;
        opticalDepthM += densityM(height) * dt;
    }

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

    float tA, tB;
    float pA, pB;
    bool intersectsAtmosphere = raySphereIntersect(rayOriginWS, rayDirectionWS, _PlanetPosition, _AtmosphereRadius, tA, tB);
    bool intersectsPlanet = raySphereIntersect(rayOriginWS, rayDirectionWS, _PlanetPosition, _PlanetRadius, pA, pB);
    
    if (!intersectsAtmosphere || (tA < 0 && tB < 0)) {
        // return float4(0, 1, 0, 1);
        return float4(0, 0, 0, 0);
    }
 
    // Is the ray passing through the planet core?
    if (intersectsPlanet) {
        if (pA > 0) {
            tB = pA;
            // return float4(0, 1, 1, 1);
        } else {
            tA = pB;
            // return float4(0, 0, 1, 1);
        }
    }

    if (tA < 0) {
        // return float4(1, 1, 0, 1);
    } else {
        // return float4(1, 0, 0, 1);
    }

    tA = max(0, tA);

    float opticalDepthR = 0;
    float opticalDepthM = 0;
    float3 integralR = float3(0, 0, 0);
    float3 integralM = float3(0, 0, 0);
    float t = tA;
    float dt = (tB - tA) / _ViewSamples;
    for (int i = 0; i < _ViewSamples; i++) {
        float3 p = rayOriginWS + rayDirectionWS * (t + dt * 0.5);
        float height = distance(_PlanetPosition, p) - _PlanetRadius;
        float hr = densityR(height) * dt;
        float hm = densityM(height) * dt;
        opticalDepthR += hr;
        opticalDepthM += hm;

        float opticalDepthLightR = 0;
        float opticalDepthLightM = 0;
        bool overground = sampleLight(p, lightDirection, _PlanetPosition, opticalDepthLightR, opticalDepthLightM);

        if (overground) {
            float3 tau = betaR * (opticalDepthR + opticalDepthLightR) + betaM * (opticalDepthM + opticalDepthLightM);
            float3 transmittance = exp(-tau);
            integralR += hr * transmittance;
            integralM += hm * transmittance;
        }

        t += dt;
    }

    float cosTheta = dot(lightDirection, rayDirectionWS);
    float3 I = _SunIntensity * _MainLightColor * (betaR * phaseR(cosTheta) * integralR + betaM * phaseM(cosTheta) * integralM);

    return float4(I, t);
}

#endif
