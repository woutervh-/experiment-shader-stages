Shader "Custom/Point Cloud" {
    Properties {
        _EmissionColor("Color", Color) = (0, 0, 0)
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
            "RenderPipeline" = "LightWeightPipeline"
        }

        Pass {
            Cull Off

            HLSLPROGRAM

            #pragma shader_feature _EMISSION
            #pragma vertex Vertex
            #pragma geometry Geometry
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float3 _EmissionColor;
            CBUFFER_END

            struct InputAssemblerOutput {
                float4 positionOS : POSITION;
                uint vertexId : SV_VertexID;
            };

            struct VertexOutput {
                float4 positionCS : SV_POSITION;
            };

            struct GeometryOutput {
                float4 positionCS : SV_POSITION;
            };

            VertexOutput Vertex(InputAssemblerOutput input) {
                uint x = saturate(input.vertexId & 1);
                uint y = saturate(input.vertexId & 2);
                uint z = saturate(input.vertexId & 4);
                float4 positionOS = float4(x, y, z, 1);

                VertexOutput output;
                output.positionCS = TransformWorldToHClip(TransformObjectToWorld(positionOS.xyz));
                return output;
            }

            [maxvertexcount(6)]
            void Geometry(point VertexOutput input[1], inout TriangleStream<GeometryOutput> outStream) {
                GeometryOutput output[4];
                output[0].positionCS = input[0].positionCS;
                output[1].positionCS = input[0].positionCS;
                output[2].positionCS = input[0].positionCS;
                output[3].positionCS = input[0].positionCS;
                output[0].positionCS.xy += float2(-0.1, -0.1);
                output[1].positionCS.xy += float2(-0.1, 0.1);
                output[2].positionCS.xy += float2(0.1, -0.1);
                output[3].positionCS.xy += float2(0.1, 0.1);

                outStream.Append(output[0]);
                outStream.Append(output[3]);
                outStream.Append(output[2]);
                outStream.RestartStrip();

                outStream.Append(output[0]);
                outStream.Append(output[3]);
                outStream.Append(output[1]);
                outStream.RestartStrip();
            }

            float4 Fragment(GeometryOutput input) : SV_Target {
                return float4(_EmissionColor, 1);
            }

            ENDHLSL
        }
    }
}
