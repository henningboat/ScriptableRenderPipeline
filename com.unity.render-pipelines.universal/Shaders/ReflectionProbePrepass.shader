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

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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
                float3 boundsCenter : TEXCOORD2;
                float3 boundsSize : TEXCOORD3;
            };

float4x4 _ViewProjInv;
            StructuredBuffer<float3> _ReflectionProbeBoundsBuffer;
sampler2D _CameraDepthTexture;
            v2f vert (appdata v)
            {
                v2f o;
                
                float3 boundsCenter = _ReflectionProbeBoundsBuffer[v.instance * 2].xyz;
                float3 boundsSize = _ReflectionProbeBoundsBuffer[v.instance * 2 + 1].xyz;
                
                v.vertex.xyz *= boundsSize;
                v.vertex.xyz += boundsCenter;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                o.value = v.instance + 1;
				o.screenUV = ComputeScreenPos( o.vertex);
                
                o.boundsCenter = boundsCenter;
                o.boundsSize = boundsSize;
                
                return o;
            }

            fixed3 frag (v2f i) : SV_Target
            {
                float2 screenUV = i.screenUV.xy/i.screenUV.w;
                
                //from https://forum.unity.com/threads/reconstructing-world-pos-from-depth-imprecision.228936/
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
                float4 H = float4(screenUV.x*2.0-1.0, (screenUV.y)*2.0-1.0, depth, 1.0);
                float4 D = mul(_ViewProjInv,H);
                float3 positionWS = D.xyz/D.w;
                
                float3 absPositionBS = abs(positionWS - i.boundsCenter);
                
                bool isInsideBox = absPositionBS.x < i.boundsSize.x && absPositionBS.y < i.boundsSize.y && absPositionBS.z < i.boundsSize.z;
                
                if(isInsideBox){
                    return i.value / 256;
                }else{
                    return 0;
                }
            }
            ENDCG
        }
    }
}
