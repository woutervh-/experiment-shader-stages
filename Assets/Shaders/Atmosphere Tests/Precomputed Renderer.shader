Shader "Atmosphere Tests/Precomputed Renderer" {
    Properties {
        _PlanetRadius ("Planet radius", Float) = 1.0
        _AtmosphereRadius ("Atmosphere radius", Float) = 1.5
        _PlanetPosition ("Planet position", Vector) = (0, 0, 0)
        _SunIntensity ("Sun intensity", Float) = 15.0
        _ScaleFactor ("Scale factor", Float) = 6371000
    }

    SubShader {
        Tags {
            "Queue" = "Overlay"
        }

        Pass {
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "./Precomputed Renderer.hlsl"

            ENDHLSL
        }
    }
}
