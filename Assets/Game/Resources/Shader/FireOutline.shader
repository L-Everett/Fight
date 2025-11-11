Shader "UI/FireOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 2
        _NoiseScale ("Noise Scale", Range(0, 0.1)) = 0.05
        _FireSpeed ("Fire Speed", Range(0, 5)) = 1.0
        _FireIntensity ("Fire Intensity", Range(0, 2)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _NoiseScale;
            float _FireSpeed;
            float _FireIntensity;

            // 简单噪声函数
            float noise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 基础纹理采样 - 保持原始清晰度
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // 火焰扰动偏移
                float time = _Time.y * _FireSpeed;
                float2 noiseOffset = float2(
                    noise(i.uv * 10.0 + time) * 2.0 - 1.0,
                    noise(i.uv * 10.0 + time + 10.0) * 2.0 - 1.0
                ) * _NoiseScale;
                
                // 边缘检测
                float outline = 0;
                
                // 4方向采样（性能优化）
                float2 offsets[4] = {
                    float2(0, 1),  // 上
                    float2(0, -1), // 下
                    float2(-1, 0), // 左
                    float2(1, 0)   // 右
                };
                
                for(int idx = 0; idx < 4; idx++)
                {
                    // 只在边缘检测中添加扰动
                    float2 uvOffset = i.uv + (offsets[idx] + noiseOffset) * _OutlineWidth / 1000.0;
                    fixed4 neighbor = tex2D(_MainTex, uvOffset);
                    
                    if (neighbor.a < 0.5 && col.a > 0.5) 
                    {
                        outline = 1;
                        break;
                    }
                }
                
                // 火焰强度波动
                float intensity = (sin(time * 3.0) * 0.5 + 0.5) * _FireIntensity;
                
                // 使用外部传入的描边颜色
                fixed4 fireColor = _OutlineColor;
                
                // 颜色保护 - 确保RGB值不低于0.2
                fireColor.r = max(fireColor.r, 0.2);
                fireColor.g = max(fireColor.g, 0.2);
                fireColor.b = max(fireColor.b, 0.2);
                
                fireColor.rgb *= intensity;
                
                // 混合颜色 - 只对描边部分应用火焰效果
                fixed4 finalColor = col;
                
                if(outline > 0)
                {
                    // 只修改描边部分的颜色
                    finalColor.rgb = fireColor.rgb;
                    
                    // 添加火焰辉光效果
                    finalColor.rgb += fireColor.rgb * (1.0 - col.a) * 0.5;
                }
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    Fallback "UI/Default"
}