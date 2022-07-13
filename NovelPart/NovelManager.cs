using System.Collections.Generic;
using UnityEngine;
using static NovelData;

[RequireComponent(typeof(NovelInputProvider))]
public class NovelManager : MonoBehaviour
{
    NovelData noveldata;
    [SerializeField] ChoiceManger choiceManger;
    [SerializeField] NovelUI novelUI;
    private AudioPlayer audioPlayer;
    private ParagraphData nowParagraph;
    private int dialogNum = 0;
    private bool choicing = false;
    private bool hideAfterPlay;
    public bool IsStop { get; private set; } = false;

    public static bool IsPlay { get; private set; } = false;

    public bool IsDisplay { get; private set; } = true;

    void Awake()
    {
        gameObject.AddComponent<AudioPlayer>();
        audioPlayer = GetComponent<AudioPlayer>();
    }

    public void SetEasy(bool IsEasy)
    {
        novelUI.SetEasyText(IsEasy);
    }
    

    public void Play(NovelData data,bool hideAfterPlay)
    {
        noveldata = data;
        dialogNum = 0;
        nowParagraph = noveldata.ParagraphList[dialogNum];
        novelUI.Initiallize(data);
        nextDialogueSet();
        this.hideAfterPlay = hideAfterPlay;
        UnPause();
        IsPlay = true;
    }

    public void SetDisplay(bool isDisplay) {
        if (isDisplay)
        {
            UnPause();
            novelUI.SetDisplay(isDisplay);
            IsDisplay = true;
        }
        else
        {
            Stop();
            novelUI.SetDisplay(isDisplay);
            IsDisplay = false;
        }
    }

    public void Stop()
    {
        if (!IsStop)
        {
            novelUI.SetStop();
            IsStop = true;
        }
    }

    public void UnPause()
    {
        if (IsStop)
        {
            novelUI.SetUnPause();
            IsStop = false;
        }
    }


    void FixedUpdate()
    {
        if (IsPlay&&!IsStop&& NovelInputProvider.Instance.Next && DialogueField.Readed)
        {
            //Paragraph再生中
            if (dialogNum < nowParagraph.dialogueList.Count)
            {
                nextDialogueSet();
            }
            //Paragraphのdialogue終了時
            else
            {
                switch (nowParagraph.next)
                {
                    //分岐
                    case Next.Choice:
                        if (!choicing)
                            choice();
                    break;

                    case Next.Continue:
                        nextParagraphSet();
                    break;

                    case Next.End:
                        end();
                    break;
                }

            }
        }
    }

    void nextDialogueSet()
    {
        //音の再生(どのタイミング?)
        audioPlayer.PlaySound(nowParagraph.dialogueList[dialogNum]);
        novelUI.SetNextDialogue(nowParagraph.dialogueList[dialogNum]);
        dialogNum++;
    }

    void nextParagraphSet()
    {
        if (nowParagraph.nextParagraphIndex == -1)
        {
            end();
        }
        else
        {
            nowParagraph = noveldata.ParagraphList[nowParagraph.nextParagraphIndex];
            dialogNum = 0;
            
            nextDialogueSet();
        }
    }

    async void choice()
    {
        if (!choicing)
        {
            choicing = true;
            //選択肢生成して待つ
            List<ChoiceData> list = new List<ChoiceData>();
            foreach (int i in nowParagraph.nextChoiceIndexes)
            {
                if (i == -1)
                    continue;
                list.Add(noveldata.ChoiceList[i]);
            }
            ChoiceData ans = await choiceManger.MakeChoice(list);
            //戻ってきた選択肢で分岐
            //-1の可能性もある
            if (ans.nextParagraphIndex == -1)
            {
                end();
            }
            else
            {
                nowParagraph = noveldata.ParagraphList[ans.nextParagraphIndex];
                dialogNum = 0;
                choicing = false;

                nextDialogueSet();
            }

        }
    }

    async void end()
    {
        if (hideAfterPlay)
        {
           await novelUI.HideUI();
        }
        IsPlay = false;
        Debug.Log("End");

    }
}
