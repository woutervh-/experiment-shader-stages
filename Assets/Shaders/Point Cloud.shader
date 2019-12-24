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
            // Name "ForwardLit"

            Cull Off

            HLSLPROGRAM

            #pragma shader_feature _EMISSION
            #pragma vertex Vertex
            #pragma hull Hull
            #pragma domain Domain
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
                float4 positionOS : POSITION;
                uint vertexId : TEXCOORD0;
            };

            struct HullConstantOutput {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            struct HullPointOutput {
                float4 positionOS : POSITION;
                uint vertexId : SV_VertexID;
            };

            struct DomainOutput {
                float4 positionCS : SV_POSITION;
            };

            struct GeometryOutput {
                float4 positionCS : SV_POSITION;
            };

            VertexOutput Vertex(InputAssemblerOutput input) {
                VertexOutput output;
                output.positionOS = input.positionOS;
                output.vertexId = input.vertexId;
                return output;
            }

            HullConstantOutput HullConstant(InputPatch<VertexOutput, 3> input) {
                HullConstantOutput output;
                output.edge[0] = 1.0;
                output.inside = 1.0;
                return output;
            }

            [maxtessfactor(64.0)]
            [domain("point")]
            [partitioning("fractional_odd")]
            [outputtopology("point")]
            [patchconstantfunc("HullConstant")]
            [outputcontrolpoints(1)]
            HullPointOutput Hull(InputPatch<VertexOutput, 1> input, uint id : SV_OutputControlPointID) {
                HullPointOutput output;
                output.positionOS = input[id].positionOS;
                output.vertexId = input[id].vertexId;
                return output;
            }

            [domain("point")]
            DomainOutput Domain(HullConstantOutput patchConstant, const OutputPatch<HullPointOutput, 1> input, float3 baryCoords : SV_DomainLocation) {
                uint x = saturate(input[0].vertexId & 1);
                uint y = saturate(input[0].vertexId & 2);
                uint z = saturate(input[0].vertexId & 4);
                float4 positionOS = float4(x, y, z, 1);

                DomainOutput output;
                output.positionCS = TransformWorldToHClip(TransformObjectToWorld(input[0].positionOS.xyz));
                return output;
            }

            [maxvertexcount(6)]
            void Geometry(point DomainOutput input[1], inout TriangleStream<GeometryOutput> outStream) {
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
