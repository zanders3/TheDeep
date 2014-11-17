Shader "Custom/Shader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
			Material {
                Diffuse (1,1,1,1)
                Ambient (1,1,1,1)
            }
            Lighting On
			AlphaTest Greater 0.5
			SetTexture [_MainTex] { combine previous * texture }
		}
	} 
	FallBack "Diffuse"
}
