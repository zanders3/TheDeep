Shader "TheDeep/Tile" {
	Properties {
		_MainTex ("Tiles (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Transparent" "RenderQueue"="Transparent" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		sampler2D _MainTex;

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv1 : TEXCOORD0;
		};
		
		struct v2f
		{
			float4 pos : POSITION;
			float2 uv1 : TEXCOORD0;
		};
		
		v2f vert(appdata v)
		{
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv1 = v.uv1;
			return o;
		}
		
		fixed4 frag(v2f o) : COLOR
		{
			return tex2D(_MainTex, o.uv1);
		}
		
		ENDCG
		}
	} 
	FallBack "Diffuse"
}
