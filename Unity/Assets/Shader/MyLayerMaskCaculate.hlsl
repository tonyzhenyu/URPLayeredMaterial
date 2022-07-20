#ifndef UNIVERSAL_LAYERMASK_CUSTOM //CUSTOM Lit 
#define UNIVERSAL_LAYERMASK_CUSTOM

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
    #include "MyLitSurfaceInput.hlsl"

    float4 _LayerTint;

    TEXTURE2D(_LayermaskMap);          
    SAMPLER(sampler_LayermaskMap);

    float _BlendMinValue;
    float _BlendMaxValue;

    float _BlendProceduralMinValue;
    float _BlendProceduralMaxValue;

    #if _LAYERMASK_BASE_PROCEDURAL
        inline float BaseProceduralMask(half inputdata , half baseMaskChannel){
            float mask = inputdata + baseMaskChannel;
            mask = smoothstep(_BlendProceduralMinValue , _BlendProceduralMaxValue , mask);

            return saturate(mask);
        }
    #endif

    #if _LAYERMASK_TINT
        #if _LAYERMASK_VERTEXCOL
            inline float4 GetLayerTint(float2 uv, float4 vertexcol){
                float4 tintmap = SAMPLE_TEXTURE2D(_LayermaskMap, sampler_LayermaskMap, uv);
                return tintmap.rgba;
            }
        #elif _LAYERMASK_TEX2D
            inline float4 GetLayerTint(float2 uv, float4 vertexcol){
                return vertexcol.rgba;
            }
        #endif
    #else
        inline float4 GetLayerTint(float2 uv, float4 vertexcol){
            return 1;
        }
    #endif

    #if _LAYERMASK_VERTEXCOL
         half4 GetLayerMask(float2 uv, float4 vertexcol){
                return vertexcol.rgba;
            }
    #elif _LAYERMASK_TEX2D
         half4 GetLayerMask(float2 uv, float4 vertexcol){
                half4 maskmap = SAMPLE_TEXTURE2D(_LayermaskMap , sampler_LayermaskMap , uv);
                return maskmap;
            }
    #else
         half4 GetLayerMask(float2 uv, float4 vertexcol){
            return 0;
        }
    #endif
    
    inline SurfaceData BlendAllLayer(float2 uv, float4 vertexcol){

        float4 maskMap = GetLayerMask(uv,vertexcol);
        float4 tintMap = GetLayerTint(uv,vertexcol);

        SurfaceData surfaceData01;
        SurfaceData surfaceData02;
        SurfaceData surfaceData03;
        SurfaceData surfaceData04;

        //..todo
    }

    inline SurfaceData BlendLayer(SurfaceData surfaceData01, SurfaceData surfaceData02, float maskmap, float4 tintmap){
        
        SurfaceData outSurfaceData = surfaceData01;

        outSurfaceData.albedo               = lerp(surfaceData01.albedo , surfaceData02.albedo * tintmap, maskmap) ;
        outSurfaceData.specular             = lerp(surfaceData01.specular , surfaceData02.specular , maskmap);
        outSurfaceData.metallic             = lerp(surfaceData01.metallic , surfaceData02.metallic , maskmap);
        outSurfaceData.smoothness           = lerp(surfaceData01.smoothness , surfaceData02.smoothness , maskmap);
        outSurfaceData.normalTS             = lerp(surfaceData01.normalTS , surfaceData02.normalTS , maskmap);
        outSurfaceData.emission             = lerp(surfaceData01.emission , surfaceData02.emission , maskmap);
        outSurfaceData.occlusion            = lerp(surfaceData01.occlusion , surfaceData02.occlusion , maskmap);
        outSurfaceData.alpha                = lerp(surfaceData01.alpha , surfaceData02.alpha , maskmap);
        outSurfaceData.clearCoatMask        = lerp(surfaceData01.clearCoatMask , surfaceData02.clearCoatMask , maskmap);
        outSurfaceData.clearCoatSmoothness  = lerp(surfaceData01.clearCoatSmoothness , surfaceData02.clearCoatSmoothness , maskmap);

        return outSurfaceData;
    }
    
#endif