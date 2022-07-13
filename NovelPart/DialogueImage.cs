using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static NovelData;
using static NovelData.ParagraphData;

internal class DialogueImage : MonoBehaviour
{

    [SerializeField] int backFadeSpeed = 10;
    [SerializeField] int charaFadeSpeed = 8;
    Image BackGroud;
    GameObject CharaSet;
    Image DisBack;
    Image AllFadePanel;


    Dialogue data;
    internal List<Image> locations { get; private set; } = new List<Image>();

    internal void Initiallize(Image backGroud)
    {
        BackGroud = backGroud;

        DisBack = Instantiate(BackGroud, BackGroud.transform);
        DisBack.name = "BackFade";
        DisBack.transform.SetParent(BackGroud.transform);
        DisBack.enabled = false;

        AllFadePanel = Instantiate(DisBack, transform);
        AllFadePanel.name = "FadePanel";
        AllFadePanel.sprite = null;
        RectTransform panelRect = AllFadePanel.GetComponent<RectTransform>();
        panelRect.SetAnchor(AnchorPresets.StretchAll);
        panelRect.localScale = new Vector3(1.1f, 1.1f, 1);
        AllFadePanel.transform.SetParent(transform);
        AllFadePanel.enabled = false;


        CharaSet = Instantiate(AllFadePanel, transform).gameObject;
        CharaSet.name = "CharaSet";
        CharaSet.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        CharaSet.transform.SetParent(transform);
        CharaSet.transform.SetSiblingIndex(BackGroud.transform.GetSiblingIndex()+1);
    }

    internal void CharaLocationInitialize(Image[] newloc)
    {
        for (int i=0;i<locations.Count;i++)
        {
            Destroy(locations[i].gameObject);
        }

        locations = new List<Image>();

        foreach (Image i in newloc)
        {
            Image obj = Instantiate(i, CharaSet.transform);
            locations.Add(obj);
            obj.transform.SetParent(CharaSet.transform);
            obj.enabled = false;
        }
    }

    internal async UniTask<bool> SetImage(Dialogue d)
    {
        data = d;
        Image panel;
        switch (d.howBack) {
            case BackChangeStyle.UnChange:
                await SetChara();
            break;

            case BackChangeStyle.FadeBack:
                panel = Instantiate(AllFadePanel, DisBack.transform);
                panel.transform.SetParent(DisBack.transform);
                panel.enabled = false;
                await BackFadeOut(panel);
                QuickSet(BackGroud, data.back);
                await BackFadeIn(panel);
                await SetChara();
                break;

            case BackChangeStyle.FadeFront:
                panel = Instantiate(AllFadePanel, CharaSet.transform);
                panel.transform.SetParent(CharaSet.transform);
                panel.enabled = false;
                await BackFadeOut(panel);
                await SetChara();
                QuickSet(BackGroud, data.back);
                await BackFadeIn(panel);
                break;

            case BackChangeStyle.FadeAll:
                panel = AllFadePanel;
                await BackFadeOut(panel);
                await SetChara();
                QuickSet(BackGroud, data.back);
                await BackFadeIn(panel);
                break;

            case BackChangeStyle.dissolve:
                await BackDisslve();
                await SetChara();

            break;
            case BackChangeStyle.Quick:
                QuickSet(BackGroud, data.back);
                await SetChara();
            break;
        }

        return true;
    }


    internal async UniTask<bool> returnEscape()
    {
        foreach (Image i in locations)
        {
            i.enabled = false;
        }
        float alpha = 1;
        Color fadeColor = Color.white;

        while (alpha >0)
        {
            fadeColor.a = alpha;

            BackGroud.color = fadeColor;

            await UniTask.Delay(backFadeSpeed);
            alpha -= 0.01f;
        }

        BackGroud.sprite = null;
        BackGroud.enabled = false;
        BackGroud.color = Color.white;

        return true;
    }

    async UniTask<bool> SetChara()
    {
        List<Image> disIm = new List<Image>();
        List<Sprite> disSp = new List<Sprite>();
        for(int i=0;i<data.charas.Length;i++)
        {
            if (data.howCharas[i] == CharaChangeStyle.dissolve)
            {
                disIm.Add(locations[i]);
                disSp.Add(data.charas[i]);
            }
        }

        if (disIm.Count > 0)
        {
            await CharaDisslve(disIm, disSp);
            return true;
        }

        for(int i = 0;i<data.charas.Length;i++)
        {
            switch (data.howCharas[i]) 
            {
                case CharaChangeStyle.Quick:
                    QuickSet(locations[i], data.charas[i]);
                break;
            }
        }

        return true;
    }


    private async UniTask<bool> BackFadeOut(Image fpanel)
    {
        float alpha = 0;
        Color fadeColor = data.backFadeColor;
        fpanel.color = fadeColor;
        fpanel.enabled = true;
        while (alpha<1)
        {
            fadeColor.a = alpha;
            fpanel.color = fadeColor;
            await UniTask.Delay(backFadeSpeed);
            alpha += 0.01f;
        }
        fadeColor.a = 1;
        fpanel.color=fadeColor;
        return true;
    }

    private async UniTask<bool> BackFadeIn(Image fpanel)
    {
        float alpha = 1;
        Color fadeColor = fpanel.color;
        fpanel.color = fadeColor;
        while (alpha >0)
        {
            fadeColor.a = alpha;
            fpanel.color = fadeColor;
            await UniTask.Delay(backFadeSpeed);
            alpha -= 0.01f;
        }
        fadeColor.a = 0;
        fpanel.color = fadeColor;
        fpanel.enabled = false;
        return true;
    }

    private async UniTask<bool> CharaDisslve(List<Image> img,List<Sprite> sprites)
    {
        float alpha = 1;
        Color fadeColor = Color.white;
        while (alpha > 0)
        {
            fadeColor.a = alpha;
            foreach (Image image in img)
            {
                //image.color = fadeColor;
                image.material.color = fadeColor;
            }
            await UniTask.Delay(charaFadeSpeed);
            alpha -= 0.1f;
        }

        alpha = 0;
        for (int i=0;i<img.Count; i++)
        {
            QuickSet(img[i], sprites[i]);
        }


        while (alpha < 1)
        {
            fadeColor.a = alpha;
            foreach (Image image in img)
            {
                //image.color = fadeColor;
                image.material.color = fadeColor;
            }
            await UniTask.Delay(charaFadeSpeed);
            alpha += 0.1f;
        }

        foreach (Image image in img)
        {
            //image.color = Color.white;
            image.material.color = Color.white;
        }

        return true;
    }


    private async UniTask<bool> BackDisslve()
    {
        float alpha = 0;
        Color fadeColor = Color.white;
        fadeColor.a = alpha;
        DisBack.color = fadeColor;
        DisBack.sprite = data.back;
        DisBack.enabled = true;

        while (alpha < 1)
        {
            fadeColor.a = alpha;

            DisBack.color = fadeColor;

            await UniTask.Delay(backFadeSpeed);
            alpha += 0.01f;
        }
        fadeColor.a = 1;
        DisBack.color = fadeColor;

        if (data.back == null)
        {
            BackGroud.enabled = false;
        }
        else
        {
            BackGroud.enabled = true;
            BackGroud.sprite = data.back;
        }
        DisBack.enabled = false;

        return true;
    }

    private void QuickSet(Image image,Sprite sprite)
    {
        if (sprite== null)
        {
            image.enabled = false;
        }
        else
        {
            image.enabled = true;
        }
        image.sprite = sprite;
    }

}
