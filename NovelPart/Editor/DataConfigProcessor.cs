using UnityEditor;
using UnityEngine;

public class DataConfigProcessor : UnityEditor.AssetModificationProcessor
{
    private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
    {
        NovelData data = AssetDatabase.LoadAssetAtPath<NovelData>(assetPath);
        if (data != null)
        {
            if (NovelEditorWindow.Instance.NovelData == data)
            {
                NovelEditorWindow.Instance.CloseWindow();
            }
        }
        return AssetDeleteResult.DidNotDelete;
    }

}