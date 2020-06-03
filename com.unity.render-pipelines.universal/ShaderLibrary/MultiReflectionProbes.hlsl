#ifndef UNIVERSAL_MULTI_REFLECTION_PROBES_INCLUDED
#define UNIVERSAL_MULTI_REFLECTION_PROBES_INCLUDED

struct ReflectionProbeData{
    float3 center;
    float3 extends;
    float box;
    float timePeriodIndex;
};

StructuredBuffer<ReflectionProbeData> _ReflectionProbeBoundsBuffer;

float _CustomReflectionProbeIndex;

void ApplyMultiReflectionProbeData(){
    ReflectionProbeData data = _ReflectionProbeBoundsBuffer[_CustomReflectionProbeIndex];
    float3 probeCenter = data.center;
    float3 probeExtends = data.extends;
    
    
    unity_SpecCube0_BoxMin.xyz = probeCenter - probeExtends;
    unity_SpecCube0_BoxMax.xyz = probeCenter + probeExtends;
    unity_SpecCube0_ProbePosition.xyz = probeCenter;
    unity_SpecCube0_ProbePosition.w = data.box;
}

#endif
