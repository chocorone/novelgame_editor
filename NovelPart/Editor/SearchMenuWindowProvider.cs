using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

internal class SearchMenuWindowProvider : ScriptableObject, ISearchWindowProvider
{
    private NovelGraphView _graphView;
    private EditorWindow _editorWindow;

    internal void Initialize(NovelGraphView graphView, EditorWindow editorWindow)
    {
        _graphView = graphView;
        _editorWindow = editorWindow;
    }

    //右クリックで開くメニュー
    List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
    {
        var entries = new List<SearchTreeEntry>();
        entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));
        entries.Add(new SearchTreeEntry(new GUIContent(nameof(ParagraphNode))) { level = 1, userData = typeof(ParagraphNode) });
        entries.Add(new SearchTreeEntry(new GUIContent(nameof(ChoiceNode))) { level = 1, userData = typeof(ChoiceNode) });

        return entries;
    }

    //メニュからノードを追加する処理
    bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        var type = searchTreeEntry.userData as Type;
        var node = Activator.CreateInstance(type) as BaseNode;

        // マウスの位置にノードを追加
        var worldMousePosition = _editorWindow.rootVisualElement.ChangeCoordinatesTo(_editorWindow.rootVisualElement.parent, context.screenMousePosition - _editorWindow.position.position);
        var localMousePosition = _graphView.contentViewContainer.WorldToLocal(worldMousePosition);
        node.SetPosition(new Rect(localMousePosition, new Vector2(100, 100)));

        //GraphViewにノードを追加して位置をセーブ
        _graphView.AddElement(node);
        node.SavePosition(true,new Rect(localMousePosition, new Vector2(100, 100)));
        return true;
    }
}