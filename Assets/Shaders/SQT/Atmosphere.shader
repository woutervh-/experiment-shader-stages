Shader "SQT/Atmosphere" {
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
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "./Atmosphere.hlsl"

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
