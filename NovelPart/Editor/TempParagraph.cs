using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static NovelData;
using static NovelData.ParagraphData;

//ParagraphDataを仮で表示するためのもの
internal class TempParagraph : ScriptableObject
{
    [HideInInspector] public ParagraphData data;
}
