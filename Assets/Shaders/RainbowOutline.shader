Shader "TwinsDefense/RainbowOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineThickness ("Outline Thickness (texels)", Range(1, 8)) = 2
        _BandCount ("Rainbow Bands", Range(2, 12)) = 6
        _SpinSpeed ("Spin Speed", Float) = 1.5
        _Brightness ("Brightness", Range(0, 3)) = 1.4
        _Alpha ("Outline Alpha", Range(0, 1)) = 0.9
        [Toggle] _Inward ("Inward (glow inside the sprite instead of an outer ring)", Range(0, 1)) = 0
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
                float2 localPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _OutlineThickness;
            float _BandCount;
            float _SpinSpeed;
            float _Brightness;
            float _Alpha;
            float _Inward;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                // Object-space position (pre-transform), NOT derived from texcoord —
                // see the frag-side comment on why the angle math reads this instead.
                OUT.localPos = IN.vertex.xy;
                return OUT;
            }

            float SampleAlpha(float2 uv)
            {
                return tex2D(_MainTex, uv).a;
            }

            // Pure-hue rainbow (S=1, V=1), then quantized into _BandCount discrete
            // steps so it reads as flat pixel-art color bands instead of a smooth
            // analog gradient — keeps the Don't Starve cel-shaded look.
            float3 HueToRGB(float hue)
            {
                float banded = floor(hue * _BandCount) / _BandCount;
                float r = abs(banded * 6.0 - 3.0) - 1.0;
                float g = 2.0 - abs(banded * 6.0 - 2.0);
                float b = 2.0 - abs(banded * 6.0 - 4.0);
                return saturate(float3(r, g, b));
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy * _OutlineThickness;
                float ownAlpha = SampleAlpha(IN.texcoord);

                // Dilate the silhouette outward by sampling 8 neighbours at
                // outline-thickness distance — max() lets a single covered
                // direction count, so the ring stays an even thickness on curves.
                float dilated = 0.0;
                dilated = max(dilated, SampleAlpha(IN.texcoord + float2( texel.x, 0)));
                dilated = max(dilated, SampleAlpha(IN.texcoord + float2(-texel.x, 0)));
                dilated = max(dilated, SampleAlpha(IN.texcoord + float2(0,  texel.y)));
                dilated = max(dilated, SampleAlpha(IN.texcoord + float2(0, -texel.y)));
                dilated = max(dilated, SampleAlpha(IN.texcoord + float2( texel.x,  texel.y)));
                dilated = max(dilated, SampleAlpha(IN.texcoord + float2(-texel.x,  texel.y)));
                dilated = max(dilated, SampleAlpha(IN.texcoord + float2( texel.x, -texel.y)));
                dilated = max(dilated, SampleAlpha(IN.texcoord + float2(-texel.x, -texel.y)));

                // Extra 1-texel reach straight up and straight down, on top of the
                // uniform ring above — the top/bottom edges (head, feet) read thin
                // next to the sides otherwise, so bias 1 more pixel of thickness
                // vertically only.
                float2 extraVertical = texel + _MainTex_TexelSize.xy;
                dilated = max(dilated, SampleAlpha(IN.texcoord + float2(0,  extraVertical.y)));
                dilated = max(dilated, SampleAlpha(IN.texcoord + float2(0, -extraVertical.y)));

                // Mirror of the dilate above, but eroding inward (min() instead of
                // max()) — shrinks the silhouette instead of growing it, so the
                // band this produces traces the INSIDE edge of the sprite.
                //
                // Deliberately does NOT sample the extra vertical reach used above:
                // that bias exists to thicken a ring that reads thin on the OUTSIDE
                // of curved top/bottom edges (head, feet). Eroding that far inward
                // at the same spot instead eats past the top of the head entirely
                // (there's rarely 3+ opaque texels of headroom above the topmost
                // row), forcing eroded to 0 across the whole cap and punching a
                // hole in the inner ring instead of a thin band.
                float eroded = 1.0;
                eroded = min(eroded, SampleAlpha(IN.texcoord + float2( texel.x, 0)));
                eroded = min(eroded, SampleAlpha(IN.texcoord + float2(-texel.x, 0)));
                eroded = min(eroded, SampleAlpha(IN.texcoord + float2(0,  texel.y)));
                eroded = min(eroded, SampleAlpha(IN.texcoord + float2(0, -texel.y)));
                eroded = min(eroded, SampleAlpha(IN.texcoord + float2( texel.x,  texel.y)));
                eroded = min(eroded, SampleAlpha(IN.texcoord + float2(-texel.x,  texel.y)));
                eroded = min(eroded, SampleAlpha(IN.texcoord + float2( texel.x, -texel.y)));
                eroded = min(eroded, SampleAlpha(IN.texcoord + float2(-texel.x, -texel.y)));

                // Outer ring: covered by the dilated silhouette but not by the
                // sprite's own pixels. Inner ring: covered by the sprite's own
                // pixels but not by the eroded (shrunk) silhouette. _Inward picks
                // which one this material draws.
                float outwardMask = saturate(dilated - ownAlpha);
                float inwardMask = saturate(ownAlpha - eroded);
                float ringMask = lerp(outwardMask, inwardMask, _Inward);
                if (ringMask <= 0.001) discard;

                // Hue travels around the sprite's angle from center, plus a
                // continuous time offset, so the rainbow visibly spins around
                // the outline (the Premier Ball look) instead of sitting still.
                //
                // IMPORTANT: this uses IN.localPos (object-space quad position),
                // NOT IN.texcoord. texcoord is the UV *inside the packed atlas
                // texture* — for an animated character, every frame (idle/walk/
                // attack) sits at a different, tiny sub-rect of _MainTex, so
                // (texcoord - 0.5) pointed at a near-constant, effectively
                // arbitrary direction instead of "around the sprite". That's why
                // the ring rendered as one flat color instead of a rainbow, and
                // why it jumped to a *different* flat color every time the
                // walk-cycle changed frame. localPos is the mesh's own geometry
                // in object space — the same -halfWidth..+halfWidth range on
                // every frame, regardless of where that frame lives in the atlas
                // — so the angle (and the rainbow) stays stable while walking.
                float angle = atan2(IN.localPos.y, IN.localPos.x) / (2.0 * UNITY_PI) + 0.5;
                float hue = frac(angle + _Time.y * _SpinSpeed * 0.1);

                float3 rgb = HueToRGB(hue) * _Brightness;
                return fixed4(rgb, ringMask * _Alpha * IN.color.a);
            }
            ENDCG
        }
    }
}
