Shader "Unlit/FillProjectedCircle"
{
    Properties
    {
        _BaseColor ("BaseColor", Color) = (0.1,0.1,0.1,0.25)
        _FillColor ("FillColor", Color) = (1,0,0,0.25)
        _FillAmount ("Fill Amount", Range(0,1)) = 1 // 控制填充程度
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            fixed4 _BaseColor;
            fixed4 _FillColor;
            float _FillAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5); // 圆心位置（UV 坐标）
                float _distance = distance(i.uv, center);
                float magnitude = length((0.8,0.5));
                int isFilled = step(_distance / magnitude, _FillAmount); 
                fixed4 col = isFilled * _FillColor + (1 - isFilled) * _BaseColor;
                return col;
            }
            ENDCG
        }
    }
}
