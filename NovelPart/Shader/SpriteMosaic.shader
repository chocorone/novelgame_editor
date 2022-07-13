Shader "Effect/mosaic" {
		Properties
		{
			[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
			_Color("Tint", Color) = (1,1,1,1)
			_Strength("Strength", Range(0.05, 0.15)) = 0.1
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
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcAlpha


			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile DUMMY PIXELSNAP_ON
				#include "UnityCG.cginc"

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
				float4 _MainTex_ST;
				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					//OUT.texcoord = IN.texcoord;
					OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
					OUT.color = IN.color;

					return OUT;
				}


				sampler2D _MainTex;
				float _Strength;
				fixed4 _Color;

				fixed4 frag(v2f IN) : COLOR
				{
					half2 uv = floor(IN.texcoord * _Strength*200) / (_Strength*200);
					half4 c = tex2D(_MainTex, uv);
					c.a  *= _Color.a;
					return c;
				}
				ENDCG
			}
		}
	}