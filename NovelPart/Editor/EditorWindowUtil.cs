using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class EditorWindowUtil
{
    /// <summary>
    /// Editor Window がすでに開かれているかどうか
    /// </summary>
    public static bool IsOpen<T>() where T : EditorWindow
    {
        var findObjects = Resources.FindObjectsOfTypeAll<T>();
        return findObjects.Length > 0;
    }

    /// <summary>
    /// Editor Window が開いてあれば開いてあるものを
    /// 開いてなければ開く
    /// </summary>
    public static T GetWindow<T>() where T : EditorWindow
    {
        var findObjects = Resources.FindObjectsOfTypeAll<T>();
        return findObjects.Length > 0 ? findObjects[0] as T : EditorWindow.GetWindow(typeof(T)) as T;
    }

    //projectWindowで開いているディレクトリを取得するための拡張機能
    public static string GetCurrentDirectory()
    {
        var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        var asm = Assembly.Load("UnityEditor.dll");
        var typeProjectBrowser = asm.GetType("UnityEditor.ProjectBrowser");
        var projectBrowserWindow = EditorWindow.GetWindow(typeProjectBrowser);
        return (string)typeProjectBrowser.GetMethod("GetActiveFolderPath", flag).Invoke(projectBrowserWindow, null);
    }

}