Shader "Scripts/UI/UnitSelectionCircle" 
{
	Properties 
	{
		_Color ("Tint Color", Color) = (1, 1, 1, 1)
		_ShadowTex ("Cookie", 2D) = "gray" {}
	}

	Subshader
	{
		Pass
		{
			ZWrite Off
			ColorMask RGB
			Blend SrcAlpha One // Additive Blending
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragement frag
			#include "UnityCG.cginc"

			fixed4 frag (v2f i) : SV_Target
			{
				// Sample cookie texture
				fixed4 texCookie = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));

				// Apply tint alpha mask
				fixed4 outColor = _Color * texCookie.a;

				// Distance attenuation
				float depth = i.uvShadow.z;

				return outColor * clamp(1.0 - abs(depth) + _Attenuation, 0.0, 1.0);
			}

			ENDCG
		}
	}
}
