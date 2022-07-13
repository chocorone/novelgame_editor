using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static NovelData.ParagraphData;

//テキストや名前を表示
internal class DialogueField : MonoBehaviour
{
    [SerializeField] Sprite nonameSprite;
    [SerializeField] Sprite nameSprite;
    [SerializeField] Text text;
    [SerializeField] Text nameText;

    private int textSpeed = 6;
    private static bool readed = false;
    private Image image;

    private int fadeSpeed = 3;
    private Font defaultFont;
    private int defaultFontSize;
    private FontStyle defaultStyle;
    private Color defaultColor;
    private Font defaultNameFont;
    private FontStyle defaultNameStyle;
    private Color defaultNameColor;
    internal bool IsEasy { get; set; } = false;
    internal bool IsStop = false;

    internal static bool Readed
    {
        get
        {
            bool r = readed;
            readed = false;
            return r;
        }
    }




    private void Awake()
    {
        image = GetComponent<Image>();
        defaultColor = text.color;
        defaultFont = text.font;
        defaultFontSize = text.fontSize;
        defaultStyle = text.fontStyle;

        defaultNameColor = nameText.color;
        defaultNameFont = nameText.font;
        defaultNameStyle = nameText.fontStyle;
    }

    internal async void ResetDialogue()
    {
        text.text = "";
        nameText.text = "";

        text.color = defaultColor;
        text.fontSize = defaultFontSize;
        text.fontStyle = defaultStyle;
        text.font = defaultFont;

        Color color = text.color;
        color.a = 1;
        text.color = color;
        color = nameText.color;
        color.a = 1;
        nameText.color = color;


        float alpha = 0;
        Color fadeColor = image.color;
        fadeColor.a = 0;
        image.color = fadeColor;

        while (alpha < 1)
        {
            fadeColor.a = alpha;
            image.color = fadeColor;
            await UniTask.Delay(fadeSpeed);
            alpha += 0.05f;
        }
        fadeColor.a = 1;
        image.color = fadeColor;
    }

    internal async void dialogueFade()
    {

        float alpha = 1;
        Color fadeColor = image.color;
        Color textColor = text.color;
        Color nameColor = nameText.color;
        fadeColor.a = alpha;
        textColor.a = alpha;
        nameColor.a = alpha;

        while (alpha >0)
        {
            fadeColor.a = alpha;
            textColor.a = alpha;
            nameColor.a = alpha;
            image.color = fadeColor;
            text.color = textColor;
            nameText.color = nameColor;

            await UniTask.Delay(fadeSpeed);
            alpha -= 0.05f;
        }
        fadeColor.a = 0;
        image.color = fadeColor;
    }

    internal void dialogueSet(Dialogue data)
    {
        SetFont(data);
        //テキストを設定する
        SetText(data.text);
        if(data.Name=="")
        {
            SetNameFrame(false);
        }
        else
        {
            SetNameFrame(true, data.Name);
        }
    }

    private async void SetText(string dialogue)
    {
        string[] splitTexts = dialogue.Split('`');
        text.text = "";
        bool red = false;

        if (IsEasy&&dialogue[0] == '`')
        {
            red = true;
        }

        for (int i = 0; i < splitTexts.Length; i++)
        {
            int wordcnt = 0;

            while (wordcnt < splitTexts[i].Length)
            {
                await UniTask.Delay(textSpeed * 10);

                if (red)
                {
                    text.text += "<color=#ff0000>";
                    text.text += splitTexts[i][wordcnt];
                    text.text += "</color>";
                }
                else
                {
                    text.text += splitTexts[i][wordcnt];
                }
                if (IsStop)
                {
                    await UniTask.WaitUntil(() => !IsStop);
                }
                wordcnt++;
            }
            if(IsEasy)
                red = !red;
        }


        readed = true;
    }

    private void SetFont(Dialogue data)
    {
        if (data.changeFont)
        {
            text.color = data.fontColor;
            text.fontSize = data.fontSize;
            text.fontStyle = data.fontStyle;
            if (data.font != null)
            {
                text.font = data.font;
            }
            nameText.color = data.nameColor;
            nameText.fontStyle = data.nameFontStyle;
            if (data.nameFont != null){
                nameText.font = data.nameFont;
            }

        }
        else{
            text.color = defaultColor;
            text.fontSize = defaultFontSize;
            text.fontStyle = defaultStyle;
            text.font = defaultFont;

            nameText.color = defaultNameColor;
            nameText.fontStyle = defaultNameStyle;
            nameText.font = defaultNameFont;
        }
    }

    private void SetNameFrame(bool flag,string name="")
    {
        if (flag)
        {
            image.sprite = nameSprite;
            nameText.text = name;
        }
        else
        {
            image.sprite = nonameSprite;
            nameText.text = "";
        }
    }

}
