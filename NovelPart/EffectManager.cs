using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static NovelData.ParagraphData;

public class EffectManager : MonoBehaviour
{
    [SerializeField] Shader defaultShader;
    [SerializeField] Shader noiseShader;
    [SerializeField] Shader mosaicSahder;
    [SerializeField] Shader sepiaShader;
    [SerializeField] Shader grayShader;
    [SerializeField] Shader jaggyShader;
    [SerializeField] Shader holoShader;
    [SerializeField] Shader choromaticShader;
    [SerializeField] Shader blurShader;


    Image backImage;
    Image dialogueImage;
    List<Image> charaImage;


    private void Awake()
    {
    }

    internal void SetEffect(Dialogue data)
    {
        switch (data.effect)
        {
            case Effect.None:
                SetMaterial(data.adopt, defaultShader,data.charaEffect);
            break;

            case Effect.Noise:
                SetMaterial(data.adopt, noiseShader, data.charaEffect);
            break;

            case Effect.Mosaic:
                SetMaterial(data.adopt, mosaicSahder, data.charaEffect);
            break;

            case Effect.Sepia:
                SetMaterial(data.adopt, sepiaShader, data.charaEffect);
            break;

            case Effect.GrayScale:
                SetMaterial(data.adopt, grayShader, data.charaEffect);
            break;

            case Effect.Jaggy:
                SetMaterial(data.adopt, jaggyShader, data.charaEffect);
            break;

            case Effect.Holo:
                SetMaterial(data.adopt, holoShader, data.charaEffect);
                break;

            case Effect.ChromaticAberration:
                SetMaterial(data.adopt, choromaticShader, data.charaEffect);
                break;
            case Effect.Blur:
                SetMaterial(data.adopt, blurShader, data.charaEffect);
                break;

        }
    }

    internal void Initiallize(Image back,Image dialogue)
    {
        backImage = back;
        back.material = new Material(defaultShader);
        dialogueImage = dialogue;
        dialogueImage.material = new Material(defaultShader);

    }

    internal void SetCharaMaterial(List<Image> materials)
    {
        charaImage = materials;
        foreach (Image img in charaImage)
        {

            img.material = new Material(defaultShader);

        }
    }


    void SetMaterial(Adopt adopt,Shader shader,bool[] chara)
    {
        if (adopt.HasFlag(Adopt.Character))
        {
            for(int i = 0; i < chara.Length; i++)
            {
                if (chara[i])
                {
                    charaImage[i].material.shader = shader;
                }
                else
                {
                    charaImage[i].material.shader = defaultShader;
                }

            }
        }
        else
        {
            foreach (Image img in charaImage)
                img.material.shader = defaultShader;
        }

        if (adopt.HasFlag(Adopt.BackGround))
        {
            backImage.material.shader = shader;
        }
        else
        {
            backImage.material.shader = defaultShader;
        }

        if (adopt.HasFlag(Adopt.DialogueField))
        {
            dialogueImage.material.shader = shader;
        }
        else
        {
            dialogueImage.material.shader = defaultShader;
        }
    }
}
