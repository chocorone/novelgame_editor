using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static NovelData;
using static NovelData.ParagraphData;

internal class ChoiceNode : BaseNode
{
    public static List<ChoiceNode> nodes = new List<ChoiceNode>();
    private ChoiceData data;
    internal int Index
    {
        get
        {
            return data.index;
        }
    }
    internal int nextParaIndex
    {
        get
        {
            return data.nextParagraphIndex;
        }
    }

    //0から作られるとき
    public ChoiceNode()
    {
        //データを作成する
        data = NovelEditorWindow.Instance.NovelData.CreateChoice();
        NodeSet();
    }

    public ChoiceNode(Vector2 position)
    {
        data = NovelEditorWindow.Instance.NovelData.CreateChoice();
        SetPosition(new Rect(position, new Vector2(100, 100)));
        SavePosition(true, new Rect(position, new Vector2(100, 100)));
        NodeSet();
    }


    //データをもとに作られるとき
    public ChoiceNode(ChoiceData Cdata)
    {
        data = Cdata;

        NodeSet();
        SetPosition(data.choicePosition);
    }

    private protected override void NodeSet()
    {
        base.NodeSet();
        setTitle(data.text);

        //InputPort作成
        InputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));
        InputPort.portName = "prev";
        inputContainer.Add(InputPort);

        //OutputPort作成
        CountinuePort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(int));
        CountinuePort.portName = "next";
        outputContainer.Add(CountinuePort);

        nodes.Add(this);

    }

    public override void OnSelected()
    {
        try
        {
            TempChoice temp = ScriptableObject.CreateInstance<TempChoice>();
            //ノードのデータをインスペクターに反映
            temp.data = data;
            Selection.activeObject = temp;
        }
        catch
        {
            Selection.activeObject = null;
        }
    }

    public override void OnUnselected()
    {
        Selection.activeObject = null;

        if (data != null)
        {
            setTitle(data.text);
        }
    }

    internal override void SavePosition(bool flag = false, Rect rect = new Rect())
    {
        if (flag)
        {
            data.choicePosition = rect;
        }
        else
        {
            data.choicePosition = GetPosition();
        }

    }

    internal override void RemoveNext(BaseNode nextNode)
    {
        data.nextParagraphIndex = -1;
    }

    internal override void AddNext(BaseNode nextNode, Port outPort)
    {
        data.nextParagraphIndex = ((ParagraphNode)nextNode).Index;
    }

    internal override void SetDataNA()
    {
        data.index = -1;
    }

    internal override void ResetNextIndex()
    {
        data.nextParagraphIndex = -1;
    }

    internal override string SerializeData()
    {
        string serializedData = "choice|" + data.text;
        //ここにシリアライズ処理

        return serializedData;
    }

    internal override void DeserializeData(string sdata)
    {
        data.text = sdata;
        setTitle(data.text);
    }

}
