#ifndef LAYER_DATA_INCLUDED
#define LAYER_DATA_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    TEXTURE2D(_BaseMap01);              SAMPLER(sampler_BaseMap01);
    TEXTURE2D(_BumpMap01);              SAMPLER(sampler_BumpMap01);
    TEXTURE2D(_PBRMaskMap01);           SAMPLER(sampler_PBRMaskMap01);
    TEXTURE2D(_EmissionMap01);          SAMPLER(sampler_EmissionMap01);
    TEXTURE2D(_HeightMap01);            SAMPLER(sampler_HeightMap01);

    // TEXTURE2D(_DetailAlbedoMap01);      SAMPLER(sampler_DetailAlbedoMap01);
    // TEXTURE2D(_DetailNormalMap01);      SAMPLER(sampler_DetailNormalMap01);
    // TEXTURE2D(_DetailPBRMaskMap01);      SAMPLER(sampler_DetailPBRMaskMap01);

    half4 _BaseColor01;
    half4 _EmissionColor01;
    float _Smoothness01;
    float _Metallic01;
    float _BumpScale01;

    // half _DetailAlbedoMapScale01;
    // half _DetailNormalMapScale01;

#endif
