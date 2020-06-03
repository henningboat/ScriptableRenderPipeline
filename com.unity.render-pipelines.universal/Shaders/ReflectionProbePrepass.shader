Shader "Hidden/ReflectionProbePrepass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Front
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MultiReflectionProbes.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                uint instance : SV_InstanceID;
            };

            struct v2f
            {
                float value : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenUV : TEXCOORD1;
                float3 boundsExtends : TEXCOORD2;
                float3 boundsCenter : TEXCOORD3;
            };

float4x4 _ViewProjInv;
StructuredBuffer<int> _ReflectionProbeIndiceBuffer;
sampler2D _CameraDepthTexture;

            v2f vert (appdata v)
            {
                v2f o;
                
                int reflectionProbeIndex = _ReflectionProbeIndiceBuffer[v.instance + 1];
                ReflectionProbeData data = _ReflectionProbeBoundsBuffer[reflectionProbeIndex];
                
                float3 boundsCenter = data.center;
                float3 boundsExtends = data.extends;
                
                v.vertex.xyz *= boundsExtends * 2;
                v.vertex.xyz += boundsCenter;
                
                o.vertex = UnityWorldToClipPos(v.vertex);
                
                o.value = reflectionProbeIndex;
                
				o.screenUV = ComputeScreenPos( o.vertex);
                
                o.boundsCenter = boundsCenter;
                o.boundsExtends = boundsExtends;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 screenUV = i.screenUV.xy/i.screenUV.w;
                
                //from https://forum.unity.com/threads/reconstructing-world-pos-from-depth-imprecision.228936/
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
                float4 H = float4(screenUV.x*2.0-1.0, (screenUV.y)*2.0-1.0, depth, 1.0);
                float4 D = mul(_ViewProjInv,H);
                float3 positionWS = D.xyz/D.w;
                
                float3 absPositionBS = abs(positionWS - i.boundsCenter);
                
                bool isInsideBox = absPositionBS.x < i.boundsExtends.x && absPositionBS.y < i.boundsExtends.y && absPositionBS.z < i.boundsExtends.z;
                
                if(!isInsideBox){
                    clip(-1);
                }
                
                    return i.value / 256;
                
            }
            ENDCG
        }
    }
}
