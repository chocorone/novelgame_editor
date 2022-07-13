using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;


public enum Next
{
    Continue,
    Choice,
    End
}

public enum CharaChangeStyle
{
    UnChange,
    Quick,
    dissolve
}

public enum BackChangeStyle
{
    UnChange,
    Quick,
    FadeAll,
    FadeFront,
    FadeBack,
    dissolve
}

public enum SoundStyle
{
    UnChange,
    Play,
    Stop
}

public enum LoopMode
{
    Endless,
    Count,
    Second,
}

public enum Effect
{
    UnChange,
    None,
    Noise,
    Mosaic,
    GrayScale,
    Sepia,
    Jaggy,
    Holo,
    ChromaticAberration,
    Blur
}

[Flags]
public enum Adopt
{
    DialogueField = 0x001,
    Character = 0x002,
    BackGround = 0x004,
}

[CreateAssetMenu(menuName = "Scriptable/Create NovelData")]
public class NovelData : ScriptableObject
{
    public bool newData
    {
        get
        {
            return ParagraphList.Count == 0;
        }
    }
    [SerializeField, HideInInspector] internal float graphZoom;

    [SerializeField, Tooltip("位置を決めてプレハブ化したImageを入れる")]
    public List<Image> locations = new List<Image>();
    [SerializeField, HideInInspector]
    internal List<Image> prelocations = new List<Image>();
    [SerializeField, HideInInspector]
    internal bool havePreLocations = false;

    [SerializeField, HideInInspector]
    public List<ParagraphData> ParagraphList = new List<ParagraphData>();

    [SerializeField, HideInInspector]
    public List<ChoiceData> ChoiceList = new List<ChoiceData>();

    [SerializeField, HideInInspector]
    internal int MaxChoiceCnt = 0;

    [SerializeField, HideInInspector]
    internal int MaxParagraphID = 1;

    public void Reset()
    {
        ParagraphData pdata = new ParagraphData();
        pdata.dialogueList.Add(new ParagraphData.Dialogue());
        pdata.dialogueList[0].text = "FirstParagraph";
        pdata.index = 0;
        pdata.nextChoiceIndexes.Add(-1);
        ParagraphList.Add(pdata);
    }

    public ParagraphData CreateParagraph()
    {
        ParagraphData data = new ParagraphData();
        data.index = MaxParagraphID;
        MaxParagraphID++;
        data.dialogueList.Add(new ParagraphData.Dialogue());
        data.dialogueList[0].text = "Paragraph";
        data.dialogueList[0].charas = new Sprite[locations.Count];
        data.dialogueList[0].howCharas = new CharaChangeStyle[locations.Count];
        data.nextChoiceIndexes.Add(-1);
        ParagraphList.Add(data);
        return data;
    }

    public ParagraphData CreateParagraphFromJson(string sdata)
    {
        ParagraphData data = JsonUtility.FromJson<ParagraphData>(sdata);
        data.index = MaxParagraphID;
        MaxParagraphID++;
        data.nextChoiceIndexes = new List<int>();
        data.nextChoiceIndexes.Add(-1);
        data.nextParagraphIndex = -1;
        ParagraphList.Add(data);
        return data;
    }

    public ChoiceData CreateChoice()
    {
        ChoiceData data = new ChoiceData();
        data.text = "Choice";
        data.index = MaxChoiceCnt;
        MaxChoiceCnt++;
        ChoiceList.Add(data);
        return data;
    }


    [System.SerializableAttribute]
    public class ChoiceData
    {
        public string text;
        //[HideInInspector] public int ID;
        [HideInInspector] public int index;
        [HideInInspector] public int nextParagraphIndex = -1;
        [HideInInspector] public Rect choicePosition;
    }


    [System.SerializableAttribute]
    public class ParagraphData
    {
        public List<Dialogue> dialogueList = new List<Dialogue>();

        [HideInInspector] public int index;

        //ノードの位置
        [SerializeField, HideInInspector]
        public Rect ParagraphPosition;

        public bool detailOpen = false;

        public Next next = Next.End;
        [SerializeField, HideInInspector]
        public int nextParagraphIndex = -1;

        //次のポート番号,choiceID
        [SerializeField, HideInInspector]
        public List<int> nextChoiceIndexes = new List<int>();

        //会話文ごとのデータ
        [System.SerializableAttribute]
        public class Dialogue
        {
            public int index = 0;
            public bool open;
            public int elementNum = 7;

            [SerializeField] public CharaChangeStyle[] howCharas;
            [SerializeField] public Sprite[] charas;

            public Sprite back;
            public BackChangeStyle howBack;
            public Color backFadeColor = Color.white;

            public string Name = "";
            [TextArea(1, 6)]
            public string text;

            public bool changeFont = false;
            public Font font;
            public Color fontColor = Color.white;
            public FontStyle fontStyle;
            public int fontSize = 30;

            public Font nameFont;
            public Color nameColor = Color.white;
            public FontStyle nameFontStyle;


            public AudioClip BGM;
            public SoundStyle BGMStyle;
            public LoopMode BGMLoop;
            public int BGMCount = 1;
            public float BGMSecond = 20;
            public bool BGMFade = false;
            public float BGMFadeTime = 3;
            public bool BGMEndFade = false;
            public float BGMEndFadeTime = 3;

            public AudioClip SE;
            public SoundStyle SEStyle;
            public LoopMode SELoop;
            public int SECount = 1;
            public float SESecond = 20;
            public bool SEFade = false;
            public float SEFadeTime = 3;
            public bool SEEndFade = false;
            public float SEEndFadeTime = 3;

            public Effect effect;
            public int effectStrength = 5;
            public Adopt adopt;
            public bool[] charaEffect;

            public Effect backEffect;
            public int backEffectStrength;
            public Effect[] charaEffects;
            public int[] charaEffectStrength;
            public Effect dialogueEffect;
            public int dialogueFieldEffectStrength;


        }

    }

}
