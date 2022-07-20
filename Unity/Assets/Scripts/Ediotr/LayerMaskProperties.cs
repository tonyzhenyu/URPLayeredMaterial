using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public struct LayerMaskProperties
{
    enum MaskControlType
    {
        VertexColor,
        Texture2D
    }

    // Advanced Props
    public MaterialProperty highlights;
    public MaterialProperty reflections;

    public MaterialProperty _layerMaskMap;
    public MaterialProperty _BlendMinValue;
    public MaterialProperty _BlendMaxValue;

    public MaterialProperty _BlendProceduralMinValue;
    public MaterialProperty _BlendProceduralMaxValue;

    public MaterialProperty[] _BaseMap;
    public MaterialProperty[] _BumpMap;
    public MaterialProperty[] _PBRMaskMap;
    public MaterialProperty[] _HeightMap;

    public MaterialProperty[] _DetailAlbedoMap;
    public MaterialProperty[] _DetailNormalMap;
    public MaterialProperty[] _DetailPBRMaskMap;

    private bool _useTint;
    private bool _useBaseLayerProceduralMask;
    private MaskControlType _maskControlType;

    public static class ShaderString
    {
        public static readonly string layermaskMap = "_LayermaskMap";
        public static readonly string blendMinValue = "_BlendMinValue";
        public static readonly string blendMaxValue = "_BlendMaxValue";
        public static readonly string blendProceduralMinValue = "_BlendProceduralMinValue";
        public static readonly string blendProceduralMaxValue = "_BlendProceduralMaxValue";

        public static readonly string baseMap = "_BaseMap";
        public static readonly string bumpMap = "_BumpMap";
        public static readonly string pBRMaskMap = "_PBRMaskMap";

        public static readonly string heightMap = "_HeightMap";

        public static readonly string detailAlbedoMap = "_DetailAlbedoMap";
        public static readonly string detailNormalMap = "_DetailNormalMap";
        public static readonly string detailPBRMaskMap = "_DetailPBRMaskMap";

    }


    public void Init(MaterialProperty[] materialProperties , Material material)
    {
        _layerMaskMap = BaseShaderGUI.FindProperty(ShaderString.layermaskMap, materialProperties, false);

        _BlendMinValue = BaseShaderGUI.FindProperty(ShaderString.blendMinValue, materialProperties, false);
        _BlendMaxValue = BaseShaderGUI.FindProperty(ShaderString.blendMaxValue, materialProperties, false);

        _BlendProceduralMinValue = BaseShaderGUI.FindProperty(ShaderString.blendProceduralMinValue, materialProperties, false);
        _BlendProceduralMaxValue = BaseShaderGUI.FindProperty(ShaderString.blendProceduralMaxValue, materialProperties, false);
        // Advanced Props
        highlights = BaseShaderGUI.FindProperty("_SpecularHighlights", materialProperties, false);
        reflections = BaseShaderGUI.FindProperty("_EnvironmentReflections", materialProperties, false);

        string[] vs = { "", "01", "02", "03" };

        _BaseMap = new MaterialProperty[4];
        _BumpMap = new MaterialProperty[4];
        _PBRMaskMap = new MaterialProperty[4];
        _HeightMap = new MaterialProperty[4];
        _DetailAlbedoMap = new MaterialProperty[4];
        _DetailNormalMap = new MaterialProperty[4];
        _DetailPBRMaskMap = new MaterialProperty[4];

        for (int i = 0; i < 2; i++)
        {
            _BaseMap[i] = BaseShaderGUI.FindProperty(ShaderString.baseMap + vs[i], materialProperties, false);
            _BumpMap[i] = BaseShaderGUI.FindProperty(ShaderString.bumpMap + vs[i], materialProperties, false);
            _PBRMaskMap[i] = BaseShaderGUI.FindProperty(ShaderString.pBRMaskMap + vs[i], materialProperties, false);
            _HeightMap[i] = BaseShaderGUI.FindProperty(ShaderString.heightMap + vs[i], materialProperties, false);

            _DetailAlbedoMap[i] = BaseShaderGUI.FindProperty(ShaderString.detailAlbedoMap + vs[i], materialProperties, false);
            _DetailNormalMap[i] = BaseShaderGUI.FindProperty(ShaderString.detailNormalMap + vs[i], materialProperties, false);
            _DetailPBRMaskMap[i] = BaseShaderGUI.FindProperty(ShaderString.detailPBRMaskMap + vs[i], materialProperties, false);

            
        }




        var check_control_keyword = material.IsKeywordEnabled(new LocalKeyword(material.shader, "_LAYERMASK_VERTEXCOL"));

        if (check_control_keyword == true)
        {
            _maskControlType = MaskControlType.VertexColor;
        }
        else
        {
            _maskControlType = MaskControlType.Texture2D;
        }

        _useTint = material.IsKeywordEnabled(new LocalKeyword(material.shader, "_LAYERMASK_TINT"));
        _useBaseLayerProceduralMask = material.IsKeywordEnabled(new LocalKeyword(material.shader, "_LAYERMASK_BASE_PROCEDURAL"));
        CoreUtils.SetKeyword(material, "_LAYER", true);
    }
    public void DrawLayerMaskProperties(MaterialEditor materialEditor)
    {
        Material material = materialEditor.target as Material;

        _maskControlType = (MaskControlType)EditorGUILayout.EnumPopup("Mask Control Type", _maskControlType);

        using (new EditorGUI.IndentLevelScope(2))
        {
            var minvs = _BlendMinValue.floatValue;
            var maxvs = _BlendMaxValue.floatValue;

            EditorGUILayout.MinMaxSlider("Blend Weight", ref minvs, ref maxvs, -10, 10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Min:{minvs}");
            EditorGUILayout.LabelField($"Max:{maxvs}");
            EditorGUILayout.EndHorizontal();

            _BlendMinValue.floatValue = minvs;
            _BlendMaxValue.floatValue = maxvs;

            _useTint = EditorGUILayout.Toggle("Use Tint", _useTint);
            _useBaseLayerProceduralMask = EditorGUILayout.Toggle("BaseLayer Procedural Mask", _useBaseLayerProceduralMask);

            if (_useBaseLayerProceduralMask)
            {
                float minvs01 = _BlendProceduralMinValue.floatValue;
                float maxvs01 = _BlendProceduralMaxValue.floatValue;

                EditorGUILayout.MinMaxSlider("Blend Weight", ref minvs01, ref maxvs01, -10, 10);

                _BlendProceduralMinValue.floatValue = minvs01;
                _BlendProceduralMaxValue.floatValue = maxvs01;
            }

            if (_maskControlType == MaskControlType.Texture2D)
            {
                GUIContent layermaskContent = new GUIContent("Layer Mask Map");
                layermaskContent.tooltip = "Layer Mask RGBA For 4 Layers";

                materialEditor.TexturePropertySingleLine(layermaskContent, _layerMaskMap);
            }
            else if (_maskControlType == MaskControlType.VertexColor)
            {
                if (_useTint == true)
                {
                    GUIContent tintmap = new GUIContent("Tint Map");
                    tintmap.tooltip = "Tint RGB A For Weight.";

                    materialEditor.TexturePropertySingleLine(tintmap, _layerMaskMap);
                }
            }

            CoreUtils.SetKeyword(material, "_LAYERMASK_VERTEXCOL", _maskControlType == MaskControlType.VertexColor);
            CoreUtils.SetKeyword(material, "_LAYERMASK_TEX2D", _maskControlType == MaskControlType.Texture2D);
            CoreUtils.SetKeyword(material, "_LAYERMASK_BASE_PROCEDURAL", _useBaseLayerProceduralMask);
            CoreUtils.SetKeyword(material, "_LAYERMASK_TINT", _useTint);
            

        }
        EditorGUILayout.Space(10);

    }

}
