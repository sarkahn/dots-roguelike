Shader "Sark/TerminalShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		// Alpha value from texture color determines FGColor/BGColor cutoff
		_BGCutoff ("Background Color Cutoff", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float4 fg : TEXCOORD1;
				float4 bg : TEXCOORD2;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float4 fg : FGCOLOR;
				float4 bg : BGCOLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _BGCutoff;

            v2f vert (appdata v, out float4 outpos : SV_POSITION)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.fg = v.fg;
				o.bg = v.bg;

				outpos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, i.uv);
				fixed4 col = i.fg * texCol;

				fixed3 fg = i.fg.rgb * texCol;
				fixed3 bg = i.bg.rgb;

                col.rgb = texCol.a < _BGCutoff ? bg : fg;
				//col.rgb = texCol.a < _BGCutoff ? 0 : 1;
				col.a = 1;

				return col;
            }
            ENDCG
        }
    }
}
