Shader "Custom/Sphere" {
    Properties {
        _EmissionColor("Color", Color) = (0, 0, 0)
        _TessellationFactor("Tessellation Factor", Range(0, 64)) = 1
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
            "RenderPipeline" = "LightWeightPipeline"
        }

        Pass {
            Cull Off

            HLSLPROGRAM

            #pragma require geometry tessellation
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
                float _TessellationFactor;
            CBUFFER_END

            struct InputAssemblerOutput {
                float4 positionOS : POSITION;
            };

            struct VertexOutput {
                float4 positionOS : POSITION;
            };

            struct HullConstantOutput {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            struct HullPointOutput {
                float4 positionOS : POSITION;
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
                return output;
            }

            HullConstantOutput HullConstant(InputPatch<VertexOutput, 3> input) {
                HullConstantOutput output;
                output.edge[0] = _TessellationFactor;
                output.edge[1] = _TessellationFactor;
                output.edge[2] = _TessellationFactor;
                output.inside = _TessellationFactor;
                return output;
            }

            [maxtessfactor(64.0)]
            [domain("tri")]
            [partitioning("fractional_odd")]
            [outputtopology("triangle_cw")]
            [patchconstantfunc("HullConstant")]
            [outputcontrolpoints(3)]
            HullPointOutput Hull(InputPatch<VertexOutput, 3> input, uint id : SV_OutputControlPointID) {
                HullPointOutput output;
                output.positionOS = input[id].positionOS;
                return output;
            }

            [domain("tri")]
            DomainOutput Domain(HullConstantOutput patchConstant, const OutputPatch<HullPointOutput, 3> input, float3 baryCoords : SV_DomainLocation) {
                #define DOMAIN_PROGRAM_INTERPOLATE(fieldName) \
                    input[0].fieldName * baryCoords.x + \
                    input[1].fieldName * baryCoords.y + \
                    input[2].fieldName * baryCoords.z;

                float4 positionOS = DOMAIN_PROGRAM_INTERPOLATE(positionOS);

                DomainOutput output;
                output.positionCS = TransformWorldToHClip(TransformObjectToWorld(positionOS.xyz));
                return output;
            }

            [maxvertexcount(3)]
            void Geometry(triangle DomainOutput input[3], inout TriangleStream<GeometryOutput> outStream) {
                GeometryOutput output[3];
                output[0].positionCS = input[0].positionCS;
                output[1].positionCS = input[1].positionCS;
                output[2].positionCS = input[2].positionCS;

                outStream.Append(output[0]);
                outStream.Append(output[1]);
                outStream.Append(output[2]);
                outStream.RestartStrip();
            }

            float4 Fragment(GeometryOutput input) : SV_Target {
                return float4(_EmissionColor, 1);
            }

            ENDHLSL
        }
    }
}
