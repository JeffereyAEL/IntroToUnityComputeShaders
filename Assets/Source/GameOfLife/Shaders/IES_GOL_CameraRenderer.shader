Shader "GameOfLife/PostProcesing/IES_GOL_CameraRenderer"
{
    Properties
    {
        _DeadColor ("Dead Color", Color) = (0,0,0,1)
        _AliveColor ("Alive Color", Color) = (1,0,0,1)
    }
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
            float3x3 _Trans;
            float3x3 _Scale;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv[0] = Lerp(v.uv[0], _UVCoordinates[0], _UVCoordinates[1]);
                o.uv[1] = Lerp(v.uv[1], _UVCoordinates[2], _UVCoordinates[3]);
                o.uv = v.uv;
                return o;
            }

            sampler2D _StateMap;
            float4 _DeadColor;
            float4 _AliveColor;
            
            float4 frag (v2f i) : SV_Target
            {
                float state = tex2D(_StateMap, i.uv).r;
                return float4(state * _AliveColor + (1.0f - state) * _DeadColor);
            }
            ENDCG
        }
    }
}
