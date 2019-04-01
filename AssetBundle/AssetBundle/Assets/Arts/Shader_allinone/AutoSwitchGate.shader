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
		Tags { "QUEUE"="Geometry" "RenderType"="Opaque" }
		Pass {
			Tags { "LIGHTMODE"="Always" "QUEUE"="Geometry" "RenderType"="Opaque" }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			
			GLSLPROGRAM
				#include "UnityCG.glslinc"
				#include "GLSLSupport.glslinc"
				#ifdef VERTEX
				
				attribute vec4 _glesVertex;
				attribute vec4 _glesMultiTexCoord0;
				
				uniform highp vec4 _Scale;
				uniform highp float _ScrollX;
				uniform highp float _ScrollY;
				uniform mediump vec4 _DisplacementStrength;
				uniform mediump vec4 _DisplacementOffset;
				uniform mediump float _FogStart;
				uniform mediump float _FogEnd;
				uniform mediump float _FogDepthMul;
				varying mediump vec2 xlv_TEXCOORD0;
				varying mediump vec2 xlv_TEXCOORD1;
				varying mediump float xlv_TEXCOORD2;
				void main ()
				{
				  mediump vec2 screenPos_1;
				  mediump vec2 tmpvar_2;
				  highp vec4 tmpvar_3;
				  tmpvar_3 = (gl_ModelViewProjectionMatrix * _glesVertex);
				  highp vec3 tmpvar_4;
				  highp vec4 tmpvar_5;
				  tmpvar_5.w = 1.0;
				  tmpvar_5.xyz = _WorldSpaceCameraPos;
				  tmpvar_4 = ((unity_WorldToObject * tmpvar_5).xyz - _glesVertex.xyz);
				  highp vec2 tmpvar_6;
				  tmpvar_6.x = _ScrollX;
				  tmpvar_6.y = _ScrollY;
				  highp vec2 tmpvar_7;
				  tmpvar_7 = fract((tmpvar_6 * _Time.xy));
				  tmpvar_2 = ((_glesMultiTexCoord0.xy * _Scale.xy) + tmpvar_7);
				  mediump float tmpvar_8;
				  highp float tmpvar_9;
				  tmpvar_9 = clamp (((
					(sqrt(dot (tmpvar_4, tmpvar_4)) - _FogStart)
				   / 
					(_FogEnd - _FogStart)
				  ) * _FogDepthMul), 0.0, 1.0);
				  tmpvar_8 = tmpvar_9;
				  highp vec2 tmpvar_10;
				  tmpvar_10 = (tmpvar_3.xy / tmpvar_3.w);
				  screenPos_1 = tmpvar_10;
				  gl_Position = tmpvar_3;
				  xlv_TEXCOORD0 = tmpvar_2;
				  xlv_TEXCOORD1 = (((
					((screenPos_1 * 0.5) + vec2(0.5, 0.5))
				   * _DisplacementStrength.zw) - (vec2(0.5, 0.5) * _DisplacementStrength.xy)) + _DisplacementOffset.xy);
				  xlv_TEXCOORD2 = tmpvar_8;
				}
				
				#endif
				
				#ifdef FRAGMENT
				
				uniform sampler2D _MainTex;
				uniform sampler2D _ScreenTex;
				uniform lowp vec4 _FogColor;
				uniform mediump float _Exposure;
				varying mediump vec2 xlv_TEXCOORD0;
				varying mediump vec2 xlv_TEXCOORD1;
				varying mediump float xlv_TEXCOORD2;
				void main ()
				{
				  mediump vec2 ofs_1;
				  lowp vec4 c_2;
				  lowp vec2 tmpvar_3;
				  tmpvar_3 = ((texture2D (_MainTex, xlv_TEXCOORD0).xy * 0.025) - 0.0125);
				  ofs_1 = tmpvar_3;
				  lowp vec4 tmpvar_4;
				  mediump vec2 P_5;
				  P_5 = (xlv_TEXCOORD1 + ofs_1);
				  tmpvar_4 = texture2D (_ScreenTex, P_5);
				  mediump vec3 tmpvar_6;
				  tmpvar_6 = mix ((tmpvar_4 * _Exposure), _FogColor, vec4(xlv_TEXCOORD2)).xyz;
				  c_2.xyz = tmpvar_6;
				  c_2.w = 1.0;
				  gl_FragData[0] = c_2;
				}
				
				#endif
			ENDGLSL		
		}
	}
}
