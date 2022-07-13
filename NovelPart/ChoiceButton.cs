using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static NovelData;

internal class ChoiceButton : MonoBehaviour
{
    [HideInInspector]internal ChoiceData data;
    private ChoiceManger manager;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(pressed);
    }
    internal ChoiceManger Manger {
        set
        {
            manager = value;
        }
    
    }


    internal void setChoice(ChoiceData d,ChoiceManger m)
    {
        data = d;

        GetComponentInChildren<Text>().text = d.text;
        manager = m;
    }


    internal void pressed()
    {
        manager.returnChoice(data);
    }
}
