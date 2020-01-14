Shader "SQT/Atmosphere" {
    Properties {
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        [MainColor] _BaseColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}

        _AtmosphereRadius ("Atmosphere radius", Float) = 1
        _PlanetRadius ("Planet radius", Float) = 0.5
        // _SunIntensity ("Sun intensity", Float) = 1
        // _ScatteringCoefficient ("Scattering coefficient", Float) = 1
        // _ViewSamples ("View samples", Int) = 8
        // _LightSamples ("Light samples", Int) = 8
        // _ScaleHeight ("Scale height", float) = 8500
        // _ScaleFactor ("Scale factor", Float) = 6371000
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
            "RenderPipeline" = "LightWeightPipeline"
            "Queue" = "Transparent"
        }

        Pass {
            Name "ForwardLit"
            Tags {
                "LightMode" = "LightWeightForward"
            }

            Blend Off
            ZWrite On

            HLSLPROGRAM

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitForwardPass.hlsl"

            Varyings Vertex(Attributes input) {
                return LitPassVertex(input);
            }

            float4 Fragment(Varyings input) : SV_TARGET {
                return LitPassFragment(input);
            }

            ENDHLSL
        }

        Pass {
            Tags {
                "RenderType" = "Transparent"
                "Queue" = "Transparent"
            }

            Blend One One
            ZWrite On

            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitInput.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float3 centerOS : TEXCOORD2;
                float3 rayOriginOS : TEXCOORD3;
                float3 rayDirectionOS : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _AtmosphereRadius;
            float _PlanetRadius;
            // float _SunIntensity;
            // float _ScatteringCoefficient;
            // int _ViewSamples;
            // int _LightSamples;
            // float _ScaleHeight;
            // float _ScaleFactor;

            #define PI 3.1415927410125732
            #define SCALE_FACTOR 6371000
            #define SCALE_HEIGHT (8500 / SCALE_FACTOR)
            #define VIEW_SAMPLES 16
            #define LIGHT_SAMPLES 8
            #define SUN_INTENSITY 1

            // Rayleigh scattering coefficient β(λ, h) - β(red) β(green) and β(blue) at Earth's sea level (h=0).
            #define betaR float3(0.00000519673, 0.0000121427, 0.0000296453) * SCALE_FACTOR

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
                input.positionOS.xyz += input.normalOS * (_AtmosphereRadius - _PlanetRadius);

                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                output.centerOS = float3(0, 0, 0);
                // output.rayDirectionOS = TransformWorldToObject(-(_WorldSpaceCameraPos.xyz - vertexInput.positionWS));
                // output.rayOriginOS = input.positionOS.xyz - output.rayDirection;
                output.rayOriginOS = input.positionOS.xyz;
                output.rayDirectionOS = input.positionOS.xyz - TransformWorldToObject(_WorldSpaceCameraPos.xyz);
                output.rayDirectionOS = normalize(output.rayDirectionOS);

                return output;
            }

            // Rayleigh phase function γ(θ)
            float phase(float cosTheta) {
                return 3.0 / (16.0 * PI) * (1.0 + cosTheta * cosTheta);
            }

            // Density ratio ρ(h)
            float density(float h) {
                return exp(-h / SCALE_HEIGHT);
            }

            bool sampleLight(float3 p, float3 s, float3 c, out float opticalDepthR) {
                float _;
                float tB;
                raySphereIntersect(p, s, c, _AtmosphereRadius, _, tB);

                float t = 0;
                float dt = distance(p, p + s * tB) / LIGHT_SAMPLES;
                for (int i = 0; i < LIGHT_SAMPLES; i++) {
                    float3 q = p + s * (t + dt * 0.5);
                    float height = distance(c, q) - _PlanetRadius;

                    if (height < 0) {
                        return false;
                    }

                    opticalDepthR += density(height) * dt;
                }

                return true;
            }

            float4 Fragment(Varyings input) : SV_TARGET {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                input.rayDirectionOS = normalize(input.rayDirectionOS);
                float3 sunDirection = normalize(_MainLightPosition.xyz); // TODO: transform to object space

                // O - position, origin of ray: rayOriginOS
                // L - light direction: _MainLightPosition.xyz
                // V - view direction: -rayDirectionOS
                // N - surface normal: unused
                // S - sun light direction: L
                // D - view direction through the atmosphere away from the camera: -V = rayDirectionOS
                // C - center of planet: centerOS

                float tA; // Atmosphere entry point (worldPos + V * tA)
                float tB; // Atmosphere exit point (worldPos + V * tB)
                if (!raySphereIntersect(input.rayOriginOS, input.rayDirectionOS, input.centerOS, _AtmosphereRadius, tA, tB)) {
                    return float4(0, 0, 0, 0); // The view rays is looking into deep space.
                }
 
                // Is the ray passing through the planet core?
                float pA, pB;
                if (raySphereIntersect(input.rayOriginOS, input.rayDirectionOS, input.centerOS, _PlanetRadius, pA, pB)) {
                    tB = pA;
                }

                float opticalDepthR = 0;
                float3 integralR = float3(0, 0, 0);
                float t = tA;
                float dt = (tB - tA) / VIEW_SAMPLES;
                for (int i = 0; i < VIEW_SAMPLES; i++) {
                    float3 p = input.rayOriginOS + input.rayDirectionOS * (t + dt * 0.5);

                    float height = distance(input.centerOS, p) - _PlanetRadius;
                    float hr = density(height) * dt;
                    opticalDepthR += hr;

                    float opticalDepthLightR = 0;
                    bool overground = sampleLight(p, sunDirection, input.centerOS, opticalDepthLightR);

                    if (overground) {
                        float transmittance = exp(-betaR * (opticalDepthR + opticalDepthLightR));
                        integralR += hr * transmittance;
                    }

                    t += dt;
                }

                float cosTheta = saturate(dot(sunDirection, input.rayDirectionOS));
                float3 I = SUN_INTENSITY * betaR * phase(cosTheta) * integralR;

                return float4(I, 1);
            }

            ENDHLSL
        }

        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #pragma shader_feature _VERTEX_DISPLACEMENT

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/ShadowCasterPass.hlsl"

            ENDHLSL
        }

        Pass {
            Name "DepthOnly"
            Tags {
                "LightMode" = "DepthOnly"
            }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM

            #pragma shader_feature _VERTEX_DISPLACEMENT

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/DepthOnlyPass.hlsl"

            ENDHLSL
        }
    }
}
