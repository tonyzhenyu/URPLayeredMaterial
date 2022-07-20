using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using UnityEditor.Rendering;

public class MyLitShader : BaseShaderGUI
{
    MyLitProperties litProperties;
    MyLitDetailProperties.DetailProperties myLitDetailProperties;

    public override void FindProperties(MaterialProperty[] properties)
    {
        base.FindProperties(properties);
        litProperties.InitProperties(properties);
        myLitDetailProperties = new MyLitDetailProperties.DetailProperties(properties);
    }
    // material main surface options
    public override void DrawSurfaceOptions(Material material)
    {
        base.DrawSurfaceOptions(material);
    }
    public override void ValidateMaterial(Material material)
    {
        base.ValidateMaterial(material);
        MyLitDetailProperties.SetMaterialKeywords(material);
    }
    #region SurfaceInput
    public override void DrawSurfaceInputs(Material material)
    {
        litProperties.DrawSurfaceInputs(materialEditor);
        
        using (new EditorGUI.IndentLevelScope(2))
        {
            DrawTileOffset(materialEditor, baseMapProp);
        }
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Detail");
        MyLitDetailProperties.DoDetailArea(myLitDetailProperties, materialEditor);
    }
    #endregion
    #region AdvancedOptions
    public override void DrawAdvancedOptions(Material material)
    {
        if (litProperties.reflections != null && litProperties.highlights != null)
        {
            materialEditor.ShaderProperty(litProperties.highlights, LitGUI.Styles.highlightsText);
            materialEditor.ShaderProperty(litProperties.reflections, LitGUI.Styles.reflectionsText);
        }

        base.DrawAdvancedOptions(material);

    }
    #endregion
    
}
