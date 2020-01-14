Shader "SQT/Atmosphere" {
    Properties {
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        [MainColor] _BaseColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}

        _AtmosphereRadius ("Atmosphere radius", Float) = 1
        _PlanetRadius ("Planet radius", Float) = 0.5
        _SunIntensity ("Sun intensity", Float) = 1
        // _ScatteringCoefficient ("Scattering coefficient", Float) = 1
        _ViewSamples ("View samples", Int) = 8
        _LightSamples ("Light samples", Int) = 8
        _ScaleHeight ("Scale height", float) = 8500
        _ScaleFactor ("Scale factor", Float) = 6371000
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
            // float _ScatteringCoefficient;
            int _ViewSamples;
            int _LightSamples;
            float _ScaleHeight;
            float _ScaleFactor;

            float RayleighPhaseFunction(float cosTheta) {
                const float rpv = 0.0596831;
                return rpv * (1.0 + cosTheta * cosTheta);
            }

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

            void transmittance(float3 Pa, float3 Pb, float3 center, out float3 tr, out float3 tm) {
                float3 d = Pb - Pa;
                float dLen = length(d);
                float ds = dLen / float(_LightSamples);
                d /= dLen;
                float opticalDepthR = 0;
                float opticalDepthM = 0;
                float ms = 0;
                for(int i = 1; i < _LightSamples; i++) {
                    float3 p = Pa + d * (ms + 0.5 * ds);
                    float h = length(p - center) - _PlanetRadius;
    	            opticalDepthR += exp(-h / (_ScaleHeight / _ScaleFactor)) * ds;
                    // opticalDepthM += exp(-h / Hm) * ds;
                    ms += ds;
                }
                const float3 Bs0Rayleigh = float3(58e-7, 135e-7, 250e-7);
                tr = Bs0Rayleigh * opticalDepthR * _ScaleFactor;
                // tm = Bs0Mie * opticalDepthM;
                tm = 0;
            }

            // bool lightSampling(float3 p, float3 sunDirection, float3 sphereCenter, out float opticalDepthCA) {
            //     float _;
            //     float ts;
            //     raySphereIntersect(p, sunDirection, sphereCenter, _PlanetRadius, _, ts);

            //     // Samples on the segment PC.
	        //     float time = 0;
	        //     float ds = distance(p, p + sunDirection * sphereCenter) / (float)(_LightSamples);
	        //     for (int i = 0; i < _LightSamples; i++) {
		    //         float3 Q = p + sunDirection * (time + ds * 0.5);
		    //         float height = distance(sphereCenter, Q) - _PlanetRadius;
		    //         // Inside the planet
		    //         if (height < 0) {
			//             return false;
            //         }

		    //         // Optical depth for the light ray
		    //         opticalDepthCA += exp(-height / (_ScaleHeight / _ScaleFactor)) * ds;

		    //         time += ds;
	        //     }

	        //     return true;
            // }

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
                // output.rayDirectionOS = TransformWorldToObject(-(_WorldSpaceCameraPos.xyz - vertexInput.positionWS));
                // output.rayOriginOS = input.positionOS.xyz - output.rayDirection;
                output.rayDirectionOS = -(TransformWorldToObject(_WorldSpaceCameraPos.xyz) - input.positionOS.xyz);
                output.rayOriginOS = input.positionOS.xyz - output.rayDirectionOS;
                output.rayDirectionOS = normalize(output.rayDirectionOS);

                return output;
            }

            float4 Fragment(Varyings input) : SV_TARGET {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                input.rayDirectionOS = normalize(input.rayDirectionOS);

                float tA; // Atmosphere entry point (worldPos + V * tA)
                float tB; // Atmosphere exit point (worldPos + V * tB)
                if (!raySphereIntersect(input.rayOriginOS, input.rayDirectionOS, input.centerOS, _AtmosphereRadius, tA, tB)) {
                    return float4(0, 0, 0, 0); // The view rays is looking into deep space.
                }
 
                // Is the ray passing through the planet core?
                float pA, pB;
                if (raySphereIntersect(input.rayOriginOS, input.rayDirectionOS, input.centerOS, _PlanetRadius, pA, pB)) {
                    tB = pA;
                }

                float3 sunDirection = normalize(_MainLightPosition.xyz); // TODO: transform to object space

                float opticalDepthPA = 0;
                float3 integralR = float3(0, 0, 0);
                float3 integralM = float3(0, 0, 0);
                float time = tA;
                float ds = (tB - tA) / _ViewSamples;
                for (int i = 1; i < _ViewSamples; i++) {
                    // Point position
                    // (sampling in the middle of the view sample segment)
                    float3 p = input.rayOriginOS + input.rayDirectionOS * (time + ds * 0.5);

                    float _;
                    float ts;
                    raySphereIntersect(p, sunDirection, input.centerOS, _PlanetRadius, _, ts);
                    float3 Ps = p + sunDirection * ts;
                    float height = distance(input.centerOS, p) - _PlanetRadius;

                    float3 T1r, T1m, T2r, T2m;
                    transmittance(input.rayOriginOS, p, input.centerOS, T1r, T1m);
                    transmittance(p, Ps, input.centerOS, T2r, T2m);

                    integralR += exp(-(T1r + T2r) - height / (_ScaleHeight / _ScaleFactor)) * ds;

                    // // T(CP) * T(PA) * ρ(h) * ds
                    // // integralR += viewSampling(p, ds);

                    // // Optical depth of current segment.
                    // // ρ(h) * ds
                    // float height = distance(input.centerOS, p) - _PlanetRadius;
                    // float opticalDepthSegment = exp(-height / (_ScaleHeight / _ScaleFactor)) * ds;

                    // // Accumulates the optical depths.
                    // // D(PA)
                    // opticalDepthPA += opticalDepthSegment;
 
                    // // D(CP)
	                // float opticalDepthCP;
	                // bool overground = lightSampling(p, lightDirection, input.centerOS, opticalDepthCP);

	                // if (overground) {
		            //     // Combined transmittance
		            //     // T(CP) * T(PA) = T(CPA) = exp{-β(λ) [D(CP) + D(PA)]}
		            //     float transmittance = exp(-_ScatteringCoefficient * (opticalDepthCP + opticalDepthPA));

		            //     // Light contribution
		            //     // T(CPA) * ρ(h) * ds
		            //     integralR += transmittance * opticalDepthSegment;
                    //     return float4(1, 0, 0, 1);
	                // }
 
                    time += ds;
                }

                const float3 Bs0Rayleigh = float3(58e-7, 135e-7, 250e-7);

                float mu = dot(input.rayDirectionOS, sunDirection);
                integralR *= RayleighPhaseFunction(mu) * Bs0Rayleigh * _ScaleFactor;
                // integralM *= MiePhaseFunction(mu) * Bs0Mie;
                return float4(_SunIntensity * (integralR + integralM), 1);
 
                // I = I_S * β(λ) * γ(θ) * integralR
                // float3 I = _SunIntensity * (_ScatteringCoefficient * _ScaleFactor) * phase * integralR;
                // float3 I = _SunIntensity * (_ScatteringCoefficient * _ScaleFactor) * 1 * integralR;

                // return float4(I, 1);
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
