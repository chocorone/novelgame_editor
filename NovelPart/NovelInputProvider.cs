using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//しんぐるとん
//すべての入力を管理する、配布するときはどうしよう?
public class NovelInputProvider : SingletonMonoBehaviour<NovelInputProvider>
{
    public bool Next {
        get
        {
            bool rvalue = next;
            next = false;
            return rvalue;

        }
    }
    private bool next = false;
    private NovelManager novelManager;

    protected override void Awake()
    {
        base.Awake();
        novelManager = GetComponent<NovelManager>();
    }

    void Update()
    {
        //nextの
        if (Input.GetKeyDown(KeyCode.Return)||Input.GetMouseButton(0))
        {
            next = true;
        }
        else
        {
            next = false;    
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            novelManager.SetDisplay(!novelManager.IsDisplay);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (novelManager.IsStop)
            {
                novelManager.UnPause();
            }
            else
            {
                novelManager.Stop();
            }
        } 
    }
}
