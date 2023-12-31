Shader "Unlit/URP_Base"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
 
        Pass
        {
            Tags{"LightMode" = "UniversalForward"  "RenderPipeline" = "UniversalRenderPipeline"}
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
 
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
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }
 
            half4 frag (v2f i) : SV_Target
            {
                return 1;
            }
            ENDHLSL
        }
    }
}
