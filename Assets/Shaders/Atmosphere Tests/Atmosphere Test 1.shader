Shader "Atmosphere Tests/Atmosphere Test 1" {
    Properties {
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        [MainColor] _BaseColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}

        // _AtmosphereRadius ("Atmosphere radius", Float) = 1
        _PlanetRadius ("Planet radius", Float) = 0.5
        [HideInInspector] _PlanetPosition ("Planet position", Vector) = (0, 0, 0)
        _SunIntensity ("Sun intensity", Float) = 50
        _ViewSamples ("View samples", Int) = 16
        _LightSamples ("Light samples", Int) = 8
        _ScaleFactor ("Scale factor", Float) = 6371000
        _ScaleHeightR ("Scale height Rayleigh", float) = 8500
        _ScaleHeightM ("Scale height Mie", float) = 1275
    }

    SubShader {
        Tags {
            "Queue" = "Overlay"
        }

        Pass {
            Blend One One
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "./Atmosphere Test 1.hlsl"

            ENDHLSL
        }
    }
}
