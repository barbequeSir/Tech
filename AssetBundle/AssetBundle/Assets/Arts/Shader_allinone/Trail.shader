Shader "Custom/Trail"
{
	Properties
	{
		_MainTex ("Base layer (RGB)", 2D) = "white" { }
		 _MaskTex ("Mask texture", 2D) = "white" { }
		 _Scale ("Scale", Vector) = (1,1,0,0)
		 _ScrollX ("Base layer Scroll speed X", Float) = 1
		 _ScrollY ("Base layer Scroll speed Y", Float) = 0
		  _ScreenTex ("Rendered screen texture", 2D) = "white" { }
		 _DisplacementStrength ("Displacement strength", Vector) = (1,1,1,1)
		 _DisplacementOffset ("Displacement offset", Vector) = (0,1,0,0)
		 _Exposure ("Exposure", Float) = 1
	}
	SubShader
	{
		Tags { "QUEUE"="Overlay" }
		Pass 
		{
			Tags { "LIGHTMODE"="Always" "QUEUE"="Overlay" }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			
			GLSLPROGRAM
				#include "UnityCG.glslinc"
				#include "GLSLSupport.glslinc"
				#ifdef VERTEX
				
				attribute vec4 _glesVertex;
				attribute vec4 _glesColor;
				attribute vec4 _glesMultiTexCoord0;
				
				
				uniform highp vec4 _Scale;
				uniform highp float _ScrollX;
				uniform highp float _ScrollY;
				uniform mediump vec4 _DisplacementStrength;
				uniform mediump vec4 _DisplacementOffset;
				varying mediump vec2 xlv_TEXCOORD0;
				varying mediump vec2 xlv_TEXCOORD1;
				varying mediump vec2 xlv_TEXCOORD2;
				varying lowp float xlv_TEXCOORD3;
				void main ()
				{
				  mediump vec2 screenPos_1;
				  mediump vec2 tmpvar_2;
				  highp vec4 tmpvar_3;
				  tmpvar_3 = (gl_ModelViewProjectionMatrix * _glesVertex);
				  highp vec2 tmpvar_4;
				  tmpvar_4.x = _ScrollX;
				  tmpvar_4.y = _ScrollY;
				  highp vec2 tmpvar_5;
				  tmpvar_5 = fract((tmpvar_4 * _Time.xy));
				  tmpvar_2 = ((_glesMultiTexCoord0.xy * _Scale.xy) + tmpvar_5);
				  highp vec2 tmpvar_6;
				  tmpvar_6 = (tmpvar_3.xy / tmpvar_3.w);
				  screenPos_1 = tmpvar_6;
				  gl_Position = tmpvar_3;
				  xlv_TEXCOORD0 = tmpvar_2;
				  xlv_TEXCOORD1 = _glesMultiTexCoord0.xy;
				  xlv_TEXCOORD2 = (((
					((screenPos_1 * 0.5) + vec2(0.5, 0.5))
				   * _DisplacementStrength.zw) - (vec2(0.5, 0.5) * _DisplacementStrength.xy)) + _DisplacementOffset.xy);
				  xlv_TEXCOORD3 = _glesColor.w;
				}
				
				#endif
				
				#ifdef FRAGMENT
				
				uniform sampler2D _MainTex;
				uniform sampler2D _MaskTex;
				uniform sampler2D _ScreenTex;
				uniform mediump float _Exposure;
				varying mediump vec2 xlv_TEXCOORD0;
				varying mediump vec2 xlv_TEXCOORD1;
				varying mediump vec2 xlv_TEXCOORD2;
				varying lowp float xlv_TEXCOORD3;
				void main ()
				{
				  mediump vec2 ofs_1;
				  lowp vec4 c_2;
				  lowp vec2 tmpvar_3;
				  tmpvar_3 = ((texture2D (_MainTex, xlv_TEXCOORD0).xy * 0.2) - 0.1);
				  ofs_1 = tmpvar_3;
				  lowp vec4 tmpvar_4;
				  mediump vec2 P_5;
				  P_5 = (xlv_TEXCOORD2 + ofs_1);
				  tmpvar_4 = texture2D (_ScreenTex, P_5);
				  mediump vec3 tmpvar_6;
				  tmpvar_6 = (tmpvar_4 * _Exposure).xyz;
				  c_2.xyz = tmpvar_6;
				  c_2.w = (xlv_TEXCOORD3 * texture2D (_MaskTex, xlv_TEXCOORD1).w);
				  gl_FragData[0] = c_2;
				}
				
				#endif
			ENDGLSL		
		}
	}
}
