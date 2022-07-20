using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public struct MyLitProperties
{
    public MaterialProperty workflowMode;

    public MaterialProperty basemap;
    public MaterialProperty baseColor;

    public MaterialProperty heightmap;

    public MaterialProperty normalmap;
    public MaterialProperty normalScale;

    public MaterialProperty pbrMaskMap;

    public MaterialProperty metallic;
    public MaterialProperty roughness;
    public MaterialProperty occlusion;
    public MaterialProperty emission;

    public MaterialProperty emissionColor;

    // Advanced Props
    public MaterialProperty highlights;
    public MaterialProperty reflections;

    public bool toggle_emissive;
    public static class Styles
    {
        public static readonly GUIContent basemapcontent = EditorGUIUtility.TrTextContent("Base Map", "BaseMap RGB Alpha For Mask");
        public static readonly GUIContent normalmapContent = EditorGUIUtility.TrTextContent("Normal Map", "Normal");
        public static readonly GUIContent pbrmaskmapContent = EditorGUIUtility.TrTextContent("PBR Mask Map", "R:roughness ,G:Metallic , B:Occlusion , A:EmissionMask");
        public static readonly GUIContent heightmapContent = new GUIContent("Height Map (R)");
    }
    public void InitProperties(MaterialProperty[] properties)
    {
        workflowMode = BaseShaderGUI.FindProperty("_WorkflowMode", properties);

        basemap = BaseShaderGUI.FindProperty("_BaseMap", properties);
        baseColor = BaseShaderGUI.FindProperty("_BaseColor", properties);

        heightmap = BaseShaderGUI.FindProperty("_HeightMap", properties);

        normalmap = BaseShaderGUI.FindProperty("_BumpMap", properties, false);
        normalScale = BaseShaderGUI.FindProperty("_BumpScale", properties);

        pbrMaskMap = BaseShaderGUI.FindProperty("_PBRMaskMap", properties, false);//..todo

        metallic = BaseShaderGUI.FindProperty("_Metallic", properties, false);
        roughness = BaseShaderGUI.FindProperty("_Smoothness", properties, false);
        occlusion = BaseShaderGUI.FindProperty("_OcclusionStrength", properties, false);
        emission = BaseShaderGUI.FindProperty("_EmissionMap", properties, false);//..todo shader

        emissionColor = BaseShaderGUI.FindProperty("_EmissionColor", properties);

        // Advanced Props
        highlights = BaseShaderGUI.FindProperty("_SpecularHighlights", properties, false);
        reflections = BaseShaderGUI.FindProperty("_EnvironmentReflections", properties, false);
        
    }
    public void DrawSurfaceInputs(MaterialEditor materialEditor)
    {
        

        Material material = materialEditor.target as Material;

        materialEditor.TexturePropertySingleLine(Styles.basemapcontent, basemap, baseColor);

        //normal
        if (normalmap.textureValue != null)
        {
            materialEditor.TexturePropertySingleLine(Styles.normalmapContent, normalmap, normalScale);
        }
        else
        {
            materialEditor.TexturePropertySingleLine(Styles.normalmapContent, normalmap);
        }
        CoreUtils.SetKeyword(material, ShaderKeywordStrings._NORMALMAP, normalmap.textureValue != null);

        materialEditor.TexturePropertySingleLine(Styles.pbrmaskmapContent, pbrMaskMap);
        using (new EditorGUI.IndentLevelScope(2))
        {
            materialEditor.ShaderProperty(roughness, "(R) Roughness");
            materialEditor.ShaderProperty(metallic, "(G) Metallic");
            materialEditor.ShaderProperty(occlusion, "(B) Occlusion");
        }

        materialEditor.TexturePropertySingleLine(Styles.heightmapContent, heightmap);
        toggle_emissive = materialEditor.EmissionEnabledProperty();
        //toggle_emissive = EditorGUILayout.Toggle("Use Emission", toggle_emissive);
        using (new EditorGUI.IndentLevelScope(2))
        {
            if (toggle_emissive)
            {
                materialEditor.ShaderProperty(emissionColor, "(A)Emission");
            }
            CoreUtils.SetKeyword(material, ShaderKeywordStrings._EMISSION, toggle_emissive);
        }
        
    }
}