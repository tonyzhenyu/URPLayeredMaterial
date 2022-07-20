using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class SOLayerLitProperties : ScriptableObject
{
    public Material[] materials = new Material[4];
    public int layercount;
    public bool islayerfold;

    public static void SetMaterial(SOLayerLitProperties soOjbect,Material[] mats)
    {
        soOjbect.materials = mats;
        EditorUtility.SetDirty(soOjbect);   
    }
}
