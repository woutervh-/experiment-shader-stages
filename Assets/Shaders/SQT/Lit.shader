Shader "SQT/Lit" {
    Properties {
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        [MainColor] _BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}

        [Toggle(_VERTEX_DISPLACEMENT)] _VertexDisplacement ("Vertex displacement", Float) = 0
        [HideInInspector] _Gradients2D ("Gradients", 2D) = "white" {}
        [HideInInspector] _Permutation2D ("Permutation", 2D) = "white" {}
        _Strength ("Strength", Float) = 1
        _Frequency ("Frequency", Float) = 1
        _Lacunarity ("Lacunarity", Float) = 2
        _Persistence ("Persistence", Float) = 0.5
        _Octaves ("Octaves", Int) = 8
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
            #pragma shader_feature _VERTEX_DISPLACEMENT

            #pragma vertex Vertex
            #pragma fragment LitPassFragment

            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitForwardPass.hlsl"
            #include "./Noise.hlsl"

            #define VertexProgram LitPassVertex
            Varyings Vertex(Attributes input) {
                #if defined(_VERTEX_DISPLACEMENT)
                    float3 pointOnUnitSphere = normalize(input.positionOS.xyz);
                    float4 noiseSample = noise(pointOnUnitSphere);
                    input.positionOS.xyz = pointOnUnitSphere * noiseSample.w;
                    input.normalOS = normalize(pointOnUnitSphere - noiseSample.xyz);
                #endif

                return LitPassVertex(input);
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

            #pragma vertex Vertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/ShadowCasterPass.hlsl"
            #include "./Noise.hlsl"

            #define VertexProgram ShadowPassVertex
            Varyings Vertex(Attributes input) {
                #if defined(_VERTEX_DISPLACEMENT)
                    float3 pointOnUnitSphere = normalize(input.positionOS.xyz);
                    float4 noiseSample = noise(pointOnUnitSphere);
                    input.positionOS.xyz = pointOnUnitSphere * noiseSample.w;
                #endif

                return ShadowPassVertex(input);
            }

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

            #pragma vertex Vertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/DepthOnlyPass.hlsl"
            #include "./Noise.hlsl"

            Varyings Vertex(Attributes input) {
                #if defined(_VERTEX_DISPLACEMENT)
                    float3 pointOnUnitSphere = normalize(input.position.xyz);
                    float4 noiseSample = noise(pointOnUnitSphere);
                    input.position.xyz = pointOnUnitSphere * noiseSample.w;
                #endif
                
                return DepthOnlyVertex(input);
            }

            ENDHLSL
        }
    }
}
