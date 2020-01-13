Shader "SQT/Lit" {
    Properties {
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        [MainColor] _BaseColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}
        _SlopeMap ("Slope albedo", 2D) = "white" {}
        _AltitudeMap ("Altitude albedo", 2D) = "white" {}
        [Toggle(_NORMALMAP)] _NormalMap ("Bump", Float) = 0
        _BumpScale ("Bump scale", Float) = 1.0
        _BumpMap ("Bump map", 2D) = "bump" {}
        _SlopeBumpMap ("Slope bump map", 2D) = "bump" {}
        _AltitudeBumpMap ("Altitude bump map", 2D) = "bump" {}
        [Toggle(_TRIPLANAR_MAPPING)] _TriplanarMapping ("Triplanar mapping", Float) = 0
        _MapScale ("Map scale", Float) = 1.0

        [Toggle(_PER_FRAGMENT_NORMALS)] _PerFragmentNormals ("Per fragment normals", Float) = 0
        [Toggle(_PER_FRAGMENT_HEIGHT)] _PerFragmentHeight ("Per fragment height", Float) = 0
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

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _VERTEX_DISPLACEMENT
            #pragma shader_feature _PER_FRAGMENT_NORMALS
            #pragma shader_feature _PER_FRAGMENT_HEIGHT
            #pragma shader_feature _FINITE_DIFFERENCE_NORMALS
            #pragma shader_feature _TRIPLANAR_MAPPING

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #pragma vertex Vertex
            #pragma fragment Fragment

            #define _ADDITIONAL_LIGHTS // This is to include positionWS in Varyings.

            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitForwardPass.hlsl"
            #include "./Noise.hlsl"

            float _MapScale;
            TEXTURE2D(_SlopeMap); SAMPLER(sampler_SlopeMap);
            TEXTURE2D(_SlopeBumpMap); SAMPLER(sampler_SlopeBumpMap); // TODO: use me
            TEXTURE2D(_AltitudeMap); SAMPLER(sampler_AltitudeMap);
            TEXTURE2D(_AltitudeBumpMap); SAMPLER(sampler_AltitudeBumpMap); // TODO: use me

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
                float3 positionOS = TransformWorldToObject(input.positionWS);
                float3 pointOnUnitSphere = normalize(positionOS);

                #if defined(_PER_FRAGMENT_NORMALS) || defined(_PER_FRAGMENT_HEIGHT)
                    float4 noiseSample = noise(pointOnUnitSphere);
                #endif

                #if defined(_PER_FRAGMENT_HEIGHT)
                    positionOS = pointOnUnitSphere * (1 + noiseSample.w);
                #endif

                #if defined(_PER_FRAGMENT_NORMALS)
                    #if defined(_FINITE_DIFFERENCE_NORMALS)
                        #ifdef UNITY_REVERSED_Z
                            float h = 1.0 - input.positionCS.z;
                        #else
                            float h = input.positionCS.z;
                        #endif
                        h /= 8.0;
                        h *= h;
                        float3 normalOS = normalize(pointOnUnitSphere - finiteDifferenceGradient(pointOnUnitSphere, h));
                    #else
                        float3 normalOS = normalize(pointOnUnitSphere - noiseSample.xyz);
                    #endif

                    input.normalWS.xyz = TransformObjectToWorldNormal(normalOS);
                #else
                    float3 normalOS = TransformWorldToObjectDir(input.normalWS);
                #endif

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                SurfaceData surfaceData;
                InitializeStandardLitSurfaceData(input.uv, surfaceData);

                #if defined(_TRIPLANAR_MAPPING)
                    float3 bf = normalize(abs(normalOS));
                    bf /= bf.x + bf.y + bf.z;
                    float2 tx = positionOS.yz * _MapScale;
                    float2 ty = positionOS.zx * _MapScale;
                    float2 tz = positionOS.xy * _MapScale;

                    float4 cx = SampleAlbedoAlpha(tx, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)) * bf.x;
                    float4 cy = SampleAlbedoAlpha(ty, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)) * bf.y;
                    float4 cz = SampleAlbedoAlpha(tz, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)) * bf.z;
                    float4 sx = SampleAlbedoAlpha(tx, TEXTURE2D_ARGS(_SlopeMap, sampler_SlopeMap)) * bf.x;
                    float4 sy = SampleAlbedoAlpha(ty, TEXTURE2D_ARGS(_SlopeMap, sampler_SlopeMap)) * bf.y;
                    float4 sz = SampleAlbedoAlpha(tz, TEXTURE2D_ARGS(_SlopeMap, sampler_SlopeMap)) * bf.z;
                    float4 ax = SampleAlbedoAlpha(tx, TEXTURE2D_ARGS(_AltitudeMap, sampler_AltitudeMap)) * bf.x;
                    float4 ay = SampleAlbedoAlpha(ty, TEXTURE2D_ARGS(_AltitudeMap, sampler_AltitudeMap)) * bf.y;
                    float4 az = SampleAlbedoAlpha(tz, TEXTURE2D_ARGS(_AltitudeMap, sampler_AltitudeMap)) * bf.z;

                    float3 flatAlbedo = (cx + cy + cz).rgb;
                    float3 slopeAlbedo = (sx + sy + sz).rgb;
                    float3 altitudeAlbedo = (ax + ay + az).rgb;

                    float3 unitSphereToPosition = positionOS - pointOnUnitSphere;
                    float unitSphereToPositionDot = dot(pointOnUnitSphere, unitSphereToPosition);
                    float signedDistnace = sign(unitSphereToPositionDot) * sqrt(unitSphereToPositionDot);
                    float flatness = smoothstep(0.65, 1, dot(pointOnUnitSphere, normalOS));
                    float altitude = smoothstep(0.1875, 0.19, signedDistnace);
                    surfaceData.albedo = flatness * (altitude * altitudeAlbedo + (1 - altitude) * flatAlbedo) + (1 - flatness) * slopeAlbedo;

                    #ifdef _NORMALMAP
                        float3 cnx = SampleNormal(tx, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale) * bf.x;
                        float3 cny = SampleNormal(ty, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale) * bf.y;
                        float3 cnz = SampleNormal(tz, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale) * bf.z;
                        float3 snx = SampleNormal(tx, TEXTURE2D_ARGS(_SlopeBumpMap, sampler_SlopeBumpMap), _BumpScale) * bf.x;
                        float3 sny = SampleNormal(ty, TEXTURE2D_ARGS(_SlopeBumpMap, sampler_SlopeBumpMap), _BumpScale) * bf.y;
                        float3 snz = SampleNormal(tz, TEXTURE2D_ARGS(_SlopeBumpMap, sampler_SlopeBumpMap), _BumpScale) * bf.z;
                        float3 anx = SampleNormal(tx, TEXTURE2D_ARGS(_AltitudeBumpMap, sampler_AltitudeBumpMap), _BumpScale) * bf.x;
                        float3 any = SampleNormal(ty, TEXTURE2D_ARGS(_AltitudeBumpMap, sampler_AltitudeBumpMap), _BumpScale) * bf.y;
                        float3 anz = SampleNormal(tz, TEXTURE2D_ARGS(_AltitudeBumpMap, sampler_AltitudeBumpMap), _BumpScale) * bf.z;

                        float3 flatNormal = cnx + cny + cnz;
                        float3 slopeNormal = snx + sny + snz;
                        float3 altitudeNormal = anx + any + anz;
                        surfaceData.normalTS = flatness * (altitude * altitudeNormal + (1 - altitude) * flatNormal) + (1 - flatness) * slopeNormal;
                    #endif
                #endif

                InputData inputData;
                InitializeInputData(input, surfaceData.normalTS, inputData);

                half4 color = LightweightFragmentPBR(inputData, surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.occlusion, surfaceData.emission, surfaceData.alpha);

                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return color;
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
