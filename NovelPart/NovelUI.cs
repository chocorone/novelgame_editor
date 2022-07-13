using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static NovelData.ParagraphData;

[RequireComponent(typeof(CanvasGroup))]
internal class NovelUI : MonoBehaviour
{
    [SerializeField] DialogueField dialogueField;
    [SerializeField] Image BackGround;
    DialogueImage dialogueImage;
    CanvasGroup NovelCanvas;
    EffectManager effectManager;

    void Awake()
    {
        NovelCanvas = GetComponent<CanvasGroup>();
        gameObject.AddComponent<DialogueImage>();
        dialogueImage = GetComponent<DialogueImage>();
        dialogueImage.Initiallize(BackGround);

        gameObject.AddComponent<EffectManager>();
        effectManager = GetComponent<EffectManager>();
        effectManager.Initiallize(BackGround,dialogueField.GetComponent<Image>());
    }

    internal void Initiallize(NovelData data)
    {
        dialogueImage.CharaLocationInitialize(data.locations.ToArray());
        effectManager.SetCharaMaterial(dialogueImage.locations);
        dialogueField.ResetDialogue();
        NovelCanvas.alpha = 1;
        NovelCanvas.interactable = true;
    }
    internal async void SetNextDialogue(Dialogue data)
    {
        await dialogueImage.SetImage(data);
        effectManager.SetEffect(data);
        dialogueField.dialogueSet(data);
    }

    internal async UniTask<bool> HideUI() 
    {
        dialogueField.dialogueFade();
        await dialogueImage.returnEscape();
        NovelCanvas.alpha = 0;
        NovelCanvas.interactable = false;
        return true;
    }

    internal void SetEasyText(bool flag)
    {
        dialogueField.IsEasy = flag;
    }

    internal void SetStop()
    {
        dialogueField.IsStop = true;
        Debug.Log("すとっぷ");
    }

    internal void SetUnPause()
    {
        dialogueField.IsStop = false;
        Debug.Log("再開");
    }

    internal void SetDisplay(bool display)
    {
        if (display)
        {
            NovelCanvas.alpha = 1;
            NovelCanvas.interactable = true;

        }
        else
        {
            NovelCanvas.alpha = 0;
            NovelCanvas.interactable = false;
        }
    }


}
