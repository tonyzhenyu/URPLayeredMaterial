#ifndef UNIVERSAL_LIT_INPUT_INCLUDED_CUSTOM
#define UNIVERSAL_LIT_INPUT_INCLUDED_CUSTOM

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "MySurfaceData.hlsl"
#include "MyLitSurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
#include "MyPBRMask.hlsl"


#if defined(_DETAIL_MULX2) || defined(_DETAIL_SCALED)
#define _DETAIL
#endif

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _DetailAlbedoMap_ST;
float4 _BaseMap01_ST;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _BumpScale;
half _Parallax;
half _OcclusionStrength;
half _ClearCoatMask;
half _ClearCoatSmoothness;
half _DetailAlbedoMapScale;
half _DetailNormalMapScale;
half _Surface;
CBUFFER_END


TEXTURE2D(_DetailMask);         SAMPLER(sampler_DetailMask);
TEXTURE2D(_DetailAlbedoMap);    SAMPLER(sampler_DetailAlbedoMap);
TEXTURE2D(_DetailNormalMap);    SAMPLER(sampler_DetailNormalMap);
TEXTURE2D(_DetailPBRMaskMap);   SAMPLER(sampler_DetailPBRMaskMap);

TEXTURE2D(_ClearCoatMap);       SAMPLER(sampler_ClearCoatMap);


#ifdef _SPECULAR_SETUP
//#define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
#else
//#define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
#endif

half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
    half4 specGloss;

#ifdef _METALLICSPECGLOSSMAP
    specGloss = half4(SAMPLE_METALLICSPECULAR(uv));
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        specGloss.a = albedoAlpha * _Smoothness;
    #else
        specGloss.a *= _Smoothness;
    #endif
#else // _METALLICSPECGLOSSMAP
    #if _SPECULAR_SETUP
        specGloss.rgb = _SpecColor.rgb;
    #else
        specGloss.rgb = _Metallic.rrr;
    #endif

    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        specGloss.a = albedoAlpha * _Smoothness;
    #else
        specGloss.a = _Smoothness;
    #endif
#endif

    return specGloss;
}

half SampleOcclusion(float2 uv)
{
#ifdef _OCCLUSIONMAP
    // TODO: Controls things like these by exposing SHADER_QUALITY levels (low, medium, high)
#if defined(SHADER_API_GLES)
    return SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
#else
    half occ = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
    return LerpWhiteTo(occ, _OcclusionStrength);
#endif
#else
    return half(1.0);
#endif
}


// Returns clear coat parameters
// .x/.r == mask
// .y/.g == smoothness
half2 SampleClearCoat(float2 uv)
{
#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half2 clearCoatMaskSmoothness = half2(_ClearCoatMask, _ClearCoatSmoothness);

#if defined(_CLEARCOATMAP)
    clearCoatMaskSmoothness *= SAMPLE_TEXTURE2D(_ClearCoatMap, sampler_ClearCoatMap, uv).rg;
#endif

    return clearCoatMaskSmoothness;
#else
    return half2(0.0, 1.0);
#endif  // _CLEARCOAT
}

void ApplyPerPixelDisplacement(half3 viewDirTS, inout float2 uv)
{
#if defined(_PARALLAXMAP)
    uv += ParallaxMapping(TEXTURE2D_ARGS(_ParallaxMap, sampler_ParallaxMap), viewDirTS, _Parallax, uv);
#endif
}

// Used for scaling detail albedo. Main features:
// - Depending if detailAlbedo brightens or darkens, scale magnifies effect.
// - No effect is applied if detailAlbedo is 0.5.
half3 ScaleDetailAlbedo(half3 detailAlbedo, half scale)
{
    // detailAlbedo = detailAlbedo * 2.0h - 1.0h;
    // detailAlbedo *= _DetailAlbedoMapScale;
    // detailAlbedo = detailAlbedo * 0.5h + 0.5h;
    // return detailAlbedo * 2.0f;

    // A bit more optimized
    return half(2.0) * detailAlbedo * scale - scale + half(1.0);
}

half3 ApplyDetailAlbedo(float2 detailUv, half3 albedo, half detailMask)
{
#if defined(_DETAIL)
    half3 detailAlbedo = SAMPLE_TEXTURE2D(_DetailAlbedoMap, sampler_DetailAlbedoMap, detailUv).rgb;

    // In order to have same performance as builtin, we do scaling only if scale is not 1.0 (Scaled version has 6 additional instructions)
#if defined(_DETAIL_SCALED)
    detailAlbedo = ScaleDetailAlbedo(detailAlbedo, _DetailAlbedoMapScale);
#else
    detailAlbedo = half(2.0) * detailAlbedo;
#endif

    return albedo * LerpWhiteTo(detailAlbedo, detailMask);
#else
    return albedo;
#endif
}

half3 ApplyDetailNormal(float2 detailUv, half3 normalTS, half detailMask,TEXTURE2D(normalmap),SAMPLER(sampler_normalmap))
{
#if defined(_DETAIL)
#if BUMP_SCALE_NOT_SUPPORTED
    half3 detailNormalTS = UnpackNormal(SAMPLE_TEXTURE2D(normalmap, sampler_normalmap, detailUv));
#else
    half3 detailNormalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(normalmap, sampler_normalmap, detailUv), _DetailNormalMapScale);
#endif

    // With UNITY_NO_DXT5nm unpacked vector is not normalized for BlendNormalRNM
    // For visual consistancy we going to do in all cases
    detailNormalTS = normalize(detailNormalTS);

    return lerp(normalTS, BlendNormalRNM(normalTS, detailNormalTS), detailMask); // todo: detailMask should lerp the angle of the quaternion rotation, not the normals
#else
    return normalTS;
#endif
}

    inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
    {
        half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
        outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

        half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
        outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

        // #if _SPECULAR_SETUP
        //     outSurfaceData.metallic = half(1.0);
        //     outSurfaceData.specular = specGloss.rgb;
        // #else
        //     outSurfaceData.metallic = specGloss.r;
        //     outSurfaceData.specular = half3(0.0, 0.0, 0.0);
        // #endif

        outSurfaceData = InitPBRmaskMap(uv , outSurfaceData , _Metallic , _Smoothness,_PBRMaskMap,sampler_PBRMaskMap);
        // outSurfaceData.smoothness = specGloss.a;

        outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
        outSurfaceData.occlusion = SampleOcclusion(uv);
        outSurfaceData.emission = 0;
        //outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

        #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
            half2 clearCoat = SampleClearCoat(uv);
            outSurfaceData.clearCoatMask = clearCoat.r;
            outSurfaceData.clearCoatSmoothness = clearCoat.g;
        #else
            outSurfaceData.clearCoatMask = half(0.0);
            outSurfaceData.clearCoatSmoothness = half(0.0);
        #endif

        #if defined(_DETAIL)
            half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
            float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
            outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
            outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask,_DetailNormalMap,sampler_DetailNormalMap);

            half4 detailPBRMap = SAMPLE_TEXTURE2D(_DetailPBRMaskMap, sampler_DetailPBRMaskMap, detailUv);

            outSurfaceData.smoothness = lerp(0,detailPBRMap.r,outSurfaceData.smoothness);
            outSurfaceData.metallic = detailPBRMap.g * outSurfaceData.metallic;

        #endif

        half height = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv).r;
        outSurfaceData.height = height;
    }

    #if _LAYER
        inline void InitializeStandardLitSurfaceData01(float2 uv, out SurfaceData outSurfaceData)
        {
            uv*= _BaseMap01_ST.xy;
            uv+= _BaseMap01_ST.zw;
            half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap01, sampler_BaseMap01));
            outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor01, _Cutoff);

            half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
            outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor01.rgb;

            // #if _SPECULAR_SETUP
            //     outSurfaceData.metallic = half(1.0);
            //     outSurfaceData.specular = specGloss.rgb;
            // #else
            //     outSurfaceData.metallic = specGloss.r;
            //     outSurfaceData.specular = half3(0.0, 0.0, 0.0);
            // #endif

            outSurfaceData = InitPBRmaskMap(uv , outSurfaceData , _Metallic01 , _Smoothness01,_PBRMaskMap01,sampler_PBRMaskMap01);
            // outSurfaceData.smoothness = specGloss.a;
            
            outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap01, sampler_BumpMap01), _BumpScale01);
            outSurfaceData.occlusion = SampleOcclusion(uv);
            outSurfaceData.emission = 0;
            //outSurfaceData.emission = SampleEmission(uv, _EmissionColor01.rgb, TEXTURE2D_ARGS(_EmissionMap01, sampler_EmissionMap01));

            #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
                half2 clearCoat = SampleClearCoat(uv);
                outSurfaceData.clearCoatMask = clearCoat.r;
                outSurfaceData.clearCoatSmoothness = clearCoat.g;
            #else
                outSurfaceData.clearCoatMask = half(0.0);
                outSurfaceData.clearCoatSmoothness = half(0.0);
            #endif

            // #if defined(_DETAIL)
            //     half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
            //     float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
            //     outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
            //     outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask,_DetailNormalMap01,sampler_DetailNormalMap01);

            //     //--detail pbr
            //     half4 detailPBRMap01 = SAMPLE_TEXTURE2D(_DetailPBRMaskMap01, sampler_DetailPBRMaskMap01, detailUv).a;

            //     outSurfaceData.smoothness = lerp(detailPBRMap01.r ,0, outSurfaceData.smoothness);
            //     outSurfaceData.metallic = detailPBRMap01.g * outSurfaceData.metallic;

            // #endif

            half height = SAMPLE_TEXTURE2D(_HeightMap01, sampler_HeightMap01, uv).r;
            outSurfaceData.height = height;
        }
    #endif
#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
