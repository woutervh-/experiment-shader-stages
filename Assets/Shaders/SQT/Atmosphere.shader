Shader "SQT/Atmosphere" {
    Properties {
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        [MainColor] _BaseColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}

        _AtmosphereRadius ("Atmosphere radius", Float) = 1
        _PlanetRadius ("Planet radius", Float) = 0.5
        _SunIntensity ("Sun intensity", Float) =
        _ScatteringCoefficient ("Scattering coefficient", Float) =
        _ViewSamples ("View samples", Int) =
        _ScaleFactor ("Scale factor", Float) = 6371000
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
            "RenderPipeline" = "LightWeightPipeline"
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
            ZWrite Off

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

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float3 centerWS : TEXCOORD2;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _AtmosphereRadius;
            float _PlanetRadius;

            bool rayIntersect(float3 O, float3 D, float3 C, float R, out float AO, out float BO) {
                float3 L = C - O;
                float DT = dot(L, D);
                float R2 = R * R;

                float CT2 = dot(L, L) - DT * DT;
 
                if (CT2 > R2) {
                    return false;
                }
 
                float AT = sqrt(R2 - CT2);
                float BT = AT;

                AO = DT - AT;
                BO = DT + BT;
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
                output.centerWS = TransformObjectToWorld(float4(0, 0, 0, 1));

                return output;
            }

            float4 Fragment(Varyings input) : SV_TARGET {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv;
                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                float3 color = texColor.rgb * _BaseColor.rgb;
                float alpha = texColor.a * _BaseColor.a;
                AlphaDiscard(alpha, _Cutoff);

                #ifdef _ALPHAPREMULTIPLY_ON
                    color *= alpha;
                #endif

                color = MixFog(color, input.fogCoord);

                float3 totalViewSamples = 0;
                float time = tA;
                float ds = (tB - tA) / (float)(_ViewSamples);
                for (int i = 0; i < _ViewSamples; i++) {
                    float3 P = O + D * (time + ds * 0.5);
                    totalViewSamples += viewSampling(P, ds);
                    time += ds;
                }
 
                float3 I = _SunIntensity * _ScatteringCoefficient * phase * totalViewSamples;

                float tA;
                float tB;
                if (!rayIntersect(O, D, _PlanetCentre, _AtmosphereRadius, tA, tB)) {
	                return float4(0, 0, 0, 0);
                }

                float pA, pB;
                if (rayIntersect(O, D, _PlanetCentre, _PlanetRadius, pA, pB)) {
	                tB = pA;
                }

                return float4(color, alpha);
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
