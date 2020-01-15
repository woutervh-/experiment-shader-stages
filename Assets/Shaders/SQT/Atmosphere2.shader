Shader "SQT/Atmosphere" {
    Properties {
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        [MainColor] _BaseColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}

        _AtmosphereRadius ("Atmosphere radius", Float) = 1
        _PlanetRadius ("Planet radius", Float) = 0.5
        _SunIntensity ("Sun intensity", Float) = 50
        _ViewSamples ("View samples", Int) = 8
        _LightSamples ("Light samples", Int) = 8
        _ScaleFactor ("Scale factor", Float) = 6371000
        _ScaleHeightR ("Scale height Rayleigh", float) = 8500
        _ScaleHeightM ("Scale height Mie", float) = 1275
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
            float _SunIntensity;
            int _ViewSamples;
            int _LightSamples;
            float _ScaleFactor;
            float _ScaleHeightR;
            float _ScaleHeightM;

            #define PI 3.1415927410125732
            #define g 0.75

            // Rayleigh scattering coefficient β(λ, h) - β(red) β(green) and β(blue) at Earth's sea level (h=0).
            #define betaR float3(0.00000519673, 0.0000121427, 0.0000296453) * _ScaleFactor

            // Mie scattering coefficient β(λ, h) - β(red) β(green) and β(blue) at Earth's sea level (h=0).
            #define betaM float3(0.000021, 0.000021, 0.000021) * _ScaleFactor

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
                output.rayOriginOS = input.positionOS.xyz;
                output.rayDirectionOS = normalize(input.positionOS.xyz - TransformWorldToObject(_WorldSpaceCameraPos.xyz));

                return output;
            }

            float4 Fragment(Varyings input) : SV_TARGET {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                input.rayDirectionOS = normalize(input.rayDirectionOS);
                float3 sunDirection = TransformWorldToObject(normalize(_MainLightPosition.xyz));

                float tA;
                float tB;
                if (!raySphereIntersect(input.rayOriginOS, input.rayDirectionOS, input.centerOS, _AtmosphereRadius, tA, tB)) {
                    return float4(0, 0, 0, 0);
                }
 
                // Is the ray passing through the planet core?
                float pA, pB;
                if (raySphereIntersect(input.rayOriginOS, input.rayDirectionOS, input.centerOS, _PlanetRadius, pA, pB)) {
                    tB = pA;
                }

                float opticalDepthR = 0;
                float opticalDepthM = 0;
                float3 integralR = float3(0, 0, 0);
                float3 integralM = float3(0, 0, 0);
                float t = tA;
                float dt = (tB - tA) / _ViewSamples;
                for (int i = 0; i < _ViewSamples; i++) {
                    float3 p = input.rayOriginOS + input.rayDirectionOS * (t + dt * 0.5);
                    float height = distance(input.centerOS, p) - _PlanetRadius;
                    float hr = densityR(height) * dt;
                    float hm = densityM(height) * dt;
                    opticalDepthR += hr;
                    opticalDepthM += hm;

                    float opticalDepthLightR = 0;
                    float opticalDepthLightM = 0;
                    bool overground = sampleLight(p, sunDirection, input.centerOS, opticalDepthLightR, opticalDepthLightM);

                    if (overground) {
                        float tau = betaR * (opticalDepthR + opticalDepthLightR) + betaM * (opticalDepthM + opticalDepthLightM);
                        float transmittance = exp(-tau);
                        integralR += hr * transmittance;
                        integralM += hm * transmittance;
                    }

                    t += dt;
                }

                float cosTheta = dot(sunDirection, input.rayDirectionOS);
                float3 I = _SunIntensity * _MainLightColor * (betaR * phaseR(cosTheta) * integralR + betaM * phaseM(cosTheta) * integralM);

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
