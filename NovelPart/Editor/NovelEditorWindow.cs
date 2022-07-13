using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static NovelData;
using static NovelData.ParagraphData;

public class NovelEditorWindow : EditorWindow
{

    [SerializeField] private static NovelEditorWindow instance;
    internal static NovelEditorWindow Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GetWindow<NovelEditorWindow>(ObjectNames.NicifyVariableName(nameof(NovelEditorWindow)), typeof(UnityEditor.SceneView));
                if (instance == null)
                {
                    Debug.LogError("NullRefarenceException NovelEditorWindow SingltonError");
                }
            }

            return instance;
        }
    }

    [SerializeField] private NovelData noveldata;
    internal NovelData NovelData
    {
        get
        {
            return noveldata;
        }
        set
        {
            noveldata = value;
            //saveNovelData();
        }
    }

    public string EditorFolderPath { get; private set; }

    internal void RecordData(String redoName)
    {
        try
        {
            Undo.RecordObject(noveldata, redoName);
        }
        catch (ArgumentNullException)
        {
            //NovelDataが削除されていたらウィンドウを閉じるようにしてる
            Close();
        }
    }

    internal void saveNovelData()
    {
        try
        {
            EditorUtility.SetDirty(noveldata);
            //AssetDatabase.SaveAssets();
        }
        catch (ArgumentNullException)
        {
            //NovelDataが削除されていたらウィンドウを閉じるようにしてる
            Close();
        }

    }

    public void CloseWindow()
    {
        Close();
    }


    public static void Open(NovelData d)
    {
        ////すでに開いてたら閉じて開き直す
        if (EditorWindowUtil.IsOpen<NovelEditorWindow>())
        {
            var findObjects = Resources.FindObjectsOfTypeAll<NovelEditorWindow>();
            findObjects[0].Close();
        }
        Instance.noveldata = d;
        //移動したい
        Undo.RecordObject(Instance.noveldata, "ノベルデータ修正");

        Instance.OnEnable();
    }

    void OnEnable()
    {
        var mono = MonoScript.FromScriptableObject(this);
        string path = AssetDatabase.GetAssetPath(mono);
        EditorFolderPath = path.Substring(0, path.Length - "NovelEditorWindow.cs".Length);


        //ウィンドウ整えてる
        rootVisualElement.Bind(new SerializedObject(this));

        if (noveldata != null)
        {
            titleContent = new GUIContent("NovelEdit : " + noveldata.name);

            NovelGraphView graphView;

            graphView = new NovelGraphView(this);

            rootVisualElement.Add(graphView);

            Undo.undoRedoPerformed += () =>
            {
                this.rootVisualElement.Clear();
                graphView = new NovelGraphView(this);

                rootVisualElement.Add(graphView);
            };
        }


    }

}