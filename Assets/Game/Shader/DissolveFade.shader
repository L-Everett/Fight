Shader "UI/DissolveFade"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        
        // 虚化参数
        _FadeStart("Fade Start", Range(0, 1)) = 0.5
        _FadeEnd("Fade End", Range(0, 1)) = 0.8
        
        // 溶解参数
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _DissolveThreshold("Dissolve Threshold", Range(0, 1)) = 0.5
        _EdgeWidth("Edge Width", Range(0, 0.2)) = 0.05
        _EdgeColor("Edge Color", Color) = (1,1,1,1)
        _Speed("Dissolve Speed", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
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
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            fixed4 _Color;
            float _FadeStart;
            float _FadeEnd;
            float _DissolveThreshold;
            float _EdgeWidth;
            fixed4 _EdgeColor;
            float _Speed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // 底部到顶部虚化效果
                float fadeFactor = smoothstep(_FadeStart, _FadeEnd, IN.texcoord.y);
                col.a *= (1 - fadeFactor);
                
                // 顶部溶解效果
                if (IN.texcoord.y > _FadeEnd)
                {
                    // 采样噪声纹理
                    float2 noiseUV = IN.texcoord + float2(0, _Time.y * _Speed);
                    float noise = tex2D(_NoiseTex, noiseUV).r;
                    
                    // 计算溶解阈值
                    float dissolve = step(noise, _DissolveThreshold);
                    
                    // 边缘发光效果
                    float edge = smoothstep(_DissolveThreshold, _DissolveThreshold + _EdgeWidth, noise);
                    
                    // 应用溶解效果
                    col.a *= (1 - dissolve);
                    
                    // 添加边缘发光
                    col.rgb = lerp(col.rgb, _EdgeColor.rgb, edge);
                }
                
                return col;
            }
            ENDCG
        }
    }
}