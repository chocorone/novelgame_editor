using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskMoving : MonoBehaviour
{
    [SerializeField]Canvas canvas;
    //キャンバス内のレクトトランスフォーム
    RectTransform canvasRect;
    //マウスの位置の最終的な格納先
    Vector2 MousePos;

    Image Mouse_Image;

    void Start()
    {
        //canvas内にあるRectTransformをcanvasRectに入れる
        canvasRect = canvas.GetComponent<RectTransform>();

        //Canvas内にあるMouseImageを探してMouse_Imageに入れる
        Mouse_Image = GetComponent<Image>();
    }

    void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,
                Input.mousePosition, canvas.worldCamera, out MousePos);

        Mouse_Image.GetComponent<RectTransform>().anchoredPosition
             = new Vector2(MousePos.x, MousePos.y);
    }
}
