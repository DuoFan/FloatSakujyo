Shader "Unlit/FillCircle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _FillAmount ("Fill Amount", Range(0,1)) = 1 // 控制填充程度
    }
    SubShader
    {
       Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _FillAmount; 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Convert UV to polar coordinates
                float2 centeredUV = i.uv - 0.5;
                float angle = atan2(centeredUV.y, centeredUV.x);
                if(angle < 0) angle += 2 * UNITY_PI; // Normalize angle between 0 and 2*PI
                float radius = length(centeredUV) * 2; // Normalize radius between 0 and 1

                // Calculate fill based on angle
                float fill = step(angle / (2 * UNITY_PI),_FillAmount);

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                col.a *= fill; // Use the fill value to adjust the alpha

                return col;
            }
            ENDCG
        }
    }
}
