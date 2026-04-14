Shader "Custom/DisableZWrite"
{
	//basically, this just disables opaque and renders whats behind it
	SubShader{
		Tags{
			"RenderType"="Opaque"
		}

		
	Pass{
		ZWrite Off
	}

	}

}