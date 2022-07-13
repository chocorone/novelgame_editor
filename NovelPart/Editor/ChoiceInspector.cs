using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TempChoice))]
internal class ChoiceInspector : Editor
{
    TempChoice tmpdata;

    void OnEnable()
    {
        tmpdata = target as TempChoice;

    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty data = serializedObject.FindProperty("data");
        SerializedProperty text = data.FindPropertyRelative("text");

        text.stringValue= EditorGUILayout.TextField("選択肢のテキスト",text.stringValue);

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(NovelEditorWindow.Instance.NovelData);
        //AssetDatabase.SaveAssets();


    }
}
