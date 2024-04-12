Shader "LD CrewBoom/Character"
{
    Properties
    {
        [HideInInspector] [KeywordEnum(Opaque, Cutout)] _Transparency ("Transparency", Float) = 0
        [HideInInspector] _CutOut("Alpha Cutout", Range(0,1)) = 0.1

        [HideInInspector] [KeywordEnum(UV0, UV1)] _MainTexUV ("UV Map", Float) = 0
        [HideInInspector] [KeywordEnum(UV0, UV1)] _EmissionUV ("UV Map", Float) = 0
        [HideInInspector] [Toggle] _MainTexScroll ("Scroll", Float) = 0
        [HideInInspector] [Toggle] _EmissionScroll ("Scroll", Float) = 0

        [HideInInspector] _MainTexUSpeed ("U Speed", Float) = 0
        [HideInInspector] _MainTexVSpeed ("V Speed", Float) = 0

        [HideInInspector] _EmissionUSpeed ("U Speed", Float) = 0
        [HideInInspector] _EmissionVSpeed ("V Speed", Float) = 0

        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _Emission ("Emission", 2D) = "black" {}
        [Toggle] _Outline ("Outline", Float) = 1
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineMultiplier("Outline Multiplier", float) = 0.005
        _MinOutlineSize("Min Outline Multiplier", float) = 0.002
        _MaxOutlineSize("Max Outline Multiplier", float) = 0.008
    }
    SubShader
    {
        Tags { "LightMode"="ForwardBase" }
        LOD 100

        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma shader_feature _OUTLINE_ON
            #pragma shader_feature _TRANSPARENCY_OPAQUE _TRANSPARENCY_CUTOUT
            #pragma shader_feature _MAINTEXSCROLL_ON
            #pragma shader_feature _MAINTEXUV_UV0 _MAINTEXUV_UV1
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            fixed4 _OutlineColor;
            float _OutlineMultiplier;
            float _MinOutlineSize;
            float _MaxOutlineSize;

            #if _TRANSPARENCY_CUTOUT
            #if _MAINTEXSCROLL_ON
            float _MainTexUSpeed;
            float _MainTexVSpeed;
            #endif
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _CutOut;
            float4 _Color;
            #endif

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                #if _TRANSPARENCY_CUTOUT
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
                #endif
            };
            struct v2f
            {
                float4 clipPos : SV_POSITION;
                #if _TRANSPARENCY_CUTOUT
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
                #endif
            };
            v2f vert(appdata v)
            {
                v2f o;
                float4 clipPos = UnityObjectToClipPos(v.vertex);
                float outlineMultiplier = clamp(clipPos.w * _OutlineMultiplier, _MinOutlineSize, _MaxOutlineSize);
                o.clipPos = UnityObjectToClipPos(v.vertex + (v.normal * outlineMultiplier));
                #if _OUTLINE_ON
                #if _TRANSPARENCY_CUTOUT
                #if _MAINTEXUV_UV0
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                #endif
                #if _MAINTEXUV_UV1
                o.uv = TRANSFORM_TEX(v.uv1, _MainTex);
                #endif

                #if _MAINTEXSCROLL_ON
                o.uv += float2(_MainTexUSpeed, _MainTexVSpeed) * _Time;
                #endif
                o.color = v.color * _Color;
                #endif
                #endif
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                #if _OUTLINE_ON
                #if _TRANSPARENCY_CUTOUT
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                clip(col.a - _CutOut);
                #endif
                #else
                clip(-1);
                #endif
                return _OutlineColor;
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma shader_feature _TRANSPARENCY_OPAQUE _TRANSPARENCY_CUTOUT
            #pragma shader_feature _MAINTEXSCROLL_ON
            #pragma shader_feature _EMISSIONSCROLL_ON
            #pragma shader_feature _MAINTEXUV_UV0 _MAINTEXUV_UV1
            #pragma shader_feature _EMISSIONUV_UV0 _EMISSIONUV_UV1
            #pragma vertex vert
            #pragma fragment frag
            #include "BRCCommon.cginc"
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float3 normal : NORMAL;
                float4 color : COLOR0;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD2;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float4 color : COLOR0;
            };
            float4 LightColor;
            float4 ShadowColor;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Emission;
            float4 _Emission_ST;
            float4 _Color;

            #if _MAINTEXSCROLL_ON
            float _MainTexUSpeed;
            float _MainTexVSpeed;
            #endif

            #if _EMISSIONSCROLL_ON
            float _EmissionUSpeed;
            float _EmissionVSpeed;
            #endif

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                #if _MAINTEXUV_UV0
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                #endif
                #if _MAINTEXUV_UV1
                o.uv = TRANSFORM_TEX(v.uv1, _MainTex);
                #endif

                #if _EMISSIONUV_UV0
                o.uv2 = TRANSFORM_TEX(v.uv, _Emission);
                #endif
                #if _EMISSIONUV_UV1
                o.uv2 = TRANSFORM_TEX(v.uv1, _Emission);
                #endif

                #if _MAINTEXSCROLL_ON
                o.uv += float2(_MainTexUSpeed, _MainTexVSpeed) * _Time;
                #endif
                #if _EMISSIONSCROLL_ON
                o.uv2 += float2(_EmissionUSpeed, _EmissionVSpeed) * _Time;
                #endif

                o.normal = UnityObjectToWorldNormal(v.normal);
                o.color = v.color * _Color;
                return o;
            }

            #if _TRANSPARENCY_CUTOUT
            float _CutOut;
            #endif

            fixed4 frag(v2f i) : SV_Target
            {
                fixed lighting = saturate(dot(i.normal, _WorldSpaceLightPos0) * LIGHT_MULTIPLY);
                float4 lightColor = lerp(ShadowColor, LightColor, lighting);
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                //col.a = 1.0;
                col.rgb *= lightColor * _LightColor0.rgb;
                fixed3 emissionCol = tex2D(_Emission, i.uv2).rgb;
                col.rgb += emissionCol.rgb;
                #if _TRANSPARENCY_CUTOUT
                clip(col.a - _CutOut);
                #endif
                return col;
            }
            ENDCG
        }
        Pass
        {
            Tags {"LightMode" = "ShadowCaster"}

            CGPROGRAM
            #pragma shader_feature _TRANSPARENCY_OPAQUE _TRANSPARENCY_CUTOUT
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster

            #include "UnityCG.cginc"

                struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f {
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.uv = v.uv;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            sampler2D _MainTex;
            float _CutOut;

            float4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                #if _TRANSPARENCY_CUTOUT
                    clip(col.a - _CutOut);
                #endif
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    CustomEditor "CharacterMaterialEditor"
}
