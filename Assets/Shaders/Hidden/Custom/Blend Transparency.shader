Shader "Hidden/Custom/Blend Transparency" {
    SubShader {
        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        // Blend Off
        // ColorMask RGBA

        Pass {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

                #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

                TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
                TEXTURE2D_SAMPLER2D(_SecondaryCameraTexture, sampler_SecondaryCameraTexture);
                TEXTURE2D_SAMPLER2D(_TertiaryCameraTexture, sampler_TertiaryCameraTexture);
                // TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

                float4 Frag(VaryingsDefault i) : SV_Target {
                    float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                    color.a = saturate(color.a);
                    return color;

                    // #ifdef UNITY_REVERSED_Z
                    //     float depth = 1.0 - LinearEyeDepth(SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord));
                    // #else
                    //     float depth = LinearEyeDepth(SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord));
                    // #endif
                    // return float4(depth, depth, depth, 1);

                    // float4 primaryColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                    // float4 secondaryColor = SAMPLE_TEXTURE2D(_SecondaryCameraTexture, sampler_SecondaryCameraTexture, i.texcoord);
                    // float4 tertiaryColor = SAMPLE_TEXTURE2D(_TertiaryCameraTexture, sampler_TertiaryCameraTexture, i.texcoord);
                    // float3 intermediateColor = secondaryColor.rgb * secondaryColor.a + tertiaryColor.rgb * (1 - secondaryColor.a);
                    // float3 finalColor = primaryColor.rgb * primaryColor.a + intermediateColor.rgb * (1 - primaryColor.a);
                    // return float4(finalColor, saturate(primaryColor.a + secondaryColor.a + tertiaryColor.a));
                }

            ENDHLSL
        }
    }
}