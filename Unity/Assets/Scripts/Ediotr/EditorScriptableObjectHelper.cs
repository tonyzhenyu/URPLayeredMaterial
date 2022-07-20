using UnityEditor;
using UnityEngine;

public static class EditorScriptableObjectHelper
{
    public static T LoadEditorDataRaw<T>(string name) where T : ScriptableObject
    {
        string[] path = name.Split('/');

        string folderpath = "";

        for (int i = 0; i < path.Length; i++)
        {
            if (i == path.Length - 1)
            {
                break;
            }
            folderpath += path[i] + "/";
        }
        string assetPath = $"{folderpath}{path[path.Length-1]}.asset";

        if (AssetDatabase.IsValidFolder(folderpath) == false)
        {
            //AssetDatabase.CreateFolder(parentFolder, foldername);
        }

        T data = AssetDatabase.LoadAssetAtPath<T>(assetPath);

        if (data == null)
        {
            data = ScriptableObject.CreateInstance(typeof(T)) as T;
            EditorUtility.SetDirty(data);
            AssetDatabase.CreateAsset(data, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        //Debug.Log("LoadData");
        return data;
    }
}