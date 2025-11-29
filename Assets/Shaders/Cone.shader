Shader "Custom/ConeUnlit"
{
    Properties
    {
        _Color     ("Color",    Color) = (1,1,1,1)
        _Height    ("Height",   Float) = 1.0
        _Radius    ("Radius",   Float) = 1.0
        _EdgeSoft  ("Edge Soft",Float) = 0.15
        _Epsilon   ("Epsilon",  Float) = 0.0001
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Properties
            fixed4 _Color;
            float _Height;
            float _Radius;
            float _EdgeSoft;
            float _Epsilon;

            struct appdata
            {
                float4 vertex : POSITION; // object space
            };

            struct v2f
            {
                float4 posCS : SV_POSITION;
                float3 posOS : TEXCOORD0; // pass object-space position to fragment
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.posCS = UnityObjectToClipPos(v.vertex);
                o.posOS = v.vertex.xyz; // object space
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // object-space coordinates
                float3 pos = i.posOS;

                // radial = sqrt(x^2 + z^2)
                float radial = length(pos.xz);

                // normalized height (clamped)
                float yNorm = saturate(pos.y / _Height);

                // max radius at this height
                float maxR = (1.0 - yNorm) * _Radius;

                // avoid divide-by-zero
                float safeMax = max(maxR, _Epsilon);

                // compute alpha raw
                float radialOver = saturate(radial / safeMax);
                float alphaRaw = 1.0 - radialOver;

                // soft edge
                float alpha = saturate(smoothstep(0.0, _EdgeSoft, alphaRaw));

                // final color (multiply by _Color.a if you want overall opacity)
                fixed3 col = _Color.rgb;
                float outAlpha = alpha * _Color.a;

                return fixed4(col * outAlpha, outAlpha);
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}