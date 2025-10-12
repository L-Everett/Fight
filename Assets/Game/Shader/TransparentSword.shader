Shader "Custom/TransparentSword"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,0.5)
        _BaseMap("Base Map", 2D) = "white" {}
        _Smoothness("Smoothness", Range(0,1)) = 0.8
        _FresnelPower("Fresnel Power", Range(0,5)) = 2.0
        _RefractionIndex("Refraction Index", Range(1,2)) = 1.3
    }

    SubShader
    {
        Tags 
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                float fogFactor : TEXCOORD4;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Smoothness;
                float _FresnelPower;
                float _RefractionIndex;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.viewDirWS = GetWorldSpaceNormalizeViewDir(o.positionWS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // 基础纹理和颜色
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                half3 albedo = baseMap.rgb * _BaseColor.rgb;
                
                // 菲涅尔效应 (边缘发光)
                float fresnel = pow(1.0 - saturate(dot(i.normalWS, i.viewDirWS)), _FresnelPower);
                half3 emission = fresnel * _BaseColor.rgb * 2.0;
                
                // 简单折射模拟 (扭曲UV)
                float2 refractedUV = i.uv + (i.normalWS.xy * (1 - _RefractionIndex) * 0.1);
                half3 refractedColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, refractedUV).rgb;
                
                // 光照计算
                Light mainLight = GetMainLight();
                half3 diffuse = LightingLambert(albedo, mainLight.direction, i.normalWS);
                
                // 组合最终颜色
                half3 finalColor = lerp(albedo, refractedColor, 0.3) + emission;
                finalColor = MixFog(finalColor, i.fogFactor);
                
                return half4(finalColor, _BaseColor.a);
            }
            ENDHLSL
        }
    }
}