Shader "Custom/VFX_ScrollMask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ScrollSpeed ("Scroll Speed (X, Y)", Vector) = (0, 1, 0, 0)
        _Feather ("Edge Feather (L, R, B, T)", Vector) = (0.05, 0.05, 0.05, 0.05)
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 1
        [Enum(Off,0,On,1)] _ZWrite ("ZWrite", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite [_ZWrite]
        Blend [_SrcBlend] [_DstBlend]

        // URP 2D Pass
        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uvScrolled   : TEXCOORD0;
                float2 uvOrig       : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _ScrollSpeed;
                float4 _Feather;
                float _SrcBlend;
                float _DstBlend;
                float _ZWrite;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color * _Color;
                output.uvOrig = input.uv;
                output.uvScrolled = input.uv + _ScrollSpeed.xy * _Time.y;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uvScrolled) * input.color;

                // Edge feathering mask
                float maskL = smoothstep(0.0, max(0.001, _Feather.x), input.uvOrig.x);
                float maskR = smoothstep(1.0, 1.0 - max(0.001, _Feather.y), input.uvOrig.x);
                float maskB = smoothstep(0.0, max(0.001, _Feather.z), input.uvOrig.y);
                float maskT = smoothstep(1.0, 1.0 - max(0.001, _Feather.w), input.uvOrig.y);
                float edgeMask = maskL * maskR * maskB * maskT;
                col.a *= edgeMask;

                // If additive blend (DstBlend == 1), premultiply alpha
                if (_DstBlend == 1.0)
                {
                    col.rgb *= col.a;
                }

                return col;
            }
            ENDHLSL
        }

        // URP Forward / General Pass
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uvScrolled   : TEXCOORD0;
                float2 uvOrig       : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _ScrollSpeed;
                float4 _Feather;
                float _SrcBlend;
                float _DstBlend;
                float _ZWrite;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color * _Color;
                output.uvOrig = input.uv;
                output.uvScrolled = input.uv + _ScrollSpeed.xy * _Time.y;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uvScrolled) * input.color;

                float maskL = smoothstep(0.0, max(0.001, _Feather.x), input.uvOrig.x);
                float maskR = smoothstep(1.0, 1.0 - max(0.001, _Feather.y), input.uvOrig.x);
                float maskB = smoothstep(0.0, max(0.001, _Feather.z), input.uvOrig.y);
                float maskT = smoothstep(1.0, 1.0 - max(0.001, _Feather.w), input.uvOrig.y);
                float edgeMask = maskL * maskR * maskB * maskT;
                col.a *= edgeMask;

                if (_DstBlend == 1.0)
                {
                    col.rgb *= col.a;
                }

                return col;
            }
            ENDHLSL
        }

        // Built-in Render Pipeline Fallback Pass
        Pass
        {
            Name "BuiltInFallback"
            Tags { "LightMode" = "Always" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex     : SV_POSITION;
                fixed4 color      : COLOR;
                float2 uvScrolled : TEXCOORD0;
                float2 uvOrig     : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ScrollSpeed;
            float4 _Feather;
            float _SrcBlend;
            float _DstBlend;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.color = IN.color * _Color;
                OUT.uvOrig = IN.texcoord;
                OUT.uvScrolled = IN.texcoord + _ScrollSpeed.xy * _Time.y;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, IN.uvScrolled) * IN.color;

                float maskL = smoothstep(0.0, max(0.001, _Feather.x), IN.uvOrig.x);
                float maskR = smoothstep(1.0, 1.0 - max(0.001, _Feather.y), IN.uvOrig.x);
                float maskB = smoothstep(0.0, max(0.001, _Feather.z), IN.uvOrig.y);
                float maskT = smoothstep(1.0, 1.0 - max(0.001, _Feather.w), IN.uvOrig.y);
                float edgeMask = maskL * maskR * maskB * maskT;
                col.a *= edgeMask;

                if (_DstBlend == 1.0)
                {
                    col.rgb *= col.a;
                }

                return col;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
