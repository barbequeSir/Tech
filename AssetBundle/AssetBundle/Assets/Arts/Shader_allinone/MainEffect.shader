Shader "Custom/MainEffeect"{
	Properties
	{
		_MainTex("Main Texture" ,2D) = "white"{}
		_BlurTex("Blur Texture",2D) = "white"{}
		_TexelSize("Texel Size",Vector) = (0.04,0.04,0,0)
	}
	
	SubShader
	{
		Pass
		{
			Name "DEPTHVISUALISATION"
			ZTest False
			ZWrite Off
			GLSLPROGRAM
			#include "UnityCG.glslinc"
			#include "GLSLSupport.glslinc"
				#ifdef VERTEX
				attribute vec4 _glesVertex;
				attribute vec4 _glesMultiTexCoord0;
				uniform highp mat4 glstate_matrix_mvp;
				varying mediump vec2 xlv_TEXCOORD0;
				void main ()
				{
				  gl_Position = (glstate_matrix_mvp * _glesVertex);
				  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
				}


				#endif
				#ifdef FRAGMENT
				uniform sampler2D _MainTex;
				uniform sampler2D _BlurTex;
				varying mediump vec2 xlv_TEXCOORD0;
				void main ()
				{
				  lowp vec4 color_1;
				  lowp vec4 tmpvar_2;
				  tmpvar_2 = texture2D (_MainTex, xlv_TEXCOORD0);
				  color_1.xyz = mix (tmpvar_2.xyz, texture2D (_BlurTex, xlv_TEXCOORD0).xyz, tmpvar_2.www);
				  color_1.w = 1.0;
				  gl_FragData[0] = color_1;
				}


				#endif
			ENDGLSL
		}
		
		Pass
		{
			Name "COMPOSIT"
			ZTest False
			ZWrite Off
			Cull Off
			
			GLSLPROGRAM
			#include "UnityCG.glslinc"
			#include "GLSLSupport.glslinc"
				#ifdef VERTEX
				attribute vec4 _glesVertex;
				attribute vec4 _glesMultiTexCoord0;
				uniform highp mat4 glstate_matrix_mvp;
				varying mediump vec2 xlv_TEXCOORD0;
				void main ()
				{
				  gl_Position = (glstate_matrix_mvp * _glesVertex);
				  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
				}


				#endif
				#ifdef FRAGMENT
				uniform sampler2D _MainTex;
				varying mediump vec2 xlv_TEXCOORD0;
				void main ()
				{
				  lowp vec4 tmpvar_1;
				  tmpvar_1 = texture2D (_MainTex, xlv_TEXCOORD0);
				  gl_FragData[0] = tmpvar_1;
				}


				#endif
			ENDGLSL
		}
		
		Pass
		{
			  Name "SIMPLEBLUR"
			  ZTest False
			  ZWrite Off
			  Cull Off
			    GLSLPROGRAM
				#include "UnityCG.glslinc"
				#include "GLSLSupport.glslinc"
					#ifdef VERTEX
					attribute vec4 _glesVertex;
					attribute vec4 _glesMultiTexCoord0;
					uniform highp mat4 glstate_matrix_mvp;
					uniform mediump vec4 _TexelSize;
					varying mediump vec2 xlv_TEXCOORD0;
					varying mediump vec2 xlv_TEXCOORD1;
					varying mediump vec2 xlv_TEXCOORD1_1;
					varying mediump vec2 xlv_TEXCOORD1_2;
					void main ()
					{
					  gl_Position = (glstate_matrix_mvp * _glesVertex);
					  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
					  xlv_TEXCOORD1 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(1.0, 0.0)));
					  xlv_TEXCOORD1_1 = (_glesMultiTexCoord0.xy + _TexelSize.xy);
					  xlv_TEXCOORD1_2 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(0.0, 1.0)));
					}
					#endif
					#ifdef FRAGMENT
					uniform sampler2D _MainTex;
					uniform sampler2D _BlurTex;
					varying mediump vec2 xlv_TEXCOORD0;
					varying mediump vec2 xlv_TEXCOORD1;
					varying mediump vec2 xlv_TEXCOORD1_1;
					varying mediump vec2 xlv_TEXCOORD1_2;
					void main ()
					{
					  mediump float a_0_1;
					  mediump float a_1_2;
					  mediump float a_2_3;
					  mediump float a_3_4;
					  lowp vec4 color_5;
					  lowp vec4 tmpvar_6;
					  tmpvar_6 = texture2D (_MainTex, xlv_TEXCOORD0);
					  a_0_1 = tmpvar_6.w;
					  lowp vec4 tmpvar_7;
					  tmpvar_7 = texture2D (_MainTex, xlv_TEXCOORD1);
					  a_1_2 = tmpvar_7.w;
					  lowp vec4 tmpvar_8;
					  tmpvar_8 = texture2D (_MainTex, xlv_TEXCOORD1_1);
					  a_2_3 = tmpvar_8.w;
					  lowp vec4 tmpvar_9;
					  tmpvar_9 = texture2D (_MainTex, xlv_TEXCOORD1_2);
					  a_3_4 = tmpvar_9.w;
					  mediump float tmpvar_10;
					  tmpvar_10 = clamp ((5.0 * abs(
						(a_0_1 - a_2_3)
					  )), 0.0, 1.0);
					  mediump float tmpvar_11;
					  tmpvar_11 = clamp ((5.0 * abs(
						(a_1_2 - a_3_4)
					  )), 0.0, 1.0);
					  color_5.xyz = ((mix (tmpvar_6.xyz, texture2D (_BlurTex, xlv_TEXCOORD0).xyz, tmpvar_6.www) * (1.0 - tmpvar_10)) * (1.0 - tmpvar_11));
					  color_5.w = 1.0;
					  gl_FragData[0] = color_5;
					}


					#endif
				ENDGLSL
		}
		
		Pass
		{
			  Name "VBLUR"
			  ZTest False
			  ZWrite Off
			  Cull Off
			  GLSLPROGRAM
			#include "UnityCG.glslinc"
			#include "GLSLSupport.glslinc"
				  #ifdef VERTEX
					attribute vec4 _glesVertex;
					attribute vec4 _glesMultiTexCoord0;
					uniform highp mat4 glstate_matrix_mvp;
					uniform mediump vec4 _TexelSize;
					varying mediump vec2 xlv_TEXCOORD0;
					varying mediump vec2 xlv_TEXCOORD1;
					varying mediump vec2 xlv_TEXCOORD1_1;
					varying mediump vec2 xlv_TEXCOORD1_2;
					varying mediump vec2 xlv_TEXCOORD1_3;
					varying mediump vec2 xlv_TEXCOORD1_4;
					varying mediump vec2 xlv_TEXCOORD1_5;
					void main ()
					{
					  gl_Position = (glstate_matrix_mvp * _glesVertex);
					  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
					  xlv_TEXCOORD1 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(-3.0, 0.0)));
					  xlv_TEXCOORD1_1 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(-2.0, 0.0)));
					  xlv_TEXCOORD1_2 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(-1.0, 0.0)));
					  xlv_TEXCOORD1_3 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(1.0, 0.0)));
					  xlv_TEXCOORD1_4 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(2.0, 0.0)));
					  xlv_TEXCOORD1_5 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(3.0, 0.0)));
					}


					#endif
					#ifdef FRAGMENT
					uniform sampler2D _MainTex;
					varying mediump vec2 xlv_TEXCOORD0;
					varying mediump vec2 xlv_TEXCOORD1;
					varying mediump vec2 xlv_TEXCOORD1_1;
					varying mediump vec2 xlv_TEXCOORD1_2;
					varying mediump vec2 xlv_TEXCOORD1_3;
					varying mediump vec2 xlv_TEXCOORD1_4;
					varying mediump vec2 xlv_TEXCOORD1_5;
					void main ()
					{
					  lowp vec4 tmpvar_1;
					  mediump float totalA_2;
					  mediump vec4 c_3;
					  mediump vec4 color_4;
					  lowp vec4 tmpvar_5;
					  tmpvar_5 = texture2D (_MainTex, xlv_TEXCOORD1);
					  c_3 = tmpvar_5;
					  c_3.w = (c_3.w * 0.006);
					  totalA_2 = c_3.w;
					  color_4 = (c_3 * c_3.w);
					  lowp vec4 tmpvar_6;
					  tmpvar_6 = texture2D (_MainTex, xlv_TEXCOORD1_1);
					  c_3 = tmpvar_6;
					  c_3.w = (c_3.w * 0.061);
					  totalA_2 = (totalA_2 + c_3.w);
					  color_4 = (color_4 + (c_3 * c_3.w));
					  lowp vec4 tmpvar_7;
					  tmpvar_7 = texture2D (_MainTex, xlv_TEXCOORD1_2);
					  c_3 = tmpvar_7;
					  c_3.w = (c_3.w * 0.242);
					  totalA_2 = (totalA_2 + c_3.w);
					  color_4 = (color_4 + (c_3 * c_3.w));
					  lowp vec4 tmpvar_8;
					  tmpvar_8 = texture2D (_MainTex, xlv_TEXCOORD0);
					  c_3 = tmpvar_8;
					  c_3.w = (c_3.w * 0.383);
					  totalA_2 = (totalA_2 + c_3.w);
					  color_4 = (color_4 + (c_3 * c_3.w));
					  lowp vec4 tmpvar_9;
					  tmpvar_9 = texture2D (_MainTex, xlv_TEXCOORD1_3);
					  c_3 = tmpvar_9;
					  c_3.w = (c_3.w * 0.242);
					  totalA_2 = (totalA_2 + c_3.w);
					  color_4 = (color_4 + (c_3 * c_3.w));
					  lowp vec4 tmpvar_10;
					  tmpvar_10 = texture2D (_MainTex, xlv_TEXCOORD1_4);
					  c_3 = tmpvar_10;
					  c_3.w = (c_3.w * 0.061);
					  totalA_2 = (totalA_2 + c_3.w);
					  color_4 = (color_4 + (c_3 * c_3.w));
					  lowp vec4 tmpvar_11;
					  tmpvar_11 = texture2D (_MainTex, xlv_TEXCOORD1_5);
					  c_3 = tmpvar_11;
					  c_3.w = (c_3.w * 0.006);
					  totalA_2 = (totalA_2 + c_3.w);
					  color_4 = (color_4 + (c_3 * c_3.w));
					  color_4 = (color_4 / totalA_2);
					  tmpvar_1 = color_4;
					  gl_FragData[0] = tmpvar_1;
					}


					#endif
				ENDGLSL
		}
		Pass 
		{
			  Name "HBLUR"
			  ZTest False
			  ZWrite Off
			  Cull Off
			  GLSLPROGRAM
			#include "UnityCG.glslinc"
			#include "GLSLSupport.glslinc"
				  #ifdef VERTEX
					attribute vec4 _glesVertex;
					attribute vec4 _glesMultiTexCoord0;
					uniform highp mat4 glstate_matrix_mvp;
					varying mediump vec2 xlv_TEXCOORD0;
					void main ()
					{
					  gl_Position = (glstate_matrix_mvp * _glesVertex);
					  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
					}


					#endif
					#ifdef FRAGMENT
					uniform sampler2D _MainTex;
					varying mediump vec2 xlv_TEXCOORD0;
					void main ()
					{
					  lowp vec4 tmpvar_1;
					  tmpvar_1 = texture2D (_MainTex, xlv_TEXCOORD0);
					  lowp vec4 tmpvar_2;
					  tmpvar_2.x = tmpvar_1.w;
					  tmpvar_2.y = tmpvar_1.w;
					  tmpvar_2.z = tmpvar_1.w;
					  tmpvar_2.w = tmpvar_1.w;
					  gl_FragData[0] = tmpvar_2;
					}


					#endif
				ENDGLSL
	
		}
		
		Pass 
		{
		  Name "COPY"
		  ZTest False
		  ZWrite Off
		  Cull Off
		  GLSLPROGRAM
			#include "UnityCG.glslinc"
			#include "GLSLSupport.glslinc"
				#ifdef VERTEX
				attribute vec4 _glesVertex;
				attribute vec4 _glesMultiTexCoord0;
				uniform highp mat4 glstate_matrix_mvp;
				uniform mediump vec4 _TexelSize;
				varying mediump vec2 xlv_TEXCOORD0;
				varying mediump vec2 xlv_TEXCOORD1;
				varying mediump vec2 xlv_TEXCOORD1_1;
				varying mediump vec2 xlv_TEXCOORD1_2;
				void main ()
				{
				  gl_Position = (glstate_matrix_mvp * _glesVertex);
				  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
				  xlv_TEXCOORD1 = (_glesMultiTexCoord0.xy + _TexelSize.xy);
				  xlv_TEXCOORD1_1 = (_glesMultiTexCoord0.xy - _TexelSize.xy);
				  xlv_TEXCOORD1_2 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(1.0, -1.0)));
				}


				#endif
				#ifdef FRAGMENT
				uniform sampler2D _MainTex;
				varying mediump vec2 xlv_TEXCOORD0;
				varying mediump vec2 xlv_TEXCOORD1;
				varying mediump vec2 xlv_TEXCOORD1_1;
				varying mediump vec2 xlv_TEXCOORD1_2;
				void main ()
				{
				  lowp vec4 tmpvar_1;
				  highp float totalA_2;
				  highp vec4 c_3;
				  mediump vec4 color_4;
				  lowp vec4 tmpvar_5;
				  tmpvar_5 = texture2D (_MainTex, xlv_TEXCOORD1);
				  c_3 = tmpvar_5;
				  c_3.w = (c_3.w * 0.2);
				  totalA_2 = c_3.w;
				  color_4 = (c_3 * c_3.w);
				  lowp vec4 tmpvar_6;
				  tmpvar_6 = texture2D (_MainTex, xlv_TEXCOORD1_1);
				  c_3 = tmpvar_6;
				  c_3.w = (c_3.w * 0.2);
				  totalA_2 = (totalA_2 + c_3.w);
				  color_4 = (color_4 + (c_3 * c_3.w));
				  lowp vec4 tmpvar_7;
				  tmpvar_7 = texture2D (_MainTex, xlv_TEXCOORD1_2);
				  c_3 = tmpvar_7;
				  c_3.w = (c_3.w * 0.2);
				  totalA_2 = (totalA_2 + c_3.w);
				  color_4 = (color_4 + (c_3 * c_3.w));
				  lowp vec4 tmpvar_8;
				  tmpvar_8 = texture2D (_MainTex, xlv_TEXCOORD0);
				  c_3 = tmpvar_8;
				  c_3.w = (c_3.w * 0.4);
				  totalA_2 = (totalA_2 + c_3.w);
				  color_4 = (color_4 + (c_3 * c_3.w));
				  color_4 = (color_4 / totalA_2);
				  tmpvar_1 = color_4;
				  gl_FragData[0] = tmpvar_1;
				}


				#endif
			ENDGLSL
		}
		
		Pass 
		{
		  Name "BLUR WITH EDGES"
		  ZTest False
		  ZWrite Off
		  Cull Off
		  GLSLPROGRAM
			#include "UnityCG.glslinc"
			#include "GLSLSupport.glslinc"
			  #ifdef VERTEX
				attribute vec4 _glesVertex;
				attribute vec4 _glesMultiTexCoord0;
				uniform highp mat4 glstate_matrix_mvp;
				uniform mediump vec4 _TexelSize;
				varying mediump vec2 xlv_TEXCOORD0;
				varying mediump vec2 xlv_TEXCOORD1;
				varying mediump vec2 xlv_TEXCOORD1_1;
				varying mediump vec2 xlv_TEXCOORD1_2;
				varying mediump vec2 xlv_TEXCOORD1_3;
				varying mediump vec2 xlv_TEXCOORD1_4;
				varying mediump vec2 xlv_TEXCOORD1_5;
				void main ()
				{
				  gl_Position = (glstate_matrix_mvp * _glesVertex);
				  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
				  xlv_TEXCOORD1 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(0.0, -3.0)));
				  xlv_TEXCOORD1_1 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(0.0, -2.0)));
				  xlv_TEXCOORD1_2 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(0.0, -1.0)));
				  xlv_TEXCOORD1_3 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(0.0, 1.0)));
				  xlv_TEXCOORD1_4 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(0.0, 2.0)));
				  xlv_TEXCOORD1_5 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(0.0, 3.0)));
				}


				#endif
				#ifdef FRAGMENT
				uniform sampler2D _MainTex;
				varying mediump vec2 xlv_TEXCOORD0;
				varying mediump vec2 xlv_TEXCOORD1;
				varying mediump vec2 xlv_TEXCOORD1_1;
				varying mediump vec2 xlv_TEXCOORD1_2;
				varying mediump vec2 xlv_TEXCOORD1_3;
				varying mediump vec2 xlv_TEXCOORD1_4;
				varying mediump vec2 xlv_TEXCOORD1_5;
				void main ()
				{
				  lowp vec4 tmpvar_1;
				  mediump float totalA_2;
				  mediump vec4 c_3;
				  mediump vec4 color_4;
				  lowp vec4 tmpvar_5;
				  tmpvar_5 = texture2D (_MainTex, xlv_TEXCOORD1);
				  c_3 = tmpvar_5;
				  c_3.w = (c_3.w * 0.006);
				  totalA_2 = c_3.w;
				  color_4 = (c_3 * c_3.w);
				  lowp vec4 tmpvar_6;
				  tmpvar_6 = texture2D (_MainTex, xlv_TEXCOORD1_1);
				  c_3 = tmpvar_6;
				  c_3.w = (c_3.w * 0.061);
				  totalA_2 = (totalA_2 + c_3.w);
				  color_4 = (color_4 + (c_3 * c_3.w));
				  lowp vec4 tmpvar_7;
				  tmpvar_7 = texture2D (_MainTex, xlv_TEXCOORD1_2);
				  c_3 = tmpvar_7;
				  c_3.w = (c_3.w * 0.242);
				  totalA_2 = (totalA_2 + c_3.w);
				  color_4 = (color_4 + (c_3 * c_3.w));
				  lowp vec4 tmpvar_8;
				  tmpvar_8 = texture2D (_MainTex, xlv_TEXCOORD0);
				  c_3 = tmpvar_8;
				  c_3.w = (c_3.w * 0.383);
				  totalA_2 = (totalA_2 + c_3.w);
				  color_4 = (color_4 + (c_3 * c_3.w));
				  lowp vec4 tmpvar_9;
				  tmpvar_9 = texture2D (_MainTex, xlv_TEXCOORD1_3);
				  c_3 = tmpvar_9;
				  c_3.w = (c_3.w * 0.242);
				  totalA_2 = (totalA_2 + c_3.w);
				  color_4 = (color_4 + (c_3 * c_3.w));
				  lowp vec4 tmpvar_10;
				  tmpvar_10 = texture2D (_MainTex, xlv_TEXCOORD1_4);
				  c_3 = tmpvar_10;
				  c_3.w = (c_3.w * 0.061);
				  totalA_2 = (totalA_2 + c_3.w);
				  color_4 = (color_4 + (c_3 * c_3.w));
				  lowp vec4 tmpvar_11;
				  tmpvar_11 = texture2D (_MainTex, xlv_TEXCOORD1_5);
				  c_3 = tmpvar_11;
				  c_3.w = (c_3.w * 0.006);
				  totalA_2 = (totalA_2 + c_3.w);
				  color_4 = (color_4 + (c_3 * c_3.w));
				  color_4 = (color_4 / totalA_2);
				  tmpvar_1 = color_4;
				  gl_FragData[0] = tmpvar_1;
				}


				#endif
			ENDGLSL
		}
		
		Pass 
		{
		  Name "DOF WITH EDGE DETECTION"
		  ZTest False
		  ZWrite Off
		  Cull Off
		  GLSLPROGRAM
			#include "UnityCG.glslinc"
			#include "GLSLSupport.glslinc"
			  #ifdef VERTEX
				attribute vec4 _glesVertex;
				attribute vec4 _glesMultiTexCoord0;
				uniform highp mat4 glstate_matrix_mvp;
				uniform mediump vec4 _TexelSize;
				varying mediump vec2 xlv_TEXCOORD0;
				varying mediump vec2 xlv_TEXCOORD0_1;
				varying mediump vec2 xlv_TEXCOORD0_2;
				void main ()
				{
				  gl_Position = (glstate_matrix_mvp * _glesVertex);
				  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
				  xlv_TEXCOORD0_1 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(1.0, 0.0)));
				  xlv_TEXCOORD0_2 = (_glesMultiTexCoord0.xy + (_TexelSize.xy * vec2(0.0, 1.0)));
				}


				#endif
				#ifdef FRAGMENT
				uniform sampler2D _MainTex;
				uniform sampler2D _BlurTex;
				varying mediump vec2 xlv_TEXCOORD0;
				varying mediump vec2 xlv_TEXCOORD0_1;
				varying mediump vec2 xlv_TEXCOORD0_2;
				void main ()
				{
				  lowp vec4 color_1;
				  lowp vec4 tmpvar_2;
				  tmpvar_2 = texture2D (_MainTex, xlv_TEXCOORD0);
				  color_1.xyz = (mix (tmpvar_2.xyz, texture2D (_BlurTex, xlv_TEXCOORD0).xyz, tmpvar_2.www) * (vec3(1.0, 1.0, 1.0) - vec3((
					(abs((texture2D (_MainTex, xlv_TEXCOORD0_1).w - tmpvar_2.w)) + abs((texture2D (_MainTex, xlv_TEXCOORD0_2).w - tmpvar_2.w)))
				   * 10.0))));
				  color_1.w = 1.0;
				  gl_FragData[0] = color_1;
				}


				#endif
			ENDGLSL
		}
	}
}