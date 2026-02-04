using UnityEngine;
using UnityEditor;

public class MToolsSettings : EditorWindow
{
    private Object targetAsset;
    private Editor cachedEditor;

    [MenuItem("Tools/MirrorTools")]
    public static void OpenWindow()
    {
        string assetPath = "Assets/Resources/MirrorTools/Config/MainConfig.asset";
        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

        if (asset != null)
        {
            var window = GetWindow<MToolsSettings>("Config Editor");
            window.targetAsset = asset;
            window.Show();
        }
    }

    private void OnGUI()
    {
        if (targetAsset == null) return;

        if (cachedEditor == null || cachedEditor.target != targetAsset)
        {
            Editor.CreateCachedEditor(targetAsset, null, ref cachedEditor);
        }

        cachedEditor.OnInspectorGUI();
    }
}