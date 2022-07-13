using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

internal abstract class BaseNode : Node
{
    internal Port InputPort { get; private protected set; }

    internal Port CountinuePort { get; private protected set; }

    private protected virtual void NodeSet()
    {
        titleButtonContainer.Clear(); // デフォルトのCollapseボタンを削除

        RegisterCallback<MouseDownEvent>(MouseDowned);
    }

    private protected void MouseDowned(MouseEventBase<MouseDownEvent> evt)
    {
        OnSelected();
    }

    internal abstract void SavePosition(bool flag = false, Rect rect = new Rect());

    internal abstract void RemoveNext(BaseNode nextNode);

    internal abstract void AddNext(BaseNode nextNode, Port outPort);

    internal abstract void ResetNextIndex();

    internal abstract void SetDataNA();

    internal abstract string SerializeData();


    internal abstract void DeserializeData(string sdata);

    protected void setTitle(string text)
    {
        title = "";
        if (text.Length > 10)
        {
            //10文字だけ表示
            title = text.Substring(0, 10);
        }
        else
        {
            title = text;
        }
    }

}
