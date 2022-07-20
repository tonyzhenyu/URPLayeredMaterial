using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyLayeredLitShader : BaseShaderGUI
{
    LayerMaskProperties _layerMaskProperties;

    Material[] _materialLayers { get; set; }
    SOLayerLitProperties _SOLayerLitProperties;
    int _layerCount { get; set; }
    bool _layerfold { get; set; }

    public override void OnClosed(Material material)
    {
        base.OnClosed(material);

    }
    public override void OnOpenGUI(Material material, MaterialEditor materialEditor)
    {
        base.OnOpenGUI(material, materialEditor);
        _SOLayerLitProperties = EditorScriptableObjectHelper.LoadEditorDataRaw<SOLayerLitProperties>(AssetDatabase.GetAssetPath(material));
        _materialLayers = _SOLayerLitProperties.materials;
        _layerCount = _SOLayerLitProperties.layercount;
        _layerfold = _SOLayerLitProperties.islayerfold;
    }
    public override void FindProperties(MaterialProperty[] properties)
    {
        base.FindProperties(properties);
        
        _layerMaskProperties.Init(properties,materialEditor.target as Material);
        
    }
    public override void DrawSurfaceInputs(Material material)
    {
        EditorGUILayout.LabelField(new GUIContent("Mask Input Come From albedo.alpha"));
        _layerMaskProperties.DrawLayerMaskProperties(materialEditor);
        _layerCount = EditorGUILayout.IntSlider("Layers Count", _layerCount, 1, 4);
        _SOLayerLitProperties.layercount = _layerCount;

        using (new EditorGUI.IndentLevelScope(1))
        {
            _layerfold = EditorGUILayout.Foldout(_layerfold, "Material Layers");
            _SOLayerLitProperties.islayerfold = _layerfold;
            EditorGUILayout.BeginVertical();

            if (_layerfold)
            {
                DoLayerMaterials(material,_layerMaskProperties,ref _SOLayerLitProperties);
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void DoLayerMaterials(Material material,LayerMaskProperties layer,ref SOLayerLitProperties solayer)
    {

        Material[] mats = solayer.materials;

        for (int i = 0; i < _layerCount; i++)
        {
            if (i == 0)
            {
                mats[i] = EditorGUILayout.ObjectField($"Base Layer:", mats[i], typeof(Material), true) as Material;
                continue;
            }
            mats[i] = EditorGUILayout.ObjectField($"Layer {i}:", mats[i], typeof(Material), true) as Material;
        }

        //Transfer Layer Data
        GUILayout.Space(10);
        if (GUILayout.Button("Synchronize Material"))
        {
            string[] vs = { "", "01", "02", "03" };
            string[] vs1 = { "_BaseMap", "_BumpMap", "_PBRMaskMap", "_HeightMap", "_DetailAlbedoMap", "_DetailNormalMap", "_DetailPBRMaskMap" };
            string[] vs2 = { "_BaseColor", "_BumpScale", "_Smoothness", "_Metallic", "_OcclusionStrength" };

            //set material properties
            for (int i = 0; i < _layerCount; i++)
            {
                if (mats[i] == null)
                {
                    continue;
                }

                layer._BaseMap[i].textureValue = mats[i].GetTexture(Shader.PropertyToID(vs1[0]));

                Vector2 basemapscale = mats[i].GetTextureScale(Shader.PropertyToID(vs1[0]));
                Vector2 basemapoffset = mats[i].GetTextureOffset(Shader.PropertyToID(vs1[0]));
                layer._BaseMap[i].textureScaleAndOffset = new Vector4(basemapscale.x, basemapscale.y, basemapoffset.x, basemapoffset.y);

                layer._BumpMap[i].textureValue = mats[i].GetTexture(Shader.PropertyToID(vs1[1]));
                layer._PBRMaskMap[i].textureValue = mats[i].GetTexture(Shader.PropertyToID(vs1[2]));
                layer._HeightMap[i].textureValue = mats[i].GetTexture(Shader.PropertyToID(vs1[3]));

                


                if (layer._BumpMap[i].textureValue != null)
                {
                    CoreUtils.SetKeyword(material, ShaderKeywordStrings._NORMALMAP, true);
                }

                material.SetColor(Shader.PropertyToID(vs2[0] + vs[i]), mats[i].GetColor(Shader.PropertyToID(vs2[0])));

                material.SetFloat(Shader.PropertyToID(vs2[1] + vs[i]), mats[i].GetFloat(Shader.PropertyToID(vs2[1])));
                material.SetFloat(Shader.PropertyToID(vs2[2] + vs[i]), mats[i].GetFloat(Shader.PropertyToID(vs2[2])));
                material.SetFloat(Shader.PropertyToID(vs2[3] + vs[i]), mats[i].GetFloat(Shader.PropertyToID(vs2[3])));
                material.SetFloat(Shader.PropertyToID(vs2[4] + vs[i]), mats[i].GetFloat(Shader.PropertyToID(vs2[4])));

                if (layer._DetailAlbedoMap[i] == null)
                {
                    continue;
                }
                //------detail mapping
                layer._DetailAlbedoMap[i].textureValue = mats[i].GetTexture(Shader.PropertyToID(vs1[4]));

                Vector2 detailmapscale = mats[i].GetTextureScale(Shader.PropertyToID(vs1[4]));
                Vector2 detailmapoffset = mats[i].GetTextureOffset(Shader.PropertyToID(vs1[4]));
                layer._DetailAlbedoMap[i].textureScaleAndOffset = new Vector4(detailmapscale.x, detailmapscale.y, detailmapoffset.x, detailmapoffset.y);

                layer._DetailNormalMap[i].textureValue = mats[i].GetTexture(Shader.PropertyToID(vs1[5]));
                layer._DetailPBRMaskMap[i].textureValue = mats[i].GetTexture(Shader.PropertyToID(vs1[6]));
            }
            MyLitDetailProperties.SetMaterialKeywords(material);
            SOLayerLitProperties.SetMaterial(solayer, mats);
            //solayer.materials = mats;
        }
    }
    #region AdvancedOptions
    public override void DrawAdvancedOptions(Material material)
    {
        if (_layerMaskProperties.reflections != null && _layerMaskProperties.highlights != null)
        {
            materialEditor.ShaderProperty(_layerMaskProperties.highlights, LitGUI.Styles.highlightsText);
            materialEditor.ShaderProperty(_layerMaskProperties.reflections, LitGUI.Styles.reflectionsText);
        }

        base.DrawAdvancedOptions(material);

    }
    #endregion

}
