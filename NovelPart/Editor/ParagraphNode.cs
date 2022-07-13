using UnityEngine;
using UnityEditor;
using static NovelData;
using static NovelData.ParagraphData;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEditor.UIElements;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;

internal class ParagraphNode : BaseNode
{

    public static List<ParagraphNode> nodes = new List<ParagraphNode>();
    //privateにしたい
    private ParagraphData data;
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
    internal List<int> nextChoices
    {
        get
        {
            return data.nextChoiceIndexes;
        }
    }
    internal Next next
    {
        get
        {
            return data.next;
        }
    }
    internal List<Port> choicePorts = new List<Port>();

    //0から作られるとき
    public ParagraphNode()
    {
        NovelEditorWindow.Instance.RecordData("Create Paragraph");
        //データを作成する
        data = NovelEditorWindow.Instance.NovelData.CreateParagraph();
        NodeSet();
        NovelEditorWindow.Instance.saveNovelData();
    }


    //データをもとに作られるとき
    public ParagraphNode(ParagraphData Pdata)
    {
        data = Pdata;

        NodeSet();
        SetPosition(data.ParagraphPosition);
    }

    //コピー&ペーストで作られる時
    public ParagraphNode(string serializedData, Vector2 position)
    {
        //データを作成する 
        data = NovelEditorWindow.Instance.NovelData.CreateParagraphFromJson(serializedData);
        SetPosition(new Rect(position, new Vector2(100, 100)));
        SavePosition(true, new Rect(position, new Vector2(100, 100)));
        NodeSet();
    }
    //あとで消す
    public ParagraphNode(string serializedData)
    {
        //データを作成する 
        data = NovelEditorWindow.Instance.NovelData.CreateParagraphFromJson(serializedData);
        NodeSet();
    }
    internal override void SavePosition(bool flag = false, Rect rect = new Rect())
    {
        if (flag)
        {
            data.ParagraphPosition = rect;
        }
        else
        {
            data.ParagraphPosition = GetPosition();
        }

    }

    public override void OnSelected()
    {
        try
        {
            TempParagraph temp = ScriptableObject.CreateInstance<TempParagraph>();
            temp.data = data;
            Selection.activeObject = temp;
            //これどうにか
            ParagraphInspector.dataChanged = true;

        }
        catch
        {
            Selection.activeObject = null;
        }

    }

    private protected override void NodeSet()
    {
        base.NodeSet();

        setTitle(data.dialogueList[0].text);

        //ノード色変更
        if (data.index == 0)
        {
            titleContainer.style.backgroundColor = new Color(0.8f, 0.2f, 0.4f);
            capabilities -= Capabilities.Deletable;
        }
        else
        {
            titleContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.8f);
        }

        //ボタン生成
        styleSheets.Add(EditorGUIUtility.Load(NovelEditorWindow.Instance.EditorFolderPath + "PNodeUSS.uss") as StyleSheet);
        var visualTree = EditorGUIUtility.Load(NovelEditorWindow.Instance.EditorFolderPath + "NodeUXML.uxml") as VisualTreeAsset;
        visualTree.CloneTree(titleButtonContainer);

        var addbutton = titleButtonContainer.Q<Button>("addChoiceButton");
        addbutton.clickable.clicked += () => { AddChoicePort(false); };

        //ドロップリスト
        var nextStateField = new EnumField(data.next);
        nextStateField.RegisterValueChangedCallback(evt =>
        {
            data.next = (Next)nextStateField.value;
            //変更されたものによってデータ消したりしてる
            if (data.next != Next.Choice)
            {
                foreach (Port i in choicePorts)
                {
                    RemoveChoiceEdge(i.connections);
                }
                choicePorts = new List<Port>();
                data.nextChoiceIndexes = new List<int>();
                data.nextChoiceIndexes.Add(-1);
            }
            if (data.next != Next.Continue)
            {
                data.nextParagraphIndex = -1;
                if (CountinuePort != null)
                {
                    foreach (Edge e in CountinuePort.connections)
                    {
                        e.input.Disconnect(e);
                        e.RemoveFromHierarchy();
                    }
                }

            }
            OutPortSet();
        });
        mainContainer.Add(nextStateField);

        // 入力用のポートを作成
        InputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(int));
        InputPort.portName = "prev";
        inputContainer.Add(InputPort);

        OutPortSet(true);

        nodes.Add(this);
    }

    //NextによってOutポートを変化させる
    void OutPortSet(bool whenNodeCreate = false)
    {
        outputContainer.Clear();
        switch (data.next)
        {
            case Next.Choice:
                var choicePort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(string));
                choicePort.portName = "choice1";
                outputContainer.Add(choicePort);
                choicePorts.Add(choicePort);

                //ノード作成時はデータのChoiceの数だけ
                if (whenNodeCreate)
                {
                    for (int i = 1; i < data.nextChoiceIndexes.Count; i++)
                    {
                        AddChoicePort(true);
                    }
                }

                break;

            case Next.Continue:
                CountinuePort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(int));
                CountinuePort.portName = "next";
                outputContainer.Add(CountinuePort);
                break;
        }
        RefreshExpandedState();
    }

    //+ボタン押したらよばれるやつ
    protected void AddChoicePort(bool whenNodeCreate)
    {
        if (data.next == Next.Choice)
        {
            //ノード作成時(データからの復元)の時以外は
            if (!whenNodeCreate)
            {
                data.nextChoiceIndexes.Add(-1);
            }

            //-ボタン作成
            Button removePortButton = new Button();
            removePortButton.text = "-";
            //ChoicePort作成
            Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(string));
            removePortButton.clickable.clicked += () =>
            {
                RemoveChoicePort(removePortButton, outputPort);
            };
            outputPort.portName = "choice" + (choicePorts.Count + 1).ToString();
            outputContainer.Add(outputPort);
            outputContainer.Add(removePortButton);
            choicePorts.Add(outputPort);
            RefreshPorts();
            RefreshExpandedState();
        }
    }

    //-ボタンを押したときにそのポートを消す
    private void RemoveChoicePort(Button rmvButton, Port outPort)
    {
        int index = choicePorts.IndexOf(outPort);
        choicePorts.RemoveAt(index);
        data.nextChoiceIndexes.RemoveAt(index);
        //Choiceポートとボタンを削除する
        RemoveChoiceEdge(outPort.connections);
        outputContainer.Remove(rmvButton);
        outputContainer.Remove(outPort);
    }

    void RemoveChoiceEdge(IEnumerable<Edge> e)
    {
        foreach (Edge i in e)
        {
            i.input.Disconnect(i);
            i.RemoveFromHierarchy();
        }
    }

    public override void OnUnselected()
    {
        Selection.activeObject = null;

        if (data != null)
        {
            setTitle(data.dialogueList[0].text);
        }
    }

    internal override void RemoveNext(BaseNode nextNode)
    {
        if (nextNode is ParagraphNode)
        {
            data.nextParagraphIndex = -1;
        }
        else if (nextNode is ChoiceNode)
        {
            int index = ((ChoiceNode)nextNode).Index;
            int portNum = data.nextChoiceIndexes.FindIndex(x => x == index);
            data.nextChoiceIndexes[portNum] = -1;
        }
    }

    internal override void AddNext(BaseNode nextNode, Port outPort)
    {
        if (nextNode is ParagraphNode)
        {
            data.nextParagraphIndex = ((ParagraphNode)nextNode).Index;
        }
        else if (nextNode is ChoiceNode)
        {
            int portNum = choicePorts.IndexOf(outPort);
            data.nextChoiceIndexes[portNum] = ((ChoiceNode)nextNode).Index;
        }
    }

    internal override void SetDataNA()
    {
        data.index = -1;
    }

    internal override void ResetNextIndex()
    {
        data.nextParagraphIndex = -1;
    }

    internal void ResetChoices(int index)
    {
        for (int i = 0; i < data.nextChoiceIndexes.Count; i++)
        {
            if (data.nextChoiceIndexes[i] == index)
                data.nextChoiceIndexes[i] = -1;
        }
    }

    internal override string SerializeData()
    {
        string serializedData = "paragraph|";
        //ここにシリアライズ処理
        String json = JsonUtility.ToJson(data, prettyPrint: true);
        serializedData += json;
        return serializedData;
    }

    internal override void DeserializeData(string sdata)
    {
        ParagraphData newData = JsonUtility.FromJson<ParagraphData>(sdata);
        newData.index = data.index;
        newData.nextChoiceIndexes = data.nextChoiceIndexes;
        newData.next = data.next;
        newData.nextParagraphIndex = data.nextParagraphIndex;
        data = newData;
        setTitle(data.dialogueList[0].text);
    }
}