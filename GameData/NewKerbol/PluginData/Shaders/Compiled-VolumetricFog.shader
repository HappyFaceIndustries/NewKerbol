Shader "FX/Spherical Fog" {
	Properties {
		_FogBaseColor ("Fog Base Color", Color) = (0,1,1,1)
		_FogDenseColor ("Fog Dense Color", Color) = (1,1,1,1)
		_InnerRatio ("Inner Ratio", Range (0.0, 0.9999)) = 0.5
		_Density ("Density", Range (0.0, 10.0)) = 10.0
		_ColorFalloff ("Color Falloff", Range (0.0, 50.0)) = 16.0
	}
	 
	Category {
		Tags { "Queue"="Transparent+99" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off Lighting Off ZWrite Off
		ZTest Always
	 	
		SubShader {
			Pass {
				Program "vp" {
// Vertex combos: 1
//   opengl - ALU: 19 to 19
//   d3d9 - ALU: 19 to 19
//   d3d11 - ALU: 16 to 16, TEX: 0 to 0, FLOW: 1 to 1
SubProgram "opengl " {
Keywords { }
Bind "vertex" Vertex
Vector 9 [_WorldSpaceCameraPos]
Vector 10 [_ProjectionParams]
Matrix 5 [_Object2World]
"3.0-!!ARBvp1.0
# 19 ALU
PARAM c[11] = { { 0, 0.5 },
		state.matrix.mvp,
		program.local[5..10] };
TEMP R0;
TEMP R1;
DP4 R0.z, vertex.position, c[4];
MOV R0.w, R0.z;
DP4 R0.x, vertex.position, c[1];
DP4 R0.y, vertex.position, c[2];
MUL R1.xyz, R0.xyww, c[0].y;
MOV result.position.xyw, R0;
MUL R1.y, R1, c[10].x;
ADD result.texcoord[1].xy, R1, R1.z;
DP4 R0.x, vertex.position, c[3];
RCP R0.y, R0.z;
MOV R0.z, R0.x;
MUL R0.y, R0.x, R0;
SLT R0.y, c[0].x, R0;
DP4 R1.z, vertex.position, c[7];
DP4 R1.x, vertex.position, c[5];
DP4 R1.y, vertex.position, c[6];
MOV result.texcoord[1].zw, R0;
ADD result.texcoord[0].xyz, R1, -c[9];
MUL result.position.z, R0.x, R0.y;
END
# 19 instructions, 2 R-regs
"
}

SubProgram "d3d9 " {
Keywords { }
Bind "vertex" Vertex
Matrix 0 [glstate_matrix_mvp]
Vector 8 [_WorldSpaceCameraPos]
Vector 9 [_ProjectionParams]
Vector 10 [_ScreenParams]
Matrix 4 [_Object2World]
"vs_3_0
; 19 ALU
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
def c11, 0.00000000, 0.50000000, 0, 0
dcl_position0 v0
dp4 r0.z, v0, c3
mov r0.w, r0.z
dp4 r0.x, v0, c0
dp4 r0.y, v0, c1
mul r1.xyz, r0.xyww, c11.y
mov o0.xyw, r0
mul r1.y, r1, c9.x
mad o2.xy, r1.z, c10.zwzw, r1
dp4 r0.x, v0, c2
rcp r0.y, r0.z
mov r0.z, r0.x
mul r0.y, r0.x, r0
slt r0.y, c11.x, r0
dp4 r1.z, v0, c6
dp4 r1.x, v0, c4
dp4 r1.y, v0, c5
mov o2.zw, r0
add o1.xyz, r1, -c8
mul o0.z, r0.x, r0.y
"
}

SubProgram "d3d11 " {
Keywords { }
Bind "vertex" Vertex
ConstBuffer "UnityPerCamera" 128 // 96 used size, 8 vars
Vector 64 [_WorldSpaceCameraPos] 3
Vector 80 [_ProjectionParams] 4
ConstBuffer "UnityPerDraw" 336 // 256 used size, 6 vars
Matrix 0 [glstate_matrix_mvp] 4
Matrix 192 [_Object2World] 4
BindCB "UnityPerCamera" 0
BindCB "UnityPerDraw" 1
// 19 instructions, 2 temp regs, 0 temp arrays:
// ALU 15 float, 0 int, 1 uint
// TEX 0 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"vs_4_0
eefiecedhkcniehannhefichkngjklpkiapckmhkabaaaaaanaadaaaaadaaaaaa
cmaaaaaakaaaaaaabaabaaaaejfdeheogmaaaaaaadaaaaaaaiaaaaaafaaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaafjaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahaaaaaagaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaafaepfdejfeejepeoaaeoepfcenebemaafeeffiedepepfceeaaklklkl
epfdeheogiaaaaaaadaaaaaaaiaaaaaafaaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaafmaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaa
fmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaapaaaaaafdfgfpfaepfdejfe
ejepeoaafeeffiedepepfceeaaklklklfdeieefcliacaaaaeaaaabaakoaaaaaa
fjaaaaaeegiocaaaaaaaaaaaagaaaaaafjaaaaaeegiocaaaabaaaaaabaaaaaaa
fpaaaaadpcbabaaaaaaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaad
hccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagiaaaaacacaaaaaadiaaaaai
pcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaabaaaaaaabaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaabaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaacaaaaaakgbkbaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaa
adaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaaaoaaaaahbcaabaaaabaaaaaa
ckaabaaaaaaaaaaadkaabaaaaaaaaaaadbaaaaahbcaabaaaabaaaaaaabeaaaaa
aaaaaaaaakaabaaaabaaaaaaabaaaaahbcaabaaaabaaaaaaakaabaaaabaaaaaa
abeaaaaaaaaaiadpdiaaaaaheccabaaaaaaaaaaackaabaaaaaaaaaaaakaabaaa
abaaaaaadgaaaaaflccabaaaaaaaaaaaegambaaaaaaaaaaadiaaaaaihcaabaaa
abaaaaaafgbfbaaaaaaaaaaaegiccaaaabaaaaaaanaaaaaadcaaaaakhcaabaaa
abaaaaaaegiccaaaabaaaaaaamaaaaaaagbabaaaaaaaaaaaegacbaaaabaaaaaa
dcaaaaakhcaabaaaabaaaaaaegiccaaaabaaaaaaaoaaaaaakgbkbaaaaaaaaaaa
egacbaaaabaaaaaadcaaaaakhcaabaaaabaaaaaaegiccaaaabaaaaaaapaaaaaa
pgbpbaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaajhccabaaaabaaaaaaegacbaaa
abaaaaaaegiccaiaebaaaaaaaaaaaaaaaeaaaaaadiaaaaaiccaabaaaaaaaaaaa
bkaabaaaaaaaaaaaakiacaaaaaaaaaaaafaaaaaadiaaaaakncaabaaaabaaaaaa
agahbaaaaaaaaaaaaceaaaaaaaaaaadpaaaaaaaaaaaaaadpaaaaaadpdgaaaaaf
mccabaaaacaaaaaakgaobaaaaaaaaaaaaaaaaaahdccabaaaacaaaaaakgakbaaa
abaaaaaamgaabaaaabaaaaaadoaaaaab"
}

SubProgram "gles " {
Keywords { }
"!!GLES


#ifdef VERTEX

varying highp vec4 xlv_TEXCOORD1;
varying highp vec3 xlv_TEXCOORD0;
uniform highp mat4 _Object2World;
uniform highp mat4 glstate_matrix_mvp;
uniform highp vec4 _ProjectionParams;
uniform highp vec3 _WorldSpaceCameraPos;
attribute vec4 _glesVertex;
void main ()
{
  highp vec4 tmpvar_1;
  highp vec4 tmpvar_2;
  tmpvar_2 = (glstate_matrix_mvp * _glesVertex);
  tmpvar_1.xyw = tmpvar_2.xyw;
  highp vec4 o_3;
  highp vec4 tmpvar_4;
  tmpvar_4 = (tmpvar_2 * 0.5);
  highp vec2 tmpvar_5;
  tmpvar_5.x = tmpvar_4.x;
  tmpvar_5.y = (tmpvar_4.y * _ProjectionParams.x);
  o_3.xy = (tmpvar_5 + tmpvar_4.w);
  o_3.zw = tmpvar_2.zw;
  tmpvar_1.z = (tmpvar_2.z * float(((tmpvar_2.z / tmpvar_2.w) > 0.0)));
  gl_Position = tmpvar_1;
  xlv_TEXCOORD0 = ((_Object2World * _glesVertex).xyz - _WorldSpaceCameraPos);
  xlv_TEXCOORD1 = o_3;
}



#endif
#ifdef FRAGMENT

varying highp vec4 xlv_TEXCOORD1;
varying highp vec3 xlv_TEXCOORD0;
uniform highp vec4 FogParam;
uniform sampler2D _CameraDepthTexture;
uniform highp float _ColorFalloff;
uniform highp float _Density;
uniform highp float _InnerRatio;
uniform lowp vec4 _FogDenseColor;
uniform lowp vec4 _FogBaseColor;
uniform highp vec4 _ZBufferParams;
uniform highp vec3 _WorldSpaceCameraPos;
void main ()
{
  mediump vec4 color_1;
  color_1 = vec4(1.0, 1.0, 1.0, 1.0);
  lowp vec4 tmpvar_2;
  tmpvar_2 = texture2DProj (_CameraDepthTexture, xlv_TEXCOORD1);
  highp float z_3;
  z_3 = tmpvar_2.x;
  highp float sphereRadius_4;
  sphereRadius_4 = (FogParam.w * 0.001);
  highp float density_5;
  density_5 = (_Density * 1000.0);
  highp vec3 viewDirection_6;
  viewDirection_6 = normalize(xlv_TEXCOORD0);
  highp float maxDistance_7;
  maxDistance_7 = ((1.0/(((_ZBufferParams.z * z_3) + _ZBufferParams.w))) * 0.001);
  highp float tmpvar_8;
  int seg_9;
  highp float clarity_10;
  highp float centerValue_11;
  highp float step_contribution_12;
  highp float step_distance_13;
  highp float xlat_varsample_14;
  highp vec3 local_15;
  highp vec3 tmpvar_16;
  tmpvar_16 = ((_WorldSpaceCameraPos * 0.001) - (FogParam.xyz * 0.001));
  local_15 = tmpvar_16;
  highp float tmpvar_17;
  tmpvar_17 = dot (viewDirection_6, viewDirection_6);
  highp float tmpvar_18;
  tmpvar_18 = (2.0 * dot (viewDirection_6, tmpvar_16));
  highp float tmpvar_19;
  tmpvar_19 = ((tmpvar_18 * tmpvar_18) - ((4.0 * tmpvar_17) * (dot (tmpvar_16, tmpvar_16) - (sphereRadius_4 * sphereRadius_4))));
  if ((tmpvar_19 <= 0.0)) {
    tmpvar_8 = 0.0;
  } else {
    highp float tmpvar_20;
    tmpvar_20 = (0.5 / tmpvar_17);
    highp float tmpvar_21;
    tmpvar_21 = sqrt(tmpvar_19);
    highp float tmpvar_22;
    tmpvar_22 = max (((-(tmpvar_18) - tmpvar_21) * tmpvar_20), 0.0);
    xlat_varsample_14 = tmpvar_22;
    highp float tmpvar_23;
    tmpvar_23 = ((min (maxDistance_7, max (((-(tmpvar_18) + tmpvar_21) * tmpvar_20), 0.0)) - tmpvar_22) / 10.0);
    step_distance_13 = tmpvar_23;
    step_contribution_12 = ((1.0 - (1.0/(pow (2.0, tmpvar_23)))) * density_5);
    centerValue_11 = (1.0/((1.0 - _InnerRatio)));
    clarity_10 = 1.0;
    seg_9 = 0;
    for (int seg_9 = 0; seg_9 < 10; ) {
      highp vec3 tmpvar_24;
      tmpvar_24 = (local_15 + (viewDirection_6 * xlat_varsample_14));
      clarity_10 = (clarity_10 * (1.0 - clamp ((clamp ((centerValue_11 * (1.0 - (sqrt(dot (tmpvar_24, tmpvar_24)) / sphereRadius_4))), 0.0, 1.0) * step_contribution_12), 0.0, 1.0)));
      xlat_varsample_14 = (xlat_varsample_14 + step_distance_13);
      seg_9 = (seg_9 + 1);
    };
    tmpvar_8 = (1.0 - clarity_10);
  };
  highp float tmpvar_25;
  tmpvar_25 = pow (tmpvar_8, _ColorFalloff);
  highp vec3 tmpvar_26;
  tmpvar_26 = mix (_FogBaseColor.xyz, _FogDenseColor.xyz, vec3(tmpvar_25));
  color_1.xyz = tmpvar_26;
  highp float tmpvar_27;
  tmpvar_27 = (tmpvar_8 * mix (_FogBaseColor.w, _FogDenseColor.w, tmpvar_25));
  color_1.w = tmpvar_27;
  gl_FragData[0] = color_1;
}



#endif"
}

SubProgram "glesdesktop " {
Keywords { }
"!!GLES


#ifdef VERTEX

varying highp vec4 xlv_TEXCOORD1;
varying highp vec3 xlv_TEXCOORD0;
uniform highp mat4 _Object2World;
uniform highp mat4 glstate_matrix_mvp;
uniform highp vec4 _ProjectionParams;
uniform highp vec3 _WorldSpaceCameraPos;
attribute vec4 _glesVertex;
void main ()
{
  highp vec4 tmpvar_1;
  highp vec4 tmpvar_2;
  tmpvar_2 = (glstate_matrix_mvp * _glesVertex);
  tmpvar_1.xyw = tmpvar_2.xyw;
  highp vec4 o_3;
  highp vec4 tmpvar_4;
  tmpvar_4 = (tmpvar_2 * 0.5);
  highp vec2 tmpvar_5;
  tmpvar_5.x = tmpvar_4.x;
  tmpvar_5.y = (tmpvar_4.y * _ProjectionParams.x);
  o_3.xy = (tmpvar_5 + tmpvar_4.w);
  o_3.zw = tmpvar_2.zw;
  tmpvar_1.z = (tmpvar_2.z * float(((tmpvar_2.z / tmpvar_2.w) > 0.0)));
  gl_Position = tmpvar_1;
  xlv_TEXCOORD0 = ((_Object2World * _glesVertex).xyz - _WorldSpaceCameraPos);
  xlv_TEXCOORD1 = o_3;
}



#endif
#ifdef FRAGMENT

varying highp vec4 xlv_TEXCOORD1;
varying highp vec3 xlv_TEXCOORD0;
uniform highp vec4 FogParam;
uniform sampler2D _CameraDepthTexture;
uniform highp float _ColorFalloff;
uniform highp float _Density;
uniform highp float _InnerRatio;
uniform lowp vec4 _FogDenseColor;
uniform lowp vec4 _FogBaseColor;
uniform highp vec4 _ZBufferParams;
uniform highp vec3 _WorldSpaceCameraPos;
void main ()
{
  mediump vec4 color_1;
  color_1 = vec4(1.0, 1.0, 1.0, 1.0);
  lowp vec4 tmpvar_2;
  tmpvar_2 = texture2DProj (_CameraDepthTexture, xlv_TEXCOORD1);
  highp float z_3;
  z_3 = tmpvar_2.x;
  highp float sphereRadius_4;
  sphereRadius_4 = (FogParam.w * 0.001);
  highp float density_5;
  density_5 = (_Density * 1000.0);
  highp vec3 viewDirection_6;
  viewDirection_6 = normalize(xlv_TEXCOORD0);
  highp float maxDistance_7;
  maxDistance_7 = ((1.0/(((_ZBufferParams.z * z_3) + _ZBufferParams.w))) * 0.001);
  highp float tmpvar_8;
  int seg_9;
  highp float clarity_10;
  highp float centerValue_11;
  highp float step_contribution_12;
  highp float step_distance_13;
  highp float xlat_varsample_14;
  highp vec3 local_15;
  highp vec3 tmpvar_16;
  tmpvar_16 = ((_WorldSpaceCameraPos * 0.001) - (FogParam.xyz * 0.001));
  local_15 = tmpvar_16;
  highp float tmpvar_17;
  tmpvar_17 = dot (viewDirection_6, viewDirection_6);
  highp float tmpvar_18;
  tmpvar_18 = (2.0 * dot (viewDirection_6, tmpvar_16));
  highp float tmpvar_19;
  tmpvar_19 = ((tmpvar_18 * tmpvar_18) - ((4.0 * tmpvar_17) * (dot (tmpvar_16, tmpvar_16) - (sphereRadius_4 * sphereRadius_4))));
  if ((tmpvar_19 <= 0.0)) {
    tmpvar_8 = 0.0;
  } else {
    highp float tmpvar_20;
    tmpvar_20 = (0.5 / tmpvar_17);
    highp float tmpvar_21;
    tmpvar_21 = sqrt(tmpvar_19);
    highp float tmpvar_22;
    tmpvar_22 = max (((-(tmpvar_18) - tmpvar_21) * tmpvar_20), 0.0);
    xlat_varsample_14 = tmpvar_22;
    highp float tmpvar_23;
    tmpvar_23 = ((min (maxDistance_7, max (((-(tmpvar_18) + tmpvar_21) * tmpvar_20), 0.0)) - tmpvar_22) / 10.0);
    step_distance_13 = tmpvar_23;
    step_contribution_12 = ((1.0 - (1.0/(pow (2.0, tmpvar_23)))) * density_5);
    centerValue_11 = (1.0/((1.0 - _InnerRatio)));
    clarity_10 = 1.0;
    seg_9 = 0;
    for (int seg_9 = 0; seg_9 < 10; ) {
      highp vec3 tmpvar_24;
      tmpvar_24 = (local_15 + (viewDirection_6 * xlat_varsample_14));
      clarity_10 = (clarity_10 * (1.0 - clamp ((clamp ((centerValue_11 * (1.0 - (sqrt(dot (tmpvar_24, tmpvar_24)) / sphereRadius_4))), 0.0, 1.0) * step_contribution_12), 0.0, 1.0)));
      xlat_varsample_14 = (xlat_varsample_14 + step_distance_13);
      seg_9 = (seg_9 + 1);
    };
    tmpvar_8 = (1.0 - clarity_10);
  };
  highp float tmpvar_25;
  tmpvar_25 = pow (tmpvar_8, _ColorFalloff);
  highp vec3 tmpvar_26;
  tmpvar_26 = mix (_FogBaseColor.xyz, _FogDenseColor.xyz, vec3(tmpvar_25));
  color_1.xyz = tmpvar_26;
  highp float tmpvar_27;
  tmpvar_27 = (tmpvar_8 * mix (_FogBaseColor.w, _FogDenseColor.w, tmpvar_25));
  color_1.w = tmpvar_27;
  gl_FragData[0] = color_1;
}



#endif"
}

SubProgram "gles3 " {
Keywords { }
"!!GLES3#version 300 es


#ifdef VERTEX

#define gl_Vertex _glesVertex
in vec4 _glesVertex;
#define gl_Normal (normalize(_glesNormal))
in vec3 _glesNormal;
#define gl_MultiTexCoord0 _glesMultiTexCoord0
in vec4 _glesMultiTexCoord0;

#line 151
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 187
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 181
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 352
struct v2f {
    highp vec4 pos;
    highp vec3 view;
    highp vec4 projPos;
};
#line 52
struct appdata_base {
    highp vec4 vertex;
    highp vec3 normal;
    highp vec4 texcoord;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[8];
uniform highp vec4 unity_LightPosition[8];
uniform highp vec4 unity_LightAtten[8];
#line 19
uniform highp vec4 unity_SpotDirection[8];
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
#line 23
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
#line 27
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
#line 31
uniform highp vec4 _LightSplitsNear;
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
#line 35
uniform highp vec4 unity_ShadowFadeCenterAndType;
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
#line 39
uniform highp mat4 _Object2World;
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
#line 43
uniform highp mat4 glstate_matrix_texture0;
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
#line 47
uniform highp mat4 glstate_matrix_projection;
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
#line 51
uniform lowp vec4 unity_ColorSpaceGrey;
#line 77
#line 82
#line 87
#line 91
#line 96
#line 120
#line 137
#line 158
#line 166
#line 193
#line 206
#line 215
#line 220
#line 229
#line 234
#line 243
#line 260
#line 265
#line 291
#line 299
#line 307
#line 311
#line 315
#line 345
uniform lowp vec4 _FogBaseColor;
uniform lowp vec4 _FogDenseColor;
uniform highp float _InnerRatio;
uniform highp float _Density;
#line 349
uniform highp float _ColorFalloff;
uniform sampler2D _CameraDepthTexture;
uniform highp vec4 FogParam;
#line 359
#line 284
highp vec4 ComputeScreenPos( in highp vec4 pos ) {
    #line 286
    highp vec4 o = (pos * 0.5);
    o.xy = (vec2( o.x, (o.y * _ProjectionParams.x)) + o.w);
    o.zw = pos.zw;
    return o;
}
#line 359
v2f vert( in appdata_base v ) {
    v2f o;
    highp vec4 wPos = (_Object2World * v.vertex);
    #line 363
    o.pos = (glstate_matrix_mvp * v.vertex);
    o.view = (wPos.xyz - _WorldSpaceCameraPos);
    o.projPos = ComputeScreenPos( o.pos);
    highp float inFrontOf = float(((o.pos.z / o.pos.w) > 0.0));
    #line 367
    o.pos.z *= inFrontOf;
    return o;
}
out highp vec3 xlv_TEXCOORD0;
out highp vec4 xlv_TEXCOORD1;
void main() {
    v2f xl_retval;
    appdata_base xlt_v;
    xlt_v.vertex = vec4(gl_Vertex);
    xlt_v.normal = vec3(gl_Normal);
    xlt_v.texcoord = vec4(gl_MultiTexCoord0);
    xl_retval = vert( xlt_v);
    gl_Position = vec4(xl_retval.pos);
    xlv_TEXCOORD0 = vec3(xl_retval.view);
    xlv_TEXCOORD1 = vec4(xl_retval.projPos);
}


#endif
#ifdef FRAGMENT

#define gl_FragData _glesFragData
layout(location = 0) out mediump vec4 _glesFragData[4];
float xll_saturate_f( float x) {
  return clamp( x, 0.0, 1.0);
}
vec2 xll_saturate_vf2( vec2 x) {
  return clamp( x, 0.0, 1.0);
}
vec3 xll_saturate_vf3( vec3 x) {
  return clamp( x, 0.0, 1.0);
}
vec4 xll_saturate_vf4( vec4 x) {
  return clamp( x, 0.0, 1.0);
}
mat2 xll_saturate_mf2x2(mat2 m) {
  return mat2( clamp(m[0], 0.0, 1.0), clamp(m[1], 0.0, 1.0));
}
mat3 xll_saturate_mf3x3(mat3 m) {
  return mat3( clamp(m[0], 0.0, 1.0), clamp(m[1], 0.0, 1.0), clamp(m[2], 0.0, 1.0));
}
mat4 xll_saturate_mf4x4(mat4 m) {
  return mat4( clamp(m[0], 0.0, 1.0), clamp(m[1], 0.0, 1.0), clamp(m[2], 0.0, 1.0), clamp(m[3], 0.0, 1.0));
}
#line 151
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 187
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 181
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 352
struct v2f {
    highp vec4 pos;
    highp vec3 view;
    highp vec4 projPos;
};
#line 52
struct appdata_base {
    highp vec4 vertex;
    highp vec3 normal;
    highp vec4 texcoord;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[8];
uniform highp vec4 unity_LightPosition[8];
uniform highp vec4 unity_LightAtten[8];
#line 19
uniform highp vec4 unity_SpotDirection[8];
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
#line 23
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
#line 27
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
#line 31
uniform highp vec4 _LightSplitsNear;
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
#line 35
uniform highp vec4 unity_ShadowFadeCenterAndType;
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
#line 39
uniform highp mat4 _Object2World;
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
#line 43
uniform highp mat4 glstate_matrix_texture0;
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
#line 47
uniform highp mat4 glstate_matrix_projection;
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
#line 51
uniform lowp vec4 unity_ColorSpaceGrey;
#line 77
#line 82
#line 87
#line 91
#line 96
#line 120
#line 137
#line 158
#line 166
#line 193
#line 206
#line 215
#line 220
#line 229
#line 234
#line 243
#line 260
#line 265
#line 291
#line 299
#line 307
#line 311
#line 315
#line 345
uniform lowp vec4 _FogBaseColor;
uniform lowp vec4 _FogDenseColor;
uniform highp float _InnerRatio;
uniform highp float _Density;
#line 349
uniform highp float _ColorFalloff;
uniform sampler2D _CameraDepthTexture;
uniform highp vec4 FogParam;
#line 359
#line 315
highp float CalcVolumeFogIntensity( in highp vec3 sphereCenter, in highp float sphereRadius, in highp float innerRatio, in highp float density, in highp vec3 cameraPosition, in highp vec3 viewDirection, in highp float maxDistance ) {
    highp vec3 local = (cameraPosition - sphereCenter);
    highp float fA = dot( viewDirection, viewDirection);
    #line 319
    highp float fB = (2.0 * dot( viewDirection, local));
    highp float fC = (dot( local, local) - (sphereRadius * sphereRadius));
    highp float fD = ((fB * fB) - ((4.0 * fA) * fC));
    if ((fD <= 0.0)){
        return 0.0;
    }
    #line 323
    highp float recpTwoA = (0.5 / fA);
    highp float DSqrt = sqrt(fD);
    highp float dist = max( (((-fB) - DSqrt) * recpTwoA), 0.0);
    highp float dist2 = max( (((-fB) + DSqrt) * recpTwoA), 0.0);
    #line 327
    highp float backDepth = min( maxDistance, dist2);
    highp float xlat_varsample = dist;
    highp float step_distance = ((backDepth - dist) / 10.0);
    highp float step_contribution = ((1.0 - (1.0 / pow( 2.0, step_distance))) * density);
    #line 331
    highp float centerValue = (1.0 / (1.0 - innerRatio));
    highp float clarity = 1.0;
    highp int seg = 0;
    for ( ; (seg < 10); (seg++)) {
        #line 337
        highp vec3 position = (local + (viewDirection * xlat_varsample));
        highp float val = xll_saturate_f((centerValue * (1.0 - (length(position) / sphereRadius))));
        highp float sample_fog_amount = xll_saturate_f((val * step_contribution));
        clarity *= (1.0 - sample_fog_amount);
        #line 341
        xlat_varsample += step_distance;
    }
    return (1.0 - clarity);
}
#line 280
highp float LinearEyeDepth( in highp float z ) {
    #line 282
    return (1.0 / ((_ZBufferParams.z * z) + _ZBufferParams.w));
}
#line 370
mediump vec4 frag( in v2f i ) {
    #line 372
    mediump vec4 color = vec4( 1.0, 1.0, 1.0, 1.0);
    highp float depth = LinearEyeDepth( textureProj( _CameraDepthTexture, i.projPos).x);
    highp vec3 viewDir = normalize(i.view);
    highp float fog = CalcVolumeFogIntensity( (FogParam.xyz * 0.001), (FogParam.w * 0.001), _InnerRatio, (_Density * 1000.0), (_WorldSpaceCameraPos * 0.001), viewDir, (depth * 0.001));
    #line 376
    highp float denseColorRatio = pow( fog, _ColorFalloff);
    color.xyz = mix( _FogBaseColor.xyz, _FogDenseColor.xyz, vec3( denseColorRatio));
    color.w = (fog * mix( _FogBaseColor.w, _FogDenseColor.w, denseColorRatio));
    return color;
}
in highp vec3 xlv_TEXCOORD0;
in highp vec4 xlv_TEXCOORD1;
void main() {
    mediump vec4 xl_retval;
    v2f xlt_i;
    xlt_i.pos = vec4(0.0);
    xlt_i.view = vec3(xlv_TEXCOORD0);
    xlt_i.projPos = vec4(xlv_TEXCOORD1);
    xl_retval = frag( xlt_i);
    gl_FragData[0] = vec4(xl_retval);
}


#endif"
}

}
Program "fp" {
// Fragment combos: 1
//   opengl - ALU: 150 to 150, TEX: 1 to 1
//   d3d9 - ALU: 155 to 155, TEX: 1 to 1, FLOW: 1 to 1
//   d3d11 - ALU: 56 to 56, TEX: 1 to 1, FLOW: 4 to 4
SubProgram "opengl " {
Keywords { }
Vector 0 [_WorldSpaceCameraPos]
Vector 1 [_ZBufferParams]
Vector 2 [_FogBaseColor]
Vector 3 [_FogDenseColor]
Float 4 [_InnerRatio]
Float 5 [_Density]
Float 6 [_ColorFalloff]
Vector 7 [FogParam]
SetTexture 0 [_CameraDepthTexture] 2D
"3.0-!!ARBfp1.0
# 150 ALU, 1 TEX
PARAM c[10] = { program.local[0..7],
		{ 0.001, 2, 4, 1 },
		{ 0, 0.5, 0.1, 1000 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
TEMP R5;
MOV R3.xy, c[8].xwzw;
DP3 R0.x, fragment.texcoord[0], fragment.texcoord[0];
RSQ R0.x, R0.x;
MUL R2.xyz, R0.x, fragment.texcoord[0];
MOV R1.xyz, c[7];
ADD R1.xyz, -R1, c[0];
MUL R1.xyz, R1, c[8].x;
DP3 R0.x, R1, R1;
MUL R2.w, R3.x, c[7];
DP3 R0.z, R2, R2;
MAD R0.x, -R2.w, R2.w, R0;
MUL R0.y, R0.z, R0.x;
RCP R3.x, R0.z;
DP3 R0.x, R2, R1;
MUL R1.w, R0.y, c[8].z;
MUL R0.y, R0.x, c[8];
MAD R1.w, R0.y, R0.y, -R1;
RSQ R0.x, R1.w;
RCP R0.z, R0.x;
ADD R0.x, -R0.y, -R0.z;
MUL R3.x, R3, c[9].y;
MUL R3.z, R0.x, R3.x;
ADD R0.y, -R0, R0.z;
MUL R0.y, R3.x, R0;
TXP R0.x, fragment.texcoord[1], texture[0], 2D;
MAD R0.x, R0, c[1].z, c[1].w;
RCP R0.x, R0.x;
MAX R4.w, R3.z, c[9].x;
MAX R0.y, R0, c[9].x;
MUL R0.x, R0, c[8];
MIN R0.x, R0, R0.y;
ADD R0.x, -R4.w, R0;
MUL R4.x, R0, c[9].z;
ADD R3.x, R4.w, R4;
ADD R3.z, R4.x, R3.x;
ADD R4.z, R4.x, R3;
MAD R0.xyz, R2, R4.z, R1;
DP3 R0.x, R0, R0;
RSQ R3.w, R0.x;
MAD R0.xyz, R2, R3.z, R1;
DP3 R0.y, R0, R0;
RCP R3.z, R3.w;
RCP R3.w, R2.w;
ADD R3.y, R3, -c[4].x;
POW R0.x, c[8].y, R4.x;
RSQ R0.y, R0.y;
RCP R0.x, R0.x;
RCP R0.y, R0.y;
ADD R0.x, -R0, c[8].w;
MUL R0.x, R0, c[5];
RCP R2.w, R3.y;
MAD R3.z, R3.w, -R3, c[8].w;
MAD R0.y, R3.w, -R0, c[8].w;
MUL R4.y, R0.x, c[9].w;
MUL_SAT R3.y, R2.w, R3.z;
MUL_SAT R0.y, R2.w, R0;
MUL_SAT R0.x, R4.y, R0.y;
MUL_SAT R5.y, R4, R3;
MAD R3.xyz, R2, R3.x, R1;
DP3 R3.x, R3, R3;
ADD R5.x, -R0, c[8].w;
MAD R0.xyz, R2, R4.w, R1;
DP3 R0.x, R0, R0;
RSQ R0.y, R3.x;
RSQ R0.x, R0.x;
RCP R0.y, R0.y;
RCP R0.x, R0.x;
MAD R0.y, R3.w, -R0, c[8].w;
MAD R0.x, -R0, R3.w, c[8].w;
MUL_SAT R0.y, R2.w, R0;
MUL_SAT R0.x, R2.w, R0;
MUL_SAT R0.y, R4, R0;
MUL_SAT R0.x, R0, R4.y;
ADD R0.y, -R0, c[8].w;
ADD R0.x, -R0, c[8].w;
MUL R0.x, R0, R0.y;
MUL R0.x, R0, R5;
ADD R0.y, -R5, c[8].w;
MUL R3.x, R0, R0.y;
ADD R3.y, R4.x, R4.z;
MAD R0.xyz, R2, R3.y, R1;
DP3 R3.z, R0, R0;
ADD R3.y, R4.x, R3;
MAD R0.xyz, R2, R3.y, R1;
DP3 R0.y, R0, R0;
RSQ R3.z, R3.z;
RCP R0.x, R3.z;
RSQ R0.y, R0.y;
MAD R0.x, R3.w, -R0, c[8].w;
RCP R0.y, R0.y;
MUL_SAT R0.x, R2.w, R0;
MAD R0.y, R3.w, -R0, c[8].w;
MUL_SAT R0.x, R4.y, R0;
MUL_SAT R0.y, R2.w, R0;
ADD R0.x, -R0, c[8].w;
MUL_SAT R0.y, R4, R0;
MUL R0.x, R3, R0;
ADD R0.y, -R0, c[8].w;
MUL R3.x, R0, R0.y;
ADD R3.y, R4.x, R3;
MAD R0.xyz, R2, R3.y, R1;
DP3 R3.z, R0, R0;
ADD R3.y, R4.x, R3;
MAD R0.xyz, R2, R3.y, R1;
DP3 R0.y, R0, R0;
RSQ R3.z, R3.z;
RCP R0.x, R3.z;
ADD R3.y, R4.x, R3;
RSQ R0.y, R0.y;
MAD R0.x, R3.w, -R0, c[8].w;
RCP R0.y, R0.y;
MUL_SAT R0.x, R2.w, R0;
MAD R0.y, R3.w, -R0, c[8].w;
MUL_SAT R0.x, R4.y, R0;
MUL_SAT R0.y, R2.w, R0;
ADD R0.x, -R0, c[8].w;
MUL_SAT R0.y, R4, R0;
MUL R0.x, R3, R0;
ADD R0.y, -R0, c[8].w;
MUL R3.x, R0, R0.y;
MAD R0.xyz, R2, R3.y, R1;
ADD R3.z, R4.x, R3.y;
DP3 R3.y, R0, R0;
MAD R0.xyz, R2, R3.z, R1;
DP3 R0.y, R0, R0;
RSQ R1.x, R3.y;
RCP R0.x, R1.x;
RSQ R0.y, R0.y;
MAD R0.x, R3.w, -R0, c[8].w;
RCP R0.y, R0.y;
MUL_SAT R0.x, R2.w, R0;
MAD R0.y, R3.w, -R0, c[8].w;
MUL_SAT R0.y, R2.w, R0;
MUL_SAT R0.z, R4.y, R0.y;
MUL_SAT R0.x, R4.y, R0;
ADD R0.x, -R0, c[8].w;
MUL R0.y, R3.x, R0.x;
ADD R0.z, -R0, c[8].w;
MOV R0.x, c[9];
MAD R1.y, -R0, R0.z, c[8].w;
CMP R1.x, -R1.w, c[8].w, R0;
CMP R1.z, -R1.w, R0.w, c[9].x;
MOV R0, c[2];
CMP R1.x, -R1, R1.y, R1.z;
ADD R1.y, -R0.w, c[3].w;
POW R0.w, R1.x, c[6].x;
MAD R1.y, R0.w, R1, c[2].w;
ADD R0.xyz, -R0, c[3];
MUL result.color.w, R1.x, R1.y;
MAD result.color.xyz, R0.w, R0, c[2];
END
# 150 instructions, 6 R-regs
"
}

SubProgram "d3d9 " {
Keywords { }
Vector 0 [_WorldSpaceCameraPos]
Vector 1 [_ZBufferParams]
Vector 2 [_FogBaseColor]
Vector 3 [_FogDenseColor]
Float 4 [_InnerRatio]
Float 5 [_Density]
Float 6 [_ColorFalloff]
Vector 7 [FogParam]
SetTexture 0 [_CameraDepthTexture] 2D
"ps_3_0
; 155 ALU, 1 TEX, 1 FLOW
dcl_2d s0
def c8, 0.00100000, 1000.00000000, 2.00000000, 4.00000000
def c9, 0.00000000, 1.00000000, 0.50000000, 0.10000000
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1
mov r1.xyz, c0
add r2.xyz, -c7, r1
mul r2.xyz, r2, c8.x
dp3 r0.x, v0, v0
rsq r0.x, r0.x
mul r1.xyz, r0.x, v0
mov r0.x, c7.w
dp3 r0.y, r2, r2
mul r0.w, c8.x, r0.x
mad r0.x, -r0.w, r0.w, r0.y
dp3 r0.y, r1, r1
mul r0.z, r0.y, r0.x
dp3 r0.x, r1, r2
mul r2.w, r0.z, c8
mul r0.z, r0.x, c8
mad r2.w, r0.z, r0.z, -r2
texldp r0.x, v1, s0
mad r3.x, r0, c1.z, c1.w
mov r0.x, c5
cmp_pp r3.y, -r2.w, c9.x, c9
rcp r3.x, r3.x
mul r4.y, c8, r0.x
mul r0.x, r3, c8
cmp r1.w, -r2, c9.x, r1
if_gt r3.y, c9.x
rsq r1.w, r2.w
rcp r1.w, r1.w
rcp r0.y, r0.y
mul r2.w, r0.y, c9.z
add r3.x, -r0.z, r1.w
add r0.y, -r0.z, -r1.w
mul r3.x, r2.w, r3
mul r0.y, r0, r2.w
max r0.z, r3.x, c9.x
rcp r2.w, r0.w
max r3.x, r0.y, c9
min r0.x, r0, r0.z
add r0.x, -r3, r0
mul r3.w, r0.x, c9
add r4.x, r3, r3.w
mad r0.xyz, r1, r4.x, r2
dp3 r0.x, r0, r0
rsq r0.x, r0.x
rcp r0.y, r0.x
mov r0.x, c4
add r0.x, c9.y, -r0
rcp r1.w, r0.x
mad r0.y, -r0, r2.w, c9
mul_sat r4.z, r1.w, r0.y
pow r0, c8.z, r3.w
mad r3.xyz, r1, r3.x, r2
dp3 r0.y, r3, r3
mov r0.z, r0.x
rsq r0.x, r0.y
rcp r0.y, r0.z
add r0.y, -r0, c9
mul r0.w, r0.y, r4.y
rcp r0.x, r0.x
mad r0.x, -r0, r2.w, c9.y
mul_sat r0.x, r1.w, r0
mul_sat r0.x, r0, r0.w
mul_sat r0.y, r0.w, r4.z
add r0.x, -r0, c9.y
add r0.y, -r0, c9
mul r3.x, r0, r0.y
add r3.y, r3.w, r4.x
mad r0.xyz, r1, r3.y, r2
dp3 r3.z, r0, r0
add r3.y, r3.w, r3
mad r0.xyz, r1, r3.y, r2
dp3 r0.y, r0, r0
rsq r3.z, r3.z
rcp r0.x, r3.z
rsq r0.y, r0.y
mad r0.x, -r0, r2.w, c9.y
mul_sat r0.x, r1.w, r0
rcp r0.y, r0.y
mad r0.y, -r0, r2.w, c9
mul_sat r0.y, r1.w, r0
mul_sat r0.x, r0.w, r0
add r0.x, -r0, c9.y
mul_sat r0.y, r0.w, r0
mul r0.x, r3, r0
add r0.y, -r0, c9
mul r3.x, r0, r0.y
add r3.y, r3.w, r3
mad r0.xyz, r1, r3.y, r2
dp3 r3.z, r0, r0
add r3.y, r3.w, r3
mad r0.xyz, r1, r3.y, r2
dp3 r0.y, r0, r0
rsq r3.z, r3.z
rcp r0.x, r3.z
rsq r0.y, r0.y
mad r0.x, -r0, r2.w, c9.y
mul_sat r0.x, r1.w, r0
rcp r0.y, r0.y
mad r0.y, -r0, r2.w, c9
mul_sat r0.y, r1.w, r0
mul_sat r0.x, r0.w, r0
add r0.x, -r0, c9.y
mul_sat r0.y, r0.w, r0
mul r0.x, r3, r0
add r0.y, -r0, c9
mul r3.x, r0, r0.y
add r3.y, r3.w, r3
mad r0.xyz, r1, r3.y, r2
dp3 r3.z, r0, r0
add r3.y, r3.w, r3
mad r0.xyz, r1, r3.y, r2
dp3 r0.y, r0, r0
add r0.z, r3.w, r3.y
rsq r3.z, r3.z
rcp r0.x, r3.z
rsq r0.y, r0.y
mad r0.x, -r0, r2.w, c9.y
mul_sat r0.x, r1.w, r0
rcp r0.y, r0.y
mad r0.y, -r0, r2.w, c9
mul_sat r0.y, r1.w, r0
mul_sat r0.x, r0.w, r0
add r0.x, -r0, c9.y
mul_sat r0.y, r0.w, r0
mul r0.x, r3, r0
add r0.y, -r0, c9
mul r3.x, r0, r0.y
add r3.y, r3.w, r0.z
mad r0.xyz, r1, r0.z, r2
dp3 r0.x, r0, r0
mad r1.xyz, r1, r3.y, r2
dp3 r0.y, r1, r1
rsq r0.x, r0.x
rsq r0.y, r0.y
rcp r0.x, r0.x
rcp r0.y, r0.y
mad r0.x, -r0, r2.w, c9.y
mul_sat r0.x, r1.w, r0
mad r0.y, -r0, r2.w, c9
mul_sat r0.y, r1.w, r0
mul_sat r0.x, r0.w, r0
mul_sat r0.y, r0.w, r0
add r0.x, -r0, c9.y
add r0.y, -r0, c9
mul r0.x, r3, r0
mad r1.w, -r0.x, r0.y, c9.y
endif
pow r0, r1.w, c6.x
mov_pp r0.y, c3.w
add_pp r0.y, -c2.w, r0
mov_pp r1.xyz, c3
mad_pp r0.y, r0.x, r0, c2.w
add_pp r1.xyz, -c2, r1
mul oC0.w, r1, r0.y
mad_pp oC0.xyz, r0.x, r1, c2
"
}

SubProgram "d3d11 " {
Keywords { }
ConstBuffer "$Globals" 80 // 80 used size, 7 vars
Vector 16 [_FogBaseColor] 4
Vector 32 [_FogDenseColor] 4
Float 48 [_InnerRatio]
Float 52 [_Density]
Float 56 [_ColorFalloff]
Vector 64 [FogParam] 4
ConstBuffer "UnityPerCamera" 128 // 128 used size, 8 vars
Vector 64 [_WorldSpaceCameraPos] 3
Vector 112 [_ZBufferParams] 4
BindCB "$Globals" 0
BindCB "UnityPerCamera" 1
SetTexture 0 [_CameraDepthTexture] 2D 0
// 68 instructions, 5 temp regs, 0 temp arrays:
// ALU 54 float, 2 int, 0 uint
// TEX 1 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 2 static, 2 dynamic
"ps_4_0
eefiecedfpkmpoffhpkdedlbifhghkkeakcglbbbabaaaaaameaiaaaaadaaaaaa
cmaaaaaajmaaaaaanaaaaaaaejfdeheogiaaaaaaadaaaaaaaiaaaaaafaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaafmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaafmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apalaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcomahaaaaeaaaaaaaplabaaaa
fjaaaaaeegiocaaaaaaaaaaaafaaaaaafjaaaaaeegiocaaaabaaaaaaaiaaaaaa
fkaaaaadaagabaaaaaaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaagcbaaaad
hcbabaaaabaaaaaagcbaaaadlcbabaaaacaaaaaagfaaaaadpccabaaaaaaaaaaa
giaaaaacafaaaaaaaoaaaaahdcaabaaaaaaaaaaaegbabaaaacaaaaaapgbpbaaa
acaaaaaaefaaaaajpcaabaaaaaaaaaaaegaabaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaabaaaaaahccaabaaaaaaaaaaaegbcbaaaabaaaaaaegbcbaaa
abaaaaaaeeaaaaafccaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahocaabaaa
aaaaaaaafgafbaaaaaaaaaaaagbjbaaaabaaaaaadiaaaaalpcaabaaaabaaaaaa
egiocaaaaaaaaaaaaeaaaaaaaceaaaaagpbciddkgpbciddkgpbciddkgpbciddk
dcaaaaaohcaabaaaabaaaaaaegiccaaaabaaaaaaaeaaaaaaaceaaaaagpbciddk
gpbciddkgpbciddkaaaaaaaaegacbaiaebaaaaaaabaaaaaabaaaaaahbcaabaaa
acaaaaaajgahbaaaaaaaaaaajgahbaaaaaaaaaaabaaaaaahccaabaaaacaaaaaa
jgahbaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaahecaabaaaacaaaaaabkaabaaa
acaaaaaabkaabaaaacaaaaaabaaaaaahicaabaaaacaaaaaaegacbaaaabaaaaaa
egacbaaaabaaaaaadcaaaaakicaabaaaacaaaaaadkaabaiaebaaaaaaabaaaaaa
dkaabaaaabaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaaakaabaaa
acaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaa
abeaaaaaaaaaiaeadcaaaaakecaabaaaacaaaaaackaabaaaacaaaaaackaabaaa
acaaaaaadkaabaiaebaaaaaaacaaaaaadbaaaaahicaabaaaacaaaaaaabeaaaaa
aaaaaaaackaabaaaacaaaaaabpaaaeaddkaabaaaacaaaaaadcaaaaalbcaabaaa
aaaaaaaackiacaaaabaaaaaaahaaaaaaakaabaaaaaaaaaaadkiacaaaabaaaaaa
ahaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpakaabaaaaaaaaaaadiaaaaaiicaabaaaacaaaaaabkiacaaaaaaaaaaa
adaaaaaaabeaaaaaaaaahkeediaaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaa
abeaaaaagpbciddkaoaaaaahbcaabaaaacaaaaaaabeaaaaaaaaaaadpakaabaaa
acaaaaaaelaaaaafecaabaaaacaaaaaackaabaaaacaaaaaadcaaaaalbcaabaaa
adaaaaaabkaabaiaebaaaaaaacaaaaaaabeaaaaaaaaaaaeackaabaiaebaaaaaa
acaaaaaadiaaaaahbcaabaaaadaaaaaaakaabaaaacaaaaaaakaabaaaadaaaaaa
deaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaaaadcaaaaak
ccaabaaaacaaaaaabkaabaiaebaaaaaaacaaaaaaabeaaaaaaaaaaaeackaabaaa
acaaaaaadiaaaaahbcaabaaaacaaaaaaakaabaaaacaaaaaabkaabaaaacaaaaaa
deaaaaahbcaabaaaacaaaaaaakaabaaaacaaaaaaabeaaaaaaaaaaaaaddaaaaah
bcaabaaaaaaaaaaaakaabaaaaaaaaaaaakaabaaaacaaaaaaaaaaaaaibcaabaaa
aaaaaaaaakaabaiaebaaaaaaadaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaa
acaaaaaaakaabaaaaaaaaaaaabeaaaaamnmmmmdnbjaaaaafbcaabaaaacaaaaaa
akaabaaaacaaaaaaaoaaaaakbcaabaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpakaabaaaacaaaaaaaaaaaaaibcaabaaaacaaaaaaakaabaia
ebaaaaaaacaaaaaaabeaaaaaaaaaiadpdiaaaaahbcaabaaaacaaaaaadkaabaaa
acaaaaaaakaabaaaacaaaaaaaaaaaaajccaabaaaacaaaaaaakiacaiaebaaaaaa
aaaaaaaaadaaaaaaabeaaaaaaaaaiadpaoaaaaakccaabaaaacaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaacaaaaaadgaaaaafecaabaaa
acaaaaaaakaabaaaadaaaaaadgaaaaaficaabaaaacaaaaaaabeaaaaaaaaaiadp
dgaaaaafccaabaaaadaaaaaaabeaaaaaaaaaaaaadaaaaaabcbaaaaahecaabaaa
adaaaaaabkaabaaaadaaaaaaabeaaaaaakaaaaaaadaaaeadckaabaaaadaaaaaa
dcaaaaajhcaabaaaaeaaaaaajgahbaaaaaaaaaaakgakbaaaacaaaaaaegacbaaa
abaaaaaabaaaaaahecaabaaaadaaaaaaegacbaaaaeaaaaaaegacbaaaaeaaaaaa
elaaaaafecaabaaaadaaaaaackaabaaaadaaaaaaaoaaaaahecaabaaaadaaaaaa
ckaabaaaadaaaaaadkaabaaaabaaaaaaaaaaaaaiecaabaaaadaaaaaackaabaia
ebaaaaaaadaaaaaaabeaaaaaaaaaiadpdicaaaahecaabaaaadaaaaaabkaabaaa
acaaaaaackaabaaaadaaaaaadicaaaahecaabaaaadaaaaaaakaabaaaacaaaaaa
ckaabaaaadaaaaaaaaaaaaaiecaabaaaadaaaaaackaabaiaebaaaaaaadaaaaaa
abeaaaaaaaaaiadpdiaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaackaabaaa
adaaaaaadcaaaaajecaabaaaacaaaaaaakaabaaaaaaaaaaaabeaaaaamnmmmmdn
ckaabaaaacaaaaaaboaaaaahccaabaaaadaaaaaabkaabaaaadaaaaaaabeaaaaa
abaaaaaabgaaaaabaaaaaaaibcaabaaaaaaaaaaadkaabaiaebaaaaaaacaaaaaa
abeaaaaaaaaaiadpbcaaaaabdgaaaaafbcaabaaaaaaaaaaaabeaaaaaaaaaaaaa
bfaaaaabcpaaaaafccaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaaiccaabaaa
aaaaaaaabkaabaaaaaaaaaaackiacaaaaaaaaaaaadaaaaaabjaaaaafccaabaaa
aaaaaaaabkaabaaaaaaaaaaaaaaaaaakpcaabaaaabaaaaaaegiocaiaebaaaaaa
aaaaaaaaabaaaaaaegiocaaaaaaaaaaaacaaaaaadcaaaaakhccabaaaaaaaaaaa
fgafbaaaaaaaaaaaegacbaaaabaaaaaaegiccaaaaaaaaaaaabaaaaaadcaaaaak
ccaabaaaaaaaaaaabkaabaaaaaaaaaaadkaabaaaabaaaaaadkiacaaaaaaaaaaa
abaaaaaadiaaaaahiccabaaaaaaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaa
doaaaaab"
}

SubProgram "gles " {
Keywords { }
"!!GLES"
}

SubProgram "glesdesktop " {
Keywords { }
"!!GLES"
}

SubProgram "gles3 " {
Keywords { }
"!!GLES3"
}

}

#LINE 133

			}
		}
	}
	Fallback "VertexLit"
}
