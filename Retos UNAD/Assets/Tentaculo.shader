Shader "Custom/SciFiTentacleGlow"
{
    Properties
    {
        _Color ("Tentacle Color", Color) = (0, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Opaque" }
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Lighting Off
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _GlowIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 finalColor = tex * _Color;
                finalColor.rgb += finalColor.rgb * _GlowIntensity; // Add glow
                return finalColor;
            }
            ENDCG
        }
    }
}