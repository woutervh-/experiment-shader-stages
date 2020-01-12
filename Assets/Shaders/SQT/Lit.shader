Shader "SQT/Lit" {
    Properties {
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        [MainColor] _BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}

        [Toggle(_PER_FRAGMENT_NORMALS)] _PerFragmentNormals ("Per fragment normals", Float) = 0
        [Toggle(_FINITE_DIFFERENCE_NORMALS)] _FiniteDifferenceNormals ("Finite difference normals", Float) = 0
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
            #pragma shader_feature _PER_FRAGMENT_NORMALS
            #pragma shader_feature _FINITE_DIFFERENCE_NORMALS

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitForwardPass.hlsl"
            #include "./Noise.hlsl"

            #define VertexProgram LitPassVertex
            Varyings Vertex(Attributes input) {
                #if defined(_VERTEX_DISPLACEMENT) || defined(_PER_FRAGMENT_NORMALS)
                    float3 pointOnUnitSphere = normalize(input.positionOS.xyz);
                    float4 noiseSample = noise(pointOnUnitSphere);

                    #if defined(_VERTEX_DISPLACEMENT)
                        input.positionOS.xyz = pointOnUnitSphere * (1 + noiseSample.w);
                    #endif

                    #if defined(_PER_FRAGMENT_NORMALS)
                        input.normalOS = pointOnUnitSphere;
                    #else
                        input.normalOS = normalize(pointOnUnitSphere - noiseSample.xyz);
                    #endif
                #endif

                return LitPassVertex(input);
            }

            float4 Fragment(Varyings input) : SV_TARGET {
                #if defined(_PER_FRAGMENT_NORMALS)
                    float3 pointOnUnitSphere = normalize(TransformWorldToObjectDir(input.normalWS));

                    #if defined(_FINITE_DIFFERENCE_NORMALS)
                        #ifdef UNITY_REVERSED_Z
                            float h = 1.0 - input.positionCS.z;
                        #else
                            float h = input.positionCS.z;
                        #endif
                        h /= 8.0;
                        h *= h;
                        float3 adjustedNormal = normalize(pointOnUnitSphere - finiteDifferenceGradient(pointOnUnitSphere, h));
                    #else
                        float3 adjustedNormal = normalize(pointOnUnitSphere - noise(pointOnUnitSphere).xyz);
                    #endif

                    input.normalWS = TransformObjectToWorldNormal(adjustedNormal);
                #endif

                return LitPassFragment(input);
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
                    input.positionOS.xyz = pointOnUnitSphere * (1 + noiseSample.w);
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
                    input.position.xyz = pointOnUnitSphere * (1 + noiseSample.w);
                #endif
                
                return DepthOnlyVertex(input);
            }

            ENDHLSL
        }
    }
}
