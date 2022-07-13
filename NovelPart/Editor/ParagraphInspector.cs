using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static NovelData;
using static NovelData.ParagraphData;

[CustomEditor(typeof(TempParagraph))]
internal class ParagraphInspector : Editor
{
    internal static bool dataChanged = false;
    TempParagraph tmpdata;
    private ReorderableList reorderableList;
    private SerializedProperty daialogueDataList;
    private int index;

    void OnEnable()
    {
        tmpdata = target as TempParagraph;

        SerializedProperty data = serializedObject.FindProperty("data");
        daialogueDataList = data.FindPropertyRelative("dialogueList");
        SetReorderableList();
        index = data.FindPropertyRelative("index").intValue;
    }


    public override void OnInspectorGUI()
    {
        if (index == 0)
        {
            EditorGUILayout.LabelField("最初に表示される会話です");
        }
        else
        {
            EditorGUILayout.LabelField("！現在の立ち絵や背景に注意");
        }
        NovelEditorWindow.Instance.RecordData("change paragraph");
        bool flag = EditorGUILayout.ToggleLeft("詳細設定全部開く", tmpdata.data.detailOpen);

        if (flag != tmpdata.data.detailOpen)
        {
            //ここで全部をひらく処理
            foreach (Dialogue data in tmpdata.data.dialogueList)
            {
                data.open = flag;
            }

            tmpdata.data.detailOpen = flag;
            dataChanged = true;
        }

        //表示するParagraphDataが変わったとき
        if (dataChanged)
        {
            SetReorderableList();
            dataChanged = false;
        }

        serializedObject.Update();
        reorderableList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(NovelEditorWindow.Instance.NovelData);
        //AssetDatabase.SaveAssets();

    }

    void SetReorderableList()
    {
        reorderableList = new ReorderableList(serializedObject, daialogueDataList);
        reorderableList.drawElementCallback = (rect, index, active, focused) =>
        {
            var actionData = daialogueDataList.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, actionData);
        };
        reorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "会話のリスト");
        reorderableList.elementHeightCallback = index => EditorGUI.GetPropertyHeight(daialogueDataList.GetArrayElementAtIndex(index));
        reorderableList.onCanRemoveCallback = (ReorderableList list) =>
        {
            //初期状態設定したい(全部Noneに)
            return reorderableList.count > 1;//2個以上の時しか削除できないように
        };
        reorderableList.onAddCallback = (ReorderableList l) =>
        {
            var index = l.serializedProperty.arraySize;
            l.serializedProperty.arraySize++;
            l.index = index;
            var element = l.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty howChara = element.FindPropertyRelative("howCharas");
            for (int i = 0; i < howChara.arraySize; i++)
            {
                howChara.GetArrayElementAtIndex(i).enumValueIndex = (int)CharaChangeStyle.UnChange;
            }

            element.FindPropertyRelative("howBack").enumValueIndex = (int)BackChangeStyle.UnChange;

            element.FindPropertyRelative("index").intValue = index;

            element.FindPropertyRelative("BGMStyle").enumValueIndex = (int)SoundStyle.UnChange;
            element.FindPropertyRelative("SEStyle").enumValueIndex = (int)SoundStyle.UnChange;
        };


        reorderableList.onChangedCallback = (ReorderableList list) =>
        {
            Changed();
        };
    }

    void Changed()
    {
        for (int i = 0; i < tmpdata.data.dialogueList.Count; i++)
        {
            tmpdata.data.dialogueList[i].index = i;
        }
    }

}