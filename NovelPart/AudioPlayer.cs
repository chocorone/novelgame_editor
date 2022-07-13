using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using static NovelData.ParagraphData;

internal class AudioPlayer : MonoBehaviour
{
    [SerializeField] AudioMixer mixiser;
    //音を再生するためのもの
    private AudioSource SE;
    private AudioSource BGM;
    CancellationTokenSource BGMcancellationToken;
    CancellationToken BGMtoken;
    CancellationTokenSource SEcancellationToken;
    CancellationToken SEtoken;

    void Start()
    {
        GameObject SEObj = new GameObject("SESource");
        SEObj.AddComponent<AudioSource>();
        SE = SEObj.GetComponent<AudioSource>();
        SE.playOnAwake = false;
        SE.outputAudioMixerGroup = mixiser.FindMatchingGroups("SE")[0];
        SEObj.transform.SetParent(transform);

        GameObject BGMObj = new GameObject("BGMSource");
        BGMObj.AddComponent<AudioSource>();
        BGM = BGMObj.GetComponent<AudioSource>();
        BGM.playOnAwake = false;
        BGMObj.transform.SetParent(transform);
        BGM.outputAudioMixerGroup = mixiser.FindMatchingGroups("BGM")[0];
    }

    internal void PlaySound(Dialogue data)
    {
        if(data.BGMStyle != SoundStyle.UnChange)
        {
            SetBGM(data);
        }

        if(data.SEStyle != SoundStyle.UnChange)
        {
            SetSE(data);
        }
    }

    async void SetBGM(Dialogue data)
    {

        if (data.BGMStyle == SoundStyle.Play)
        {
            if(BGMcancellationToken!=null)
            {
                BGMcancellationToken.Cancel();
            }
            BGM.Stop();

            BGM.clip = data.BGM;

            if(data.BGMLoop == LoopMode.Endless)
            {
                if (data.BGMFade)
                {
                    FadeIn(BGM, data.BGMFadeTime);
                }
                BGM.Play();
                BGM.loop = true;
            }

            BGMcancellationToken = new CancellationTokenSource();
            BGMtoken = BGMcancellationToken.Token;
            if (data.BGMLoop == LoopMode.Second)
            {
                if (data.BGMFade)
                {
                    FadeIn(BGM, data.BGMFadeTime);
                }
                BGM.Play();
                BGM.loop = true;
                try { 
                    //キャンセル処理が必要
                    if (data.BGMEndFade)
                    {
                        await UniTask.Delay((int)((data.BGMSecond-data.BGMEndFadeTime) * 1000), cancellationToken: BGMtoken);
                        FadeOut(BGM, data.BGMEndFadeTime);
                    }
                    else
                    {
                        await UniTask.Delay((int)(data.BGMSecond * 1000), cancellationToken: BGMtoken);
                        BGM.Stop();
                    }
                }
                catch (OperationCanceledException)
                {
                }

            }

            if(data.BGMLoop == LoopMode.Count)
            {
                BGM.loop = false;
                for (int i = 0; i < data.BGMCount; i++)
                {
                    //初回
                    if (i == 0&&data.BGMFade)
                    {
                        FadeIn(BGM, data.BGMFadeTime);
                    }
                    BGM.Play();

                    try {
                        //キャンセル処理が必要
                        if (i == data.BGMCount - 1 && data.BGMEndFade)
                        {
                            await UniTask.Delay((int)((data.BGM.length - data.BGMEndFadeTime) * 1000), cancellationToken: BGMtoken);
                            FadeOut(BGM, data.BGMEndFadeTime);
                        }
                        else
                        {
                            await UniTask.WaitUntil(() => !BGM.isPlaying, cancellationToken: BGMtoken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }

            }
            

        }else if (data.BGMStyle == SoundStyle.Stop)
        {
            if (BGMcancellationToken != null)
            {
                BGMcancellationToken.Cancel();
            }

            if (data.BGMFade)
            {
                FadeOut(BGM, data.BGMFadeTime);
            }
            else
            {
                BGM.Stop();
            }

        }
    }

    async void SetSE(Dialogue data)
    {

        if (data.SEStyle == SoundStyle.Play)
        {
            if (SEcancellationToken != null)
            {
                SEcancellationToken.Cancel();
            }

            SE.Stop();
            SE.clip = data.SE;

            if (data.SELoop == LoopMode.Endless)
            {
                if (data.SEFade)
                {
                    FadeIn(SE, data.SEFadeTime);
                }
                SE.Play();
                SE.loop = true;
            }

            SEcancellationToken = new CancellationTokenSource();
            SEtoken = SEcancellationToken.Token;
            if (data.SELoop == LoopMode.Second)
            {
                if (data.SEFade)
                {
                    FadeIn(SE, data.SEFadeTime);
                }
                SE.Play();
                SE.loop = true;
                try
                {
                    //キャンセル処理が必要
                    if (data.SEEndFade)
                    {
                        await UniTask.Delay((int)((data.SESecond - data.SEEndFadeTime) * 1000), cancellationToken: SEtoken);
                        FadeOut(SE, data.SEEndFadeTime);
                    }
                    else
                    {
                        await UniTask.Delay((int)(data.SESecond * 1000), cancellationToken: SEtoken);
                        SE.Stop();
                    }
                }
                catch (OperationCanceledException)
                {
                }

            }

            if (data.SELoop == LoopMode.Count)
            {
                SE.loop = false;
                for (int i = 0; i < data.SECount; i++)
                {
                    //初回
                    if (i == 0 && data.SEFade)
                    {
                        FadeIn(SE, data.SEFadeTime);
                    }
                    SE.Play();

                    try
                    {
                        //キャンセル処理が必要
                        if (i == data.SECount - 1 && data.SEEndFade)
                        {
                            await UniTask.Delay((int)((data.SE.length - data.SEEndFadeTime) * 1000), cancellationToken: SEtoken);
                            FadeOut(SE, data.SEEndFadeTime);
                        }
                        else
                        {
                            await UniTask.WaitUntil(() => !SE.isPlaying, cancellationToken: SEtoken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }

            }


        }
        else if (data.SEStyle == SoundStyle.Stop)
        {
            if (SEcancellationToken != null)
            {
                SEcancellationToken.Cancel();
            }
            if (data.SEFade)
            {
                FadeOut(SE, data.SEFadeTime);
            }
            else
            {
                SE.Stop();
            }

        }
    }

    async void FadeIn(AudioSource source,float time)
    {
        source.volume = 0;
        while (source.volume < 1)
        {
            await UniTask.Delay(1);
            source.volume += 1 / (time * 1000);
        }
    }

    async void FadeOut(AudioSource source, float time)
    {
        try
        {
            while (source.volume > 0)
            {
                await UniTask.Delay(1, cancellationToken: SEtoken);
                source.volume -= 1 / (time * 1000);
            }
        }
        catch (OperationCanceledException)
        {

        }
        source.Stop();
        source.volume = 1;

    }
}
