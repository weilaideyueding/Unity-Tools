Shader "NPR/Particle"
{
    Properties
    {
		_MainTex("MainTex", 2D) = "white" {}
		_AlphaClip("Alpha Clip", Range(0.0,1.0)) = 0.1
		_BaseColor("BaseColor", Color) = (1,1,1,1)
		_Scale("Scale", Range(0.01, 10)) = 0.1
    }
    SubShader
    {
		Tags{
			"Queue" = "Transparent"
		}

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			Tags
			{
				"LightMode" = "UniversalForward"
			}

			HLSLPROGRAM

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			struct Particle
			{
			    float3 position; // 粒子位置
			    float3 velocity; // 粒子速度
			    float life;       // 粒子生命周期
			};

			#pragma vertex vert
			#pragma fragment frag
			
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			half4 _BaseColor;
			half _Scale;
			half _AlphaClip;


			struct Attributes {
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float2 uv : TEXCOORD0;
				
			};

			struct Varings {
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			

			//由Material.SetBuffer所绑定的粒子buffer
			StructuredBuffer<Particle> _ParticleBuffer;

			Varings vert(Attributes input, uint instanceID : SV_InstanceID) {
				Varings o;

				// 利用实例化序号获取到位置
				Particle p = _ParticleBuffer[instanceID];
				float3 position = input.positionOS.xyz * _Scale;

				// 位置偏移
				position += p.position;

				// 空间转换
				o.positionCS = TransformObjectToHClip(position);
				o.uv = input.uv;
				return o;
			}

			half4 frag(Varings i) : SV_TARGET{
				
				half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				clip(color.r - _AlphaClip);
				return half4(color.rgb * _BaseColor.rgb, 1);
			}

			ENDHLSL
		}
	}
}
