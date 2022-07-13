Shader "Effect/Sepia"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		//_Darkness("Dark", Range(0, 0.1)) = 0.04
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

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color;

					return OUT;
				}


				sampler2D _MainTex;
				const half _Darkness =0.02;
				half _Strength;
				fixed4 _Color;

				fixed4 frag(v2f IN) : COLOR
				{
					half4 c = tex2D(_MainTex, IN.texcoord);
					half gray = c.r * 0.3 + c.g * 0.6 + c.b * 0.1 - _Darkness;
					gray = (gray < 0) ? 0 : gray;

					half R = gray + _Strength;
					half B = gray - _Strength;

					R = (R > 1.0) ? 1.0 : R;
					B = (B < 0) ? 0 : B;
					c.rgb = fixed3(R, gray, B);
					c.a *= _Color.a;
					return c;
				}
			ENDCG
			}
		}
}