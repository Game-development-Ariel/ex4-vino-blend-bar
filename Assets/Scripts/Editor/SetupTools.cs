using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Small helpers shared by the editor tools that build the scenes.
/// </summary>
public static class SetupTools
{
    /// <summary>Loads a ScriptableObject asset, and creates it with default values if it doesn't exist yet.</summary>
    public static T Asset<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }

    /// <summary>Sets a private [SerializeField] reference, the same way dragging into the Inspector does.</summary>
    public static void SetField(Object target, string field, Object value)
    {
        SerializedObject fields = new SerializedObject(target);
        SerializedProperty property = fields.FindProperty(field);
        if (property == null)
        {
            Debug.LogError($"{target.GetType().Name} has no field called {field}.");
            return;
        }
        property.objectReferenceValue = value;
        fields.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Sets a private [SerializeField] array of references.</summary>
    public static void SetArray(Object target, string field, Object[] values)
    {
        SerializedObject fields = new SerializedObject(target);
        SerializedProperty property = fields.FindProperty(field);
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
        fields.ApplyModifiedPropertiesWithoutUndo();
    }
}
