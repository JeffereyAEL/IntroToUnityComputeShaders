Shader "GameOfLife/PostProcesing/IES_GOL_CameraRenderer"
{
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Shaders/Utilities/UTIL_Generics.cginc"
            
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

            float4 _UVCoordinates;
            float _UVScale;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                v.uv = mul(v.uv, float2x2(_UVScale, 0.0f, 0.0f, _UVScale));
                o.uv[0] = Lerp(v.uv[0], _UVCoordinates[0], _UVCoordinates[1]);
                o.uv[1] = Lerp(v.uv[1], _UVCoordinates[2], _UVCoordinates[3]);
                return o;
            }

            sampler2D _StateMap;
            float4 _DeadColorA;
            float4 _DeadColorB;
            float4 _AliveColorA;
            float4 _AliveColorB;
            
            float4 frag (v2f i) : SV_Target
            {
                float state = tex2D(_StateMap, i.uv).x;
                float color_weight = tex2D(_StateMap, i.uv).y + 1.0f;
                float color_mod = 1.0f /
                    (
                        color_weight * color_weight *
                        color_weight * color_weight
                        );
                
                return float4(state * Blend(_AliveColorA, _AliveColorB, color_mod) +
                    (1.0f - state) * Blend(_DeadColorA, _DeadColorB, color_mod));
            }
            ENDCG
        }
    }
}
