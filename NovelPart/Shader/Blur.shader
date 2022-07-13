Shader "Effect/Blur"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Strength("Strength", Range(0.05, 0.15)) = 0.05
	}

		SubShader
		{

				Tags
				{
					"Queue" = "Transparent"
					"IgnoreProjector" = "True"
					"RenderType" = "Transparent"
					"PreviewType" = "Plane"
					"CanUseSpriteAtlas" = "True"
				}
				Cull Off
				ZTest Always
				ZWrite Off

				Lighting Off
				Fog{ Mode Off }
				Blend SrcAlpha OneMinusSrcAlpha


			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				//#pragma multi_compile DUMMY PIXELSNAP_ON
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _MainTex_TexelSize;


				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					half2 texcoord  : TEXCOORD0;
				};

				float rand(float2 co)
				{
					return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
				}

				// メインテクスチャからサンプリングしてRGBのみ返す
				half3 sampleMain(float2 uv) {
					return tex2D(_MainTex, uv).rgb;
				}

				// 対角線上の4点からサンプリングした色の平均値を返す
				half3 sampleBox(float2 uv, float delta) {
					float4 offset = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
					//offset.x *= sin(rand(float2(0.1, 0.1)));
					half3 sum = sampleMain(uv + offset.xy)
						+ sampleMain(uv + offset.zy)
						+ sampleMain(uv + offset.xw)
						+ sampleMain(uv + offset.zw);
					return sum * 0.25;
				}

				half3 sampling(half3 rgb,float2 uv,float delta) {
					float4 offset = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
					half3 sum = sampleMain(uv + offset.xy) + sampleMain(uv + offset.zy) + sampleMain(uv + offset.xw) + sampleMain(uv + offset.zw);
					return sum * 0.25;
				}


				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					//OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color;

					return OUT;
				}

				static float _r = 0.8;
					// ダウンサンプリング時には1ピクセル分ずらした対角線上の4点からサンプリング
				fixed4 frag(v2f i) : SV_Target
				{
					half4 col = tex2D(_MainTex, i.texcoord);
					float s = (int)(_Time.y * 20+180)/60;
					float random = abs(sin(rand(float2(s, s))))+0.5;

					col.rgb = sampleBox(i.texcoord, random*6 * abs(sin(_Time * 20))+2);
					//col.r = random;

					return col;
				}

				ENDCG
			}


			}
}