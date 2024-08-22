Shader "Custom/GaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSizeX ("Blur Size X", Float) = 1.0
        _BlurSizeY ("Blur Size Y", Float) = 1.0
    }
    SubShader
    {
       Tags { "Queue"="Transparent" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
            float _BlurSizeX;
            float _BlurSizeY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = fixed4(0,0,0,0);
                float2 blurSize = float2(_BlurSizeX, _BlurSizeY) / _ScreenParams.xy;

                // Gaussian weights
                // These values should be precomputed to add up to 1
                float weights[3] = { 0.2270270270, 0.1945945946, 0.1216216216 };

                // Calculate the weighted sum of the surrounding pixels
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 offset = float2(x, y) * blurSize;
                        float weight = weights[1 + x] * weights[1 + y];
                        col += tex2D(_MainTex, i.uv + offset) * weight;
                    }
                }

                // The weight sum for a 3x3 kernel should be 1 if the weights are normalized
                float weightSum = (weights[0] + weights[1] + weights[2]) * (weights[0] + weights[1] + weights[2]);

                return col / weightSum;
            }


            ENDCG
        }
    }
}
