using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using static NovelData;
using static NovelData.ParagraphData;

[CustomEditor(typeof(NovelData))]
internal class NovelDataInspector : Editor
{
    NovelData noveldata;
    string tmp = "";
    string effectTmp = "";

    void OnEnable()
    {
        noveldata = target as NovelData;

    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        //立ち位置の追加
        if (GUILayout.Button("プレハブをセットしたら押す　同じ名前なら引き継がれます"))
        {
            //ここどうにかしたい
            ResetLocations();
        }

        GUILayout.Space(10);


        if (GUILayout.Button("Open"))
        {
            open();
        }


        serializedObject.ApplyModifiedProperties();
    }

    void open()
    {
        if (noveldata.newData)
        {
            noveldata.Reset();

        }

        NovelEditorWindow.Open(noveldata);

    }

    void ResetLocations()
    {
        serializedObject.Update();

        if (noveldata.newData)
        {
            noveldata.Reset();
        }

        if (noveldata.havePreLocations)
        {
            //先にテキストファイルに書き出す
            saveTemp();
            noveldata.ParagraphList.ForEach(p => p.dialogueList.ForEach(d =>
            {
                d.charas = new Sprite[noveldata.locations.Count];
                d.howCharas = new CharaChangeStyle[noveldata.locations.Count];
                d.charaEffect = new bool[noveldata.locations.Count];
            }));
            sameImageCopy();
        }
        else
        {
            noveldata.ParagraphList.ForEach(p => p.dialogueList.ForEach(d =>
            {
                d.charas = new Sprite[noveldata.locations.Count];
                d.howCharas = new CharaChangeStyle[noveldata.locations.Count];
                d.charaEffect = new bool[noveldata.locations.Count];
            }));

        }
        noveldata.prelocations = new List<Image>(noveldata.locations);
        noveldata.havePreLocations = true;
        EditorGUIUtility.systemCopyBuffer = null;
        serializedObject.ApplyModifiedProperties();
    }

    void sameImageCopy()
    {
        string[] s = tmp.Split('/');
        string[][] save = new string[s.Length][];
        for (int i = 0; i < s.Length; i++)
        {
            string[] ssplit = s[i].Split(',');
            save[i] = new string[ssplit.Length];
            for (int j = 0; j < ssplit.Length; j++)
            {
                save[i][j] = ssplit[j];
            }

        }

        string[] s2 = effectTmp.Split('/');
        string[][] save2 = new string[s2.Length][];
        for (int i = 0; i < s2.Length; i++)
        {
            string[] ssplit = s2[i].Split(',');
            save2[i] = new string[ssplit.Length];
            for (int j = 0; j < ssplit.Length; j++)
            {
                save2[i][j] = ssplit[j];
            }

        }

        for (int nowIndex = 0; nowIndex < noveldata.locations.Count; nowIndex++)
        {
            for (int preIndex = 0; preIndex < noveldata.prelocations.Count; preIndex++)
            {
                //同じのがあれば
                if (noveldata.locations[nowIndex].name == noveldata.prelocations[preIndex].name)
                {
                    //すべてのParagraphについて
                    for (int i = 0; i < noveldata.ParagraphList.Count; i++)
                    {
                        if (noveldata.ParagraphList[i].index == -1)
                        {
                            continue;
                        }
                        for (int j = 0; j < noveldata.ParagraphList[i].dialogueList.Count; j++)
                        {
                            //テキストファイルから復元
                            int e = int.Parse(save[i][preIndex + j * 2 * noveldata.prelocations.Count]);

                            noveldata.ParagraphList[i].dialogueList[j].howCharas[nowIndex] = (CharaChangeStyle)e;


                            int ID = int.Parse(save[i][preIndex + (j * 2 + 1) * noveldata.prelocations.Count]);

                            if (ID != 0)
                            {
                                Sprite sprite = (Sprite)EditorUtility.InstanceIDToObject(ID);
                                noveldata.ParagraphList[i].dialogueList[j].charas[nowIndex] = sprite;
                            }
                            else
                            {
                                noveldata.ParagraphList[i].dialogueList[j].charas[nowIndex] = null;
                            }

                            if (save2[i][preIndex + j * noveldata.prelocations.Count] == "True")
                            {
                                noveldata.ParagraphList[i].dialogueList[j].charaEffect[nowIndex] = true;
                            }
                            else
                            {
                                noveldata.ParagraphList[i].dialogueList[j].charaEffect[nowIndex] = false;
                            }

                        }
                    }

                }
            }
        }
    }

    void saveTemp()
    {
        tmp = "";
        effectTmp = "";
        for (int j = 0; j < noveldata.ParagraphList.Count; j++)
        {
            foreach (Dialogue d in noveldata.ParagraphList[j].dialogueList)
            {
                for (int i = 0; i < d.howCharas.Length; i++)
                {
                    tmp += (int)d.howCharas[i] + ",";
                }

                Sprite[] sprite = new Sprite[d.charas.Length];
                d.charas.CopyTo(sprite, 0);
                for (int i = 0; i < d.charas.Length; i++)
                {

                    tmp += sprite[i].GetInstanceID().ToString();

                    tmp += ",";
                }

                for (int i = 0; i < d.charaEffect.Length; i++)
                {

                    effectTmp += d.charaEffect[i];

                    effectTmp += ",";
                }

            }
            tmp = tmp.TrimEnd(',');
            tmp += "/";

            effectTmp = effectTmp.TrimEnd(',');
            effectTmp += "/";

        }
        tmp = tmp.TrimEnd('/');
        effectTmp = effectTmp.TrimEnd(',');
    }



}
