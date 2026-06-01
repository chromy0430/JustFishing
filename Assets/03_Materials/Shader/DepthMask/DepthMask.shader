Shader "DepthMask"
{
	SubShader
	{
		Tags {"Queue" = "Geometry+10"}

		ColorMask 0
		ZWrite On
		
		Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }
		

		Pass {}
	}
}