Shader "TwinsDefense/SpriteFlashWhite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
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
        ZWrite Off
        Blend One OneMinusSrcAlpha

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
            fixed4 _Color;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            // Same shape/alpha as the sprite, but fully white - used as a
            // swap-in "hit flash" material since tinting .color toward white
            // does nothing when the sprite's own pixels are already near-white.
            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 texcol = tex2D(_MainTex, IN.texcoord);
                fixed alpha = texcol.a * IN.color.a;

                fixed4 c;
                c.rgb = fixed3(1, 1, 1) * alpha;
                c.a = alpha;
                return c;
            }
            ENDCG
        }
    }
}
