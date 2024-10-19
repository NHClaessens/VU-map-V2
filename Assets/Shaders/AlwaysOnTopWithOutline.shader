// Save this as AlwaysOnTopWithOutline.shader in your Assets folder
Shader "Custom/AlwaysOnTopWithOutline"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _Outline ("Outline width", Range (.001, 1)) = .5
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Color [_Color]
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
        
            CGPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
        
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };
        
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };
        
            fixed _Outline;
            fixed4 _OutlineColor;
            // float4 _ScreenParams;

            v2f vertOutline (appdata v)
            {
                // expand vertex positions along normals
                v2f o;
                float3 norm = mul((float3x3) UNITY_MATRIX_IT_MV, v.vertex.xyz);

                // Correct for aspect ratio
                // norm.x *= _ScreenParams.y / _ScreenParams.x / 5;
                // norm.y *= _ScreenParams.x / _ScreenParams.y / 5;
                
                o.pos = UnityObjectToClipPos(v.vertex + float4(norm * _Outline, 0));
                o.color = _OutlineColor;
                return o;
            }

            fixed4 fragOutline (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
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

