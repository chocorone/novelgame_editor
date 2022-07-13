using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static NovelData;

internal class ChoiceManger : MonoBehaviour
{
    [SerializeField]ChoiceButton button;
    private ChoiceData sendData;
    private bool returned = false;



    internal async UniTask<ChoiceData> MakeChoice(List<ChoiceData> datas)
    {
        List<ChoiceButton> buttons = new List<ChoiceButton>();
        //�{�^�������
        foreach(ChoiceData data in datas)
        {
            ChoiceButton obj = Instantiate(button, transform) ;
            buttons.Add(obj);
            obj.transform.SetParent(transform);
            obj.setChoice(data,this);
        }

        //������҂�
        await UniTask.WaitUntil(() => returned);
        returned = false;

        //�{�^����
        foreach(ChoiceButton b in buttons)
        {
            Destroy(b.gameObject);
        }

        return sendData;
    }

    internal void returnChoice(ChoiceData data)
    {
        sendData = data;
        returned = true;
    }

}
