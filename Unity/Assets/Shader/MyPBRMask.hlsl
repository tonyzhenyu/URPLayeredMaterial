#ifndef UNIVERSAL_MYPBRMASK_CUSTOM //CUSTOM Lit 
#define UNIVERSAL_MYPBRMASK_CUSTOM

//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

TEXTURE2D(_PBRMaskMap);
SAMPLER(sampler_PBRMaskMap);

///////////////////////////////////////////////////////////////////////////////
//                      Material Property Helpers                            //
///////////////////////////////////////////////////////////////////////////////

half4 SamplePBRMask(float2 uv,TEXTURE2D_PARAM(pbrMaskMap, sampler_pbrMaskMap))
{
    return half4(SAMPLE_TEXTURE2D(pbrMaskMap, sampler_pbrMaskMap, uv));
}

inline SurfaceData InitPBRmaskMap(float2 uv, SurfaceData outSurfaceData, float metallic , float smoothness,TEXTURE2D(map),SAMPLER(sampler_map))
{
    half4 pbrmask = SamplePBRMask(uv,TEXTURE2D_ARGS(map,sampler_map));

    #if _SPECULAR_SETUP
        outSurfaceData.metallic = half(1.0);
        //outSurfaceData.specular = specGloss.rgb; //..todo
    #else
        outSurfaceData.metallic = pbrmask.g * metallic;
        outSurfaceData.specular = half3(0.0, 0.0, 0.0);
    #endif
    
    outSurfaceData.smoothness = lerp(pbrmask.r ,0, smoothness);

    return outSurfaceData;
}

inline SurfaceData ApplyDetailPBR(float2 uv ,SurfaceData outSurfaceData){

    
    return outSurfaceData;
}


#endif