Shader "Atmosphere Tests/Atmosphere Test 1" {
    Properties {
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        [MainColor] _BaseColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}

        _AtmosphereRadius ("Atmosphere radius", Float) = 1
        _PlanetRadius ("Planet radius", Float) = 0.5
        _SunIntensity ("Sun intensity", Float) = 50
        _ViewSamples ("View samples", Int) = 16
        _LightSamples ("Light samples", Int) = 8
        _ScaleFactor ("Scale factor", Float) = 6371000
        _ScaleHeightR ("Scale height Rayleigh", float) = 8500
        _ScaleHeightM ("Scale height Mie", float) = 1275
    }

    SubShader {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "LightWeightPipeline"
        }

        Pass {
            Tags {
                "RenderType" = "Transparent"
                "Queue" = "Transparent"
            }

            Blend One One
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/UnlitInput.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            Varyings Vertex(Attributes input) {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertexInput.positionCS;

                return output;
            }

            float4 Fragment(Varyings input) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                return half4(1, 0, 0, 1);
            }


            ENDHLSL
        }
    }
}
