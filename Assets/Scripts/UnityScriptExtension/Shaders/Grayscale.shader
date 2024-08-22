Shader "Custom/Grayscale"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Coex("Coex",Range(0,1)) = 1
        _RCoex("RCoex",Range(0,1)) = 0.299
        _GCoex("GCoex",Range(0,1)) = 0.587
        _BCoex("BCoex",Range(0,1)) = 0.114
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        
        Stencil
    {
        Ref[_Stencil]
        Comp[_StencilComp]
        Pass[_StencilOp]
        ReadMask[_StencilReadMask]
        WriteMask[_StencilWriteMask]
    }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]

        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Coex;
            float _RCoex;
            float _GCoex;
            float _BCoex;

            // 在 appdata 结构体中添加颜色属性
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // 添加颜色属性
            };

            // 修改 v2f 结构体以包含颜色
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR; // 添加颜色属性
            };

            // 修改 vert 函数以传递颜色
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color; // 传递颜色
                return o;
            }

            // 修改 frag 函数以应用颜色的 Alpha
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float gray = dot(col.rgb, float3(_RCoex, _GCoex, _BCoex));
                col = lerp(col, float4(gray, gray, gray, col.a), _Coex);
                col *= i.color; // 应用传入的顶点颜色的 Alpha
                return col;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
