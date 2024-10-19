Shader "Custom/AlwaysOnTop"
{
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Color [_Color]
        Pass {
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            SetTexture[_] {
                constantColor [_Color]
                Combine constant
            }
        }
    }
    FallBack "Diffuse"
}
