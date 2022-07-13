using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using static NovelData;

internal class NovelGraphView : GraphView
{

    SearchMenuWindowProvider menuWindowProvider;
    public NovelGraphView(EditorWindow editorWindow)
    {
        Initiallize(editorWindow);
        NodeInitiallize();
    }

    internal void Initiallize(EditorWindow editorWindow)
    {
        // 親のサイズに合わせてGraphViewのサイズを設定
        this.StretchToParentSize();
        float newScale = Mathf.Clamp(NovelEditorWindow.Instance.NovelData.graphZoom, ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        // MMBスクロールでズームインアウトができるように
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale, ContentZoomer.DefaultScaleStep, newScale);
        NovelEditorWindow.Instance.NovelData.graphZoom = scale;

        // MMBドラッグで描画範囲を動かせるように
        this.AddManipulator(new ContentDragger());
        // LMBドラッグで選択した要素を動かせるように
        this.AddManipulator(new SelectionDragger());
        // LMBドラッグで範囲選択ができるように
        this.AddManipulator(new RectangleSelector());

        // 右クリックでメニューを開く
        menuWindowProvider = ScriptableObject.CreateInstance<SearchMenuWindowProvider>();
        menuWindowProvider.Initialize(this, editorWindow);
        nodeCreationRequest += context =>
        {
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), menuWindowProvider);
        };

        graphViewChanged += OnGraphChange;
        viewTransformChanged += SaveZoom;
    }



    //ノードのルール
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        compatiblePorts.AddRange(ports.ToList().Where(port =>
        {
            // 同じノードには繋げない
            if (startPort.node == port.node)
                return false;

            // Input同士、Output同士は繋げない
            if (port.direction == startPort.direction)
                return false;

            // ポートの型が一致していない場合は繋げない
            if (port.portType != startPort.portType)
                return false;


            return true;
        }));

        return compatiblePorts;
    }

    private void NodeInitiallize()
    {
        NovelData data = NovelEditorWindow.Instance.NovelData;

        ParagraphNode.nodes = new List<ParagraphNode>();
        ChoiceNode.nodes = new List<ChoiceNode>();
        //データからノードを作る
        foreach (ParagraphData pdata in data.ParagraphList)
        {
            if (pdata.index != -1)
            {
                ParagraphNode node = new ParagraphNode(pdata);
                AddElement(node);
            }
        }

        foreach (ChoiceData cdata in data.ChoiceList)
        {
            if (cdata.index != -1)
            {
                ChoiceNode node = new ChoiceNode(cdata);
                AddElement(node);
            }

        }


        foreach (ParagraphNode node in ParagraphNode.nodes)
        {
            if (node.next == Next.Continue)
            {
                //ParagraphからParagraphにつなぐ
                if (node.nextParaIndex == -1)
                    continue;
                int index = ParagraphNode.nodes.FindIndex(x => x.Index == node.nextParaIndex);
                Edge edge = node.CountinuePort.ConnectTo(ParagraphNode.nodes[index].InputPort);
                AddElement(edge);
            }

            else if (node.next == Next.Choice)
            {
                //ParagraphからChoiceにつなぐ
                for (int i = 0; i < node.nextChoices.Count; i++)
                {
                    if (node.nextChoices[i] == -1)
                    {
                        continue;
                    }
                    int index = ChoiceNode.nodes.FindIndex(x => x.Index == node.nextChoices[i]);
                    if (index == -1)
                        continue;
                    Edge edge = node.choicePorts[i].ConnectTo(ChoiceNode.nodes[index].InputPort);
                    AddElement(edge);
                }
            }
        }

        foreach (ChoiceNode node in ChoiceNode.nodes)
        {
            if (node.nextParaIndex != -1)
            {
                int index = ParagraphNode.nodes.FindIndex(x => x.Index == node.nextParaIndex);
                Edge edge = node.CountinuePort.ConnectTo(ParagraphNode.nodes[index].InputPort);
                AddElement(edge);
            }
        }
    }

    private void SaveZoom(GraphView graphView)
    {
        NovelEditorWindow.Instance.NovelData.graphZoom = scale;
        NovelEditorWindow.Instance.saveNovelData();
    }

    //ノードが変化したときに呼ばれる
    private GraphViewChange OnGraphChange(GraphViewChange change)
    {

        //エッジが作成されたとき
        if (change.edgesToCreate != null)
        {
            NovelEditorWindow.Instance.RecordData("Create Edge");
            foreach (Edge edge in change.edgesToCreate)
            {
                if (edge.output.node is BaseNode && edge.input.node is BaseNode)
                {
                    ((BaseNode)edge.output.node).AddNext((BaseNode)edge.input.node, edge.output);
                }
            }

        }

        //何かが削除された時
        if (change.elementsToRemove != null)
        {
            foreach (GraphElement e in change.elementsToRemove)
            {
                //ノードが削除されたとき
                if (e is BaseNode)
                {
                    NovelEditorWindow.Instance.RecordData("Delete Node");
                    RemoveNode((BaseNode)e);
                }

                //エッジが削除されたとき
                if (e.GetType() == typeof(Edge))
                {
                    NovelEditorWindow.Instance.RecordData("Delete Edge");
                    Edge edge = (Edge)e;
                    if (edge.output.node is BaseNode && edge.input.node is BaseNode)
                    {
                        ((BaseNode)edge.output.node).RemoveNext((BaseNode)edge.input.node);
                    }
                }
            }

            Selection.activeObject = null;
        }

        //何かが動いたとき
        if (change.movedElements != null)
        {
            foreach (GraphElement e in change.movedElements)
            {
                //ノードが動いた時
                if (e is BaseNode)
                {
                    NovelEditorWindow.Instance.RecordData("Move Node");
                    BaseNode node = (BaseNode)e;
                    node.SavePosition();
                }
            }
        }

        NovelEditorWindow.Instance.saveNovelData();

        return change;
    }

    void RemoveNode(BaseNode deletedNode)
    {
        if (deletedNode is ParagraphNode)
        {
            ParagraphNode node = (ParagraphNode)deletedNode;
            RemoveEdgeToParagraph(node.Index);
            node.SetDataNA();
            //pnodeから要素を削除する
            ParagraphNode.nodes.Remove(node);
            //NovelEditorWindow.Instance.saveNovelData();
        }
        else if (deletedNode is ChoiceNode)
        {
            ChoiceNode node = (ChoiceNode)deletedNode;
            RemoveEdgeToChoice(node.Index);
            node.SetDataNA();
            //ノベルデータとcnodeから要素を削除する
            ChoiceNode.nodes.Remove(node);
            //NovelEditorWindow.Instance.saveNovelData();
        }
    }

    void RemoveEdgeToParagraph(int paraIndex)
    {
        //そのParagraphに接続しているParagraphを-1にする
        var pnode = ParagraphNode.nodes.FindAll(x => x.nextParaIndex == paraIndex);
        foreach (var node in pnode)
        {
            node.ResetNextIndex();
        }

        //そのParagraphに接続しているChoiceのデータを-1に
        var cnode = ChoiceNode.nodes.FindAll(x => x.nextParaIndex == paraIndex);
        cnode.ForEach(x => x.ResetNextIndex());
    }

    void RemoveEdgeToChoice(int index)
    {
        //そのChoiceに接続しているParagraphを-1にする
        var pn = ParagraphNode.nodes.FindAll(x => x.nextChoices.Exists(y => y == index));
        foreach (ParagraphNode p in pn)
        {
            p.ResetChoices(index);
        }
    }


    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        // Create Nodeメニュー
        if (evt.target is GraphView && nodeCreationRequest != null)
        {
            evt.menu.AppendAction("Create Node", OnContextMenuNodeCreate, DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendSeparator();
        }
        // Copyメニュー
        if ((evt.target is BaseNode))
        {            // ノードが選択されている
            evt.menu.AppendAction(            // menu選択後のAction追加
                "Copy",                        // menu名
                copy => { CopyAction(); },    // 実際に呼び出されれるアクション
                                              // canCopySelection:コピーが成功/失敗 => (メニュー活性/非活性)
                copy => (this.canCopySelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled),
                (object)null);
        }
        // Pasteメニュー
        if (evt.target is GraphView)
        {
            Vector2 position = evt.mousePosition;
            evt.menu.AppendAction(
                "Paste",
                paste =>
                {
                    // 1.貼り付け用デシリアライズ登録
                    unserializeAndPaste = new UnserializeAndPasteDelegate(
                                                (string operationName, string pasteData) =>
                    {
                        Paste(pasteData, position);
                    });
                    // 2.ペースト用コールバック呼び出し
                    PasteCallback();

                },
                paste => (this.canPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled),
                (object)null);
        }

        if (evt.target is BaseNode)
        {
            BaseNode node = (BaseNode)evt.target;
            evt.menu.AppendAction(
                "Paste",
                Paste =>
                {
                    unserializeAndPaste = new UnserializeAndPasteDelegate(
                        (string operationName, string pasteData) =>
                        {
                            PasteNode(pasteData, node);
                        }
                    );
                    PasteCallback();
                },
                Paste => (this.canPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled),
                (object)null);
        }
        // Deleteメニュー
        if (evt.target is GraphView || evt.target is Node || evt.target is Group || evt.target is Edge)
        {
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Delete", (a) => { DeleteSelectionCallback(AskUser.DontAskUser); },
                (a) => { return canDeleteSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled; });
        }
    }

    void OnContextMenuNodeCreate(DropdownMenuAction a)
    {
        RequestNodeCreation(null, -1, a.eventInfo.mousePosition);
    }

    private void RequestNodeCreation(VisualElement target, int index, Vector2 position)
    {
        if (nodeCreationRequest == null)
            return;

        var editorWindow = NovelEditorWindow.Instance;

        Vector2 screenPoint = editorWindow.position.position + position;
        nodeCreationRequest(new NodeCreationContext() { screenMousePosition = screenPoint, target = target, index = index });
    }

    private void CopyAction()
    {
        // 1.コピー用シリアライズ登録
        serializeGraphElements = new SerializeGraphElementsDelegate(Copy);

        // 2.コピー用コールバック呼び出し
        CopySelectionCallback();

    }

    string Copy(IEnumerable<GraphElement> elements)
    {
        string data = "";
        foreach (GraphElement element in elements)
        {
            GraphElement e = element;
            if (e is BaseNode)
            {
                BaseNode node = (BaseNode)e;
                data = node.SerializeData();
            }
        }
        // コピー処理 ノードをstringにシリアライズ
        return data;
    }
    void Paste(string pasteData, Vector2 position)
    {
        // ペースト処理 シリアライズからノード生成
        string[] data = pasteData.Split('|');
        if (data[0] == "choice")
        {
            NovelEditorWindow.Instance.RecordData("Paste node");
            ChoiceNode node = new ChoiceNode(position);
            node.DeserializeData(data[1]);
            AddElement(node);
        }
        else if (data[0] == "paragraph")
        {
            NovelEditorWindow.Instance.RecordData("Paste node");
            ParagraphNode node = new ParagraphNode(data[1], position);
            AddElement(node);
        }
        NovelEditorWindow.Instance.saveNovelData();
    }

    void PasteNode(string pasteData, BaseNode node)
    {
        string[] data = pasteData.Split('|');
        if (data[0] == "choice" && node is ChoiceNode)
        {
            NovelEditorWindow.Instance.RecordData("Paste node");
            node.DeserializeData(data[1]);
            NovelEditorWindow.Instance.saveNovelData();
        }
        else if (data[0] == "paragraph" && node is ParagraphNode)
        {
            NovelEditorWindow.Instance.RecordData("Paste node");
            node.DeserializeData(data[1]);
            NovelEditorWindow.Instance.saveNovelData();
        }

    }


}