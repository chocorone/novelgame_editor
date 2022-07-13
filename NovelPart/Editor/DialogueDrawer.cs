using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static NovelData;
using static NovelData.ParagraphData;

[CustomPropertyDrawer(typeof(Dialogue))]
internal class DialogueDrawer : PropertyDrawer
{

    [SerializeField] private Sprite[] tempSp = null;
    [SerializeField] private Sprite[] tempBack = new Sprite[1];
    int index;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // List用に1つのプロパティであることを示すためPropertyScopeで囲む
        using (new EditorGUI.PropertyScope(position, label, property))
        {
            //index
            index = property.FindPropertyRelative("index").intValue;
            int elementNum = 0;

            elementNum = BasicField(property, position, elementNum);

            elementNum = DetailOption(property, position, elementNum);

            property.FindPropertyRelative("elementNum").intValue = elementNum;
        }
    }

    int BasicField(SerializedProperty property, Rect position, int elementNum)
    {
        position.height = EditorGUIUtility.singleLineHeight;

        var nameRect = new Rect(position)
        {
            y = position.y
        };

        var Name = property.FindPropertyRelative("Name");
        Name.stringValue = EditorGUI.TextField(nameRect, "名前", Name.stringValue);

        elementNum++;

        var TextLabelRect = new Rect(position)
        {
            y = position.y + EditorGUIUtility.singleLineHeight * elementNum + 2f
        };
        EditorGUI.LabelField(TextLabelRect, "テキスト");

        elementNum++;

        var TextRect = new Rect(position)
        {
            // TextAreaなので3行分確保
            y = position.y + EditorGUIUtility.singleLineHeight * elementNum + 2f,
            height = (EditorGUIUtility.singleLineHeight * 3)
        };

        var style = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true
        };

        var talkTextProperty = property.FindPropertyRelative("text");
        talkTextProperty.stringValue = EditorGUI.TextArea(TextRect, talkTextProperty.stringValue, style);
        elementNum += 3;

        return elementNum;
    }

    int DetailOption(SerializedProperty property, Rect position, int elementNum)
    {
        //なんか開いたり閉じたりするやつ
        var open = property.FindPropertyRelative("open");

        var FoldRect = new Rect(position)
        {
            y = position.y + EditorGUIUtility.singleLineHeight * elementNum + 2f,
            height = EditorGUIUtility.singleLineHeight
        };

        open.boolValue = EditorGUI.Foldout(FoldRect, open.boolValue, "詳細設定");

        elementNum++;

        if (open.boolValue)
        {
            elementNum = CharaField(property, position, elementNum);
            elementNum = BackField(property, position, elementNum);
            elementNum = FontField(property, position, elementNum);
            elementNum = BGMField(property, position, elementNum);
            elementNum = SEField(property, position, elementNum);
            elementNum = effectField(property, position, elementNum);
            elementNum += 4;
        }
        else
        {
            SetNowChara(property);
            SetNowBack(property);
        }



        return elementNum;
    }

    int CharaField(SerializedProperty property, Rect position, int elementNum)
    {
        var howCharas = property.FindPropertyRelative("howCharas");
        var charas = property.FindPropertyRelative("charas");

        if (tempSp == null || index == 0)
        {
            tempSp = new Sprite[charas.arraySize];
            Sprite[] tmps = new Sprite[charas.arraySize];
            for (int k = 0; k < charas.arraySize; k++)
            {
                tmps[k] = (Sprite)charas.GetArrayElementAtIndex(k).objectReferenceValue;
            }
            tmps.CopyTo(tempSp, 0);
        }

        //それぞれの立ち絵の設定
        for (int i = 0; i < charas.arraySize; i++)
        {
            string ctext = "change chara" + i.ToString();
            if (NovelEditorWindow.Instance.NovelData != null)
            {
                ctext = NovelEditorWindow.Instance.NovelData.prelocations[i].name;
            }

            var charaLabelRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
                width = EditorGUIUtility.labelWidth / 2
            };


            EditorGUI.LabelField(charaLabelRect, ctext);

            var charaEnumRect = new Rect(position)
            {
                x = EditorGUIUtility.labelWidth,
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
                width = EditorGUIUtility.labelWidth / 2
            };


            howCharas.GetArrayElementAtIndex(i).enumValueIndex = (int)(CharaChangeStyle)EditorGUI.EnumPopup(charaEnumRect, (CharaChangeStyle)Enum.ToObject(typeof(CharaChangeStyle), howCharas.GetArrayElementAtIndex(i).enumValueIndex));

            var charaRect = new Rect(position)
            {
                x = EditorGUIUtility.labelWidth * 3 / 2,
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
                width = EditorGUIUtility.labelWidth
            };

            if (howCharas.GetArrayElementAtIndex(i).enumValueIndex != (int)CharaChangeStyle.UnChange)
            {

                EditorGUI.ObjectField(charaRect, charas.GetArrayElementAtIndex(i), new GUIContent(""));

                Sprite[] tmps = new Sprite[1];
                tmps[0] = (Sprite)charas.GetArrayElementAtIndex(i).objectReferenceValue;
                tmps.CopyTo(tempSp, i);
            }
            else
            {
                if (index == 0)
                {
                    tempSp[i] = null;
                }

                //Debug.Log(index);
                //Debug.Log(tempSp[i]);


                string text = "現在 : " + (tempSp[i] == null ?
                                        "None" : tempSp[i].name);

                charas.GetArrayElementAtIndex(i).objectReferenceValue = tempSp[i];

                EditorGUI.LabelField(charaRect, text);
            }

            elementNum++;

        }
        return elementNum;
    }

    void SetNowChara(SerializedProperty property)
    {
        var howCharas = property.FindPropertyRelative("howCharas");
        var charas = property.FindPropertyRelative("charas");

        //nullのとき？index==1のときのほうがよさそう
        if (tempSp == null)
        {
            tempSp = new Sprite[charas.arraySize];
            Sprite[] tmps = new Sprite[charas.arraySize];
            for (int k = 0; k < charas.arraySize; k++)
            {
                tmps[k] = (Sprite)charas.GetArrayElementAtIndex(k).objectReferenceValue;
            }
            tmps.CopyTo(tempSp, 0);
        }

        for (int i = 0; i < charas.arraySize; i++)
        {
            //キャラ変更する時
            if (howCharas.GetArrayElementAtIndex(i).enumValueIndex != (int)CharaChangeStyle.UnChange)
            {
                //そのまま代入するとシャローコピーになるので配列を使う
                Sprite[] tmps = new Sprite[1];
                tmps[0] = (Sprite)charas.GetArrayElementAtIndex(i).objectReferenceValue;
                tmps.CopyTo(tempSp, i);
            }
            else
            {
                if (index == 0)
                {
                    tempSp[i] = null;
                }

                charas.GetArrayElementAtIndex(i).objectReferenceValue = tempSp[i];
            }

        }
    }

    int BackField(SerializedProperty property, Rect position, int elementNum)
    {
        elementNum++;
        var back = property.FindPropertyRelative("back");
        var howBack = property.FindPropertyRelative("howBack");

        if (tempBack[0] == null)
        {
            tempBack = new Sprite[1];
            Sprite[] tmpb = new Sprite[1];

            tmpb[0] = (Sprite)back.objectReferenceValue;

            tmpb.CopyTo(tempBack, 0);
        }

        var backLabelRect = new Rect(position)
        {
            y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
            height = EditorGUIUtility.singleLineHeight,
            width = EditorGUIUtility.labelWidth / 2
        };


        EditorGUI.LabelField(backLabelRect, "背景");


        var backEnumRect = new Rect(position)
        {
            x = EditorGUIUtility.labelWidth,
            y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
            height = EditorGUIUtility.singleLineHeight,
            width = EditorGUIUtility.labelWidth / 2
        };


        howBack.enumValueIndex = (int)(BackChangeStyle)EditorGUI.EnumPopup(backEnumRect, (BackChangeStyle)Enum.ToObject(typeof(BackChangeStyle), howBack.enumValueIndex));

        var backRect = new Rect(backEnumRect)
        {
            x = EditorGUIUtility.labelWidth * 3 / 2,
            width = EditorGUIUtility.labelWidth
        };

        if (howBack.enumValueIndex != (int)BackChangeStyle.UnChange)
        {

            EditorGUI.ObjectField(backRect, back, new GUIContent(""));
            Sprite[] tmpb = new Sprite[1];
            tmpb[0] = (Sprite)back.objectReferenceValue;
            tmpb.CopyTo(tempBack, 0);

            if (howBack.enumValueIndex != (int)BackChangeStyle.Quick && howBack.enumValueIndex != (int)BackChangeStyle.dissolve)
            {
                elementNum++;
                var colorRect = new Rect(position)
                {
                    y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                    height = EditorGUIUtility.singleLineHeight
                };
                var FadeColor = property.FindPropertyRelative("backFadeColor");
                FadeColor.colorValue = EditorGUI.ColorField(colorRect, "    フェードの色", FadeColor.colorValue);

            }

        }
        else
        {
            if (index == 0)
            {
                tempBack[0] = null;
            }

            string text = "現在 : " + (tempBack[0] == null ?
                "None" :
                tempBack[0].name);

            back.objectReferenceValue = tempBack[0];
            EditorGUI.LabelField(backRect, text);
        }
        elementNum++;

        return elementNum;
    }

    void SetNowBack(SerializedProperty property)
    {
        var back = property.FindPropertyRelative("back");
        var howBack = property.FindPropertyRelative("howBack");

        if (tempBack[0] == null)
        {
            tempBack = new Sprite[1];
            Sprite[] tmpb = new Sprite[1];

            tmpb[0] = (Sprite)back.objectReferenceValue;

            tmpb.CopyTo(tempBack, 0);
        }

        if (howBack.enumValueIndex != (int)BackChangeStyle.UnChange)
        {
            Sprite[] tmpb = new Sprite[1];
            tmpb[0] = (Sprite)back.objectReferenceValue;
            tmpb.CopyTo(tempBack, 0);

        }
        else
        {
            if (index == 0)
            {
                tempBack[0] = null;
            }

            back.objectReferenceValue = tempBack[0];
        }
    }

    int FontField(SerializedProperty property, Rect position, int elementNum)
    {
        var changeFont = property.FindPropertyRelative("changeFont");

        elementNum++;

        var boolRect = new Rect(position)
        {
            y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
            height = EditorGUIUtility.singleLineHeight,
        };
        changeFont.boolValue = EditorGUI.Toggle(boolRect, new GUIContent("フォント変更"), changeFont.boolValue);

        elementNum++;

        if (changeFont.boolValue)
        {
            var font = property.FindPropertyRelative("font");

            var fontRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };

            EditorGUI.ObjectField(fontRect, font, new GUIContent("    テキストのフォント"));

            elementNum++;

            var fontColor = property.FindPropertyRelative("fontColor");
            var ColorRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };

            fontColor.colorValue = EditorGUI.ColorField(ColorRect, new GUIContent("    テキストのフォントの色"), fontColor.colorValue);
            elementNum++;

            var fontStyle = property.FindPropertyRelative("fontStyle");
            var styleRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };
            fontStyle.enumValueIndex = (int)(FontStyle)EditorGUI.EnumPopup(styleRect, "    テキストのフォントのスタイル", (FontStyle)Enum.ToObject(typeof(FontStyle), fontStyle.enumValueIndex));
            elementNum++;

            var fontSize = property.FindPropertyRelative("fontSize");
            var sizeRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };
            fontSize.intValue = EditorGUI.IntField(sizeRect, "    テキストのサイズ", fontSize.intValue);
            elementNum++;


            //名前の
            var nameFont = property.FindPropertyRelative("nameFont");

            var nameFontRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };

            EditorGUI.ObjectField(nameFontRect, nameFont, new GUIContent("    名前のフォント"));

            elementNum += 2;

            var nameFontColor = property.FindPropertyRelative("nameColor");
            var nameColorRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };

            nameFontColor.colorValue = EditorGUI.ColorField(nameColorRect, new GUIContent("    名前のフォントの色"), nameFontColor.colorValue);
            elementNum++;

            var nameFontStyle = property.FindPropertyRelative("nameFontStyle");
            var nameStyleRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };
            nameFontStyle.enumValueIndex = (int)(FontStyle)EditorGUI.EnumPopup(nameStyleRect, "    名前のフォントのスタイル", (FontStyle)Enum.ToObject(typeof(FontStyle), nameFontStyle.enumValueIndex));
            elementNum++;
        }

        return elementNum;
    }

    int BGMField(SerializedProperty property, Rect position, int elementNum)
    {
        elementNum++;
        var BGMStyle = property.FindPropertyRelative("BGMStyle");
        var styleRect = new Rect(position)
        {
            y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
            height = EditorGUIUtility.singleLineHeight,
        };
        BGMStyle.enumValueIndex = (int)(SoundStyle)EditorGUI.EnumPopup(styleRect, "BGM再生状態", (SoundStyle)Enum.ToObject(typeof(SoundStyle), BGMStyle.enumValueIndex));
        elementNum++;

        if (BGMStyle.enumValueIndex == (int)SoundStyle.Play)
        {
            var BGM = property.FindPropertyRelative("BGM");

            var BGMRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };

            EditorGUI.ObjectField(BGMRect, BGM, new GUIContent("    BGM"));

            elementNum++;

            var Fade = property.FindPropertyRelative("BGMFade");
            var FadeRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
                width = EditorGUIUtility.labelWidth
            };
            Fade.boolValue = EditorGUI.Toggle(FadeRect, "    開始フェードあり", Fade.boolValue);
            elementNum++;
            if (Fade.boolValue)
            {

                var FadeTime = property.FindPropertyRelative("BGMFadeTime");
                var TimeRect = new Rect(position)
                {
                    y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                    height = EditorGUIUtility.singleLineHeight,
                };
                FadeTime.floatValue = EditorGUI.FloatField(TimeRect, "        フェード時間", FadeTime.floatValue);
                elementNum++;
            }

            var LoopStyle = property.FindPropertyRelative("BGMLoop");
            var LoopRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };
            LoopStyle.enumValueIndex = (int)(LoopMode)EditorGUI.EnumPopup(LoopRect, "    ループ設定", (LoopMode)Enum.ToObject(typeof(LoopMode), LoopStyle.enumValueIndex));
            elementNum++;

            if (LoopStyle.enumValueIndex == (int)LoopMode.Count)
            {
                var Count = property.FindPropertyRelative("BGMCount");
                var CountRect = new Rect(position)
                {
                    y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                    height = EditorGUIUtility.singleLineHeight,
                };
                Count.intValue = EditorGUI.IntField(CountRect, "    ループ回数", Count.intValue);
                elementNum++;
            }
            else if (LoopStyle.enumValueIndex == (int)LoopMode.Second)
            {
                var Second = property.FindPropertyRelative("BGMSecond");
                var SecondRect = new Rect(position)
                {
                    y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                    height = EditorGUIUtility.singleLineHeight,
                };
                Second.floatValue = EditorGUI.FloatField(SecondRect, "再生時間", Second.floatValue);
                elementNum++;
            }

            if (LoopStyle.enumValueIndex != (int)LoopMode.Endless)
            {
                var EndFade = property.FindPropertyRelative("BGMEndFade");
                var EndFadeRect = new Rect(position)
                {
                    y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                    height = EditorGUIUtility.singleLineHeight,
                    width = EditorGUIUtility.labelWidth
                };
                EndFade.boolValue = EditorGUI.Toggle(EndFadeRect, "    終了フェードあり", EndFade.boolValue);
                elementNum++;
                if (EndFade.boolValue)
                {

                    var FadeEndTime = property.FindPropertyRelative("BGMEndFadeTime");
                    var TimeRect = new Rect(position)
                    {
                        y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                        height = EditorGUIUtility.singleLineHeight,
                    };
                    FadeEndTime.floatValue = EditorGUI.FloatField(TimeRect, "        終了時フェード時間", FadeEndTime.floatValue);
                    elementNum++;
                }
            }

        }
        else if (BGMStyle.enumValueIndex == (int)SoundStyle.Stop)
        {
            var Fade = property.FindPropertyRelative("BGMFade");
            var FadeRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
                width = EditorGUIUtility.labelWidth
            };
            Fade.boolValue = EditorGUI.Toggle(FadeRect, "フェードあり", Fade.boolValue);

            if (Fade.boolValue)
            {
                elementNum++;
                var FadeTime = property.FindPropertyRelative("BGMFadeTime");
                var TimeRect = new Rect(position)
                {
                    y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                    height = EditorGUIUtility.singleLineHeight,
                };
                FadeTime.floatValue = EditorGUI.FloatField(TimeRect, "    フェード時間", FadeTime.floatValue);
            }
        }

        return elementNum;
    }

    int SEField(SerializedProperty property, Rect position, int elementNum)
    {
        elementNum++;
        var SEStyle = property.FindPropertyRelative("SEStyle");
        var styleRect = new Rect(position)
        {
            y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
            height = EditorGUIUtility.singleLineHeight,
        };
        SEStyle.enumValueIndex = (int)(SoundStyle)EditorGUI.EnumPopup(styleRect, "SE再生状態", (SoundStyle)Enum.ToObject(typeof(SoundStyle), SEStyle.enumValueIndex));
        elementNum++;

        if (SEStyle.enumValueIndex == (int)SoundStyle.Play)
        {
            var SE = property.FindPropertyRelative("SE");

            var SERect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };

            EditorGUI.ObjectField(SERect, SE, new GUIContent("    SE"));

            elementNum++;

            var Fade = property.FindPropertyRelative("SEFade");

            var FadeRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
                width = EditorGUIUtility.labelWidth
            };
            Fade.boolValue = EditorGUI.Toggle(FadeRect, "    開始フェードあり", Fade.boolValue);
            elementNum++;
            if (Fade.boolValue)
            {

                var FadeTime = property.FindPropertyRelative("SEFadeTime");
                var TimeRect = new Rect(position)
                {
                    y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                    height = EditorGUIUtility.singleLineHeight,
                };
                FadeTime.floatValue = EditorGUI.FloatField(TimeRect, "        フェード時間", FadeTime.floatValue);
                elementNum++;
            }

            var LoopStyle = property.FindPropertyRelative("SELoop");
            var LoopRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };
            LoopStyle.enumValueIndex = (int)(LoopMode)EditorGUI.EnumPopup(LoopRect, "    ループ設定", (LoopMode)Enum.ToObject(typeof(LoopMode), LoopStyle.enumValueIndex));
            elementNum++;

            if (LoopStyle.enumValueIndex == (int)LoopMode.Count)
            {
                var Count = property.FindPropertyRelative("SECount");
                var CountRect = new Rect(position)
                {
                    y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                    height = EditorGUIUtility.singleLineHeight,
                };
                Count.intValue = EditorGUI.IntField(CountRect, "    ループ回数", Count.intValue);
                elementNum++;
            }
            else if (LoopStyle.enumValueIndex == (int)LoopMode.Second)
            {
                var Second = property.FindPropertyRelative("SESecond");
                var SecondRect = new Rect(position)
                {
                    y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                    height = EditorGUIUtility.singleLineHeight,
                };
                Second.floatValue = EditorGUI.FloatField(SecondRect, "    再生時間", Second.floatValue);
                elementNum++;
            }
            if (LoopStyle.enumValueIndex != (int)LoopMode.Endless)
            {
                var EndFade = property.FindPropertyRelative("SEEndFade");
                var EndFadeRect = new Rect(position)
                {
                    y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                    height = EditorGUIUtility.singleLineHeight,
                    width = EditorGUIUtility.labelWidth
                };
                EndFade.boolValue = EditorGUI.Toggle(EndFadeRect, "    終了フェードあり", EndFade.boolValue);
                elementNum++;
                if (EndFade.boolValue)
                {

                    var FadeEndTime = property.FindPropertyRelative("SEEndFadeTime");
                    var TimeRect = new Rect(position)
                    {
                        y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                        height = EditorGUIUtility.singleLineHeight,
                    };
                    FadeEndTime.floatValue = EditorGUI.FloatField(TimeRect, "        終了時フェード時間", FadeEndTime.floatValue);
                    elementNum++;
                }
            }
        }
        else if (SEStyle.enumValueIndex == (int)SoundStyle.Stop)
        {
            var Fade = property.FindPropertyRelative("SEFade");
            var FadeRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
                width = EditorGUIUtility.labelWidth
            };
            Fade.boolValue = EditorGUI.Toggle(FadeRect, "フェードあり", Fade.boolValue);

            if (Fade.boolValue)
            {
                elementNum++;
                var FadeTime = property.FindPropertyRelative("SEFadeTime");
                var TimeRect = new Rect(position)
                {
                    y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                    height = EditorGUIUtility.singleLineHeight,
                };
                FadeTime.floatValue = EditorGUI.FloatField(TimeRect, "    フェード時間", FadeTime.floatValue);
            }
        }
        return elementNum;
    }

    int effectField2(SerializedProperty property, Rect position, int elementNum)
    {
        elementNum++;
        var backEffect = property.FindPropertyRelative("backEffect");
        var backEffectRect = new Rect(position)
        {
            y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
            height = EditorGUIUtility.singleLineHeight,
        };
        backEffect.enumValueIndex = (int)(Effect)EditorGUI.EnumPopup(backEffectRect, "背景のエフェクト", (Effect)Enum.ToObject(typeof(Effect), backEffect.enumValueIndex));

        elementNum++;
        var dialogueEffect = property.FindPropertyRelative("dialogueEffect");
        var dialogueEffectRect = new Rect(position)
        {
            y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
            height = EditorGUIUtility.singleLineHeight,
        };
        dialogueEffect.enumValueIndex = (int)(Effect)EditorGUI.EnumPopup(dialogueEffectRect, "セリフ枠のエフェクト", (Effect)Enum.ToObject(typeof(Effect), dialogueEffect.enumValueIndex));

        var charaEffects = property.FindPropertyRelative("charaEffects");
        for (int i = 0; i < charaEffects.arraySize; i++)
        {
            elementNum++;
            var charaRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (i + elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };
        }
        return elementNum;
    }

    int effectField(SerializedProperty property, Rect position, int elementNum)
    {
        elementNum++;
        var effect = property.FindPropertyRelative("effect");
        var effectRect = new Rect(position)
        {
            y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
            height = EditorGUIUtility.singleLineHeight,
        };
        effect.enumValueIndex = (int)(Effect)EditorGUI.EnumPopup(effectRect, "エフェクト", (Effect)Enum.ToObject(typeof(Effect), effect.enumValueIndex));
        elementNum++;
        if (effect.enumValueIndex != (int)Effect.None && effect.enumValueIndex != (int)Effect.UnChange)
        {
            var strength = property.FindPropertyRelative("effectStrength");
            var strengthRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };

            strength.intValue = EditorGUI.IntSlider(strengthRect, "    エフェクトの強さ", strength.intValue, 1, 10);
            elementNum++;

            var adopt = property.FindPropertyRelative("adopt");
            var adoptRect = new Rect(position)
            {
                y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                height = EditorGUIUtility.singleLineHeight,
            };
            int bit = (int)(Adopt)EditorGUI.EnumFlagsField(adoptRect, "    エフェクトの適用", (Adopt)Enum.ToObject(typeof(Adopt), adopt.intValue));

            adopt.intValue = bit;

            elementNum++;

            if ((bit & (int)Adopt.Character) != 0)
            {
                var labelRect = new Rect(position)
                {
                    y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                    height = EditorGUIUtility.singleLineHeight,
                };
                EditorGUI.LabelField(labelRect, "エフェクトを適用するキャラ");

                elementNum++;

                var charas = property.FindPropertyRelative("charaEffect");

                for (int i = 0; i < charas.arraySize; i++)
                {
                    var charaRect = new Rect(position)
                    {
                        y = position.y + (EditorGUIUtility.singleLineHeight + 2f) * (elementNum),
                        height = EditorGUIUtility.singleLineHeight,
                    };
                    string ctext = "chara" + (i + 1);
                    if (NovelEditorWindow.Instance.NovelData != null)
                    {
                        ctext = NovelEditorWindow.Instance.NovelData.prelocations[i].name;
                    }
                    charas.GetArrayElementAtIndex(i).boolValue = EditorGUI.ToggleLeft(charaRect, ctext, charas.GetArrayElementAtIndex(i).boolValue);
                    elementNum++;
                }


            }

        }
        return elementNum;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var height = EditorGUIUtility.singleLineHeight;
        int elementNum = property.FindPropertyRelative("elementNum").intValue;
        height *= elementNum;

        return height;
    }
}
