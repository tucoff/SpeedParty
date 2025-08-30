Shader "Custom/PlayerColorSwapShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ShirtColor ("Shirt Color", Color) = (1, 0, 0, 1)
        _PantsColor ("Pants Color", Color) = (0, 1, 0, 1)
        _HatColor ("Hat Color", Color) = (0, 0, 1, 1)
        _SkinColor ("Skin Color", Color) = (1, 1, 0, 1)
        _ColorTolerance ("Color Tolerance", Float) = 0.1
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
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _ShirtColor;
            fixed4 _PantsColor;
            fixed4 _HatColor;
            fixed4 _SkinColor;
            float _ColorTolerance;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Define as cores originais que queremos substituir
                fixed3 originalShirtColor = fixed3(1.0, 0.0, 0.0); // Vermelho
                fixed3 originalPantsColor = fixed3(0.0, 1.0, 0.0); // Verde
                fixed3 originalHatColor = fixed3(0.0, 0.0, 1.0);   // Azul
                fixed3 originalSkinColor = fixed3(1.0, 1.0, 0.0);  // Amarelo
                
                // Calcula a distância da cor atual para cada cor original
                float shirtDistance = distance(c.rgb, originalShirtColor);
                float pantsDistance = distance(c.rgb, originalPantsColor);
                float hatDistance = distance(c.rgb, originalHatColor);
                float skinDistance = distance(c.rgb, originalSkinColor);
                
                // Encontra a menor distância para determinar qual cor substituir
                float minDistance = min(min(shirtDistance, pantsDistance), min(hatDistance, skinDistance));
                
                // Substitui a cor apenas se a menor distância estiver dentro da tolerância
                if (minDistance < _ColorTolerance)
                {
                    if (shirtDistance == minDistance)
                    {
                        c.rgb = _ShirtColor.rgb;
                    }
                    else if (pantsDistance == minDistance)
                    {
                        c.rgb = _PantsColor.rgb;
                    }
                    else if (hatDistance == minDistance)
                    {
                        c.rgb = _HatColor.rgb;
                    }
                    else if (skinDistance == minDistance)
                    {
                        c.rgb = _SkinColor.rgb;
                    }
                }
                
                // Aplica alpha premultiplicado
                c.rgb *= c.a;
                
                return c;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
