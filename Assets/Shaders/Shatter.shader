Shader "J/Shatter"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Overlay"
		}

		Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

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
		
			half _ScreenRatio;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;

				// write vertex data to local var
				o.vertex = v.vertex;

				// adjust x to compensate for screen ratio
				o.vertex.x = o.vertex.x * _ScreenRatio;

				// transform vertex
				o.vertex = mul(UNITY_MATRIX_MV, o.vertex);

				// revert x value to NDCs
				o.vertex.x = o.vertex.x / _ScreenRatio;
				
				// I don't know why this works but this prevents clipping when rotating 
				o.vertex.z = 0;

				// Pass in uv coords
				o.uv = v.uv;

				return o;
			}
			
			sampler2D _MainTex;
			
			half _Alpha;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
			
				col.a = _Alpha;

				return col;
			}
			ENDCG
		}
	}
}
