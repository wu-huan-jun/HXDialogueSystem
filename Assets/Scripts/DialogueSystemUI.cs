using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.Rendering;

[Serializable]
public class NamedImageList
{
    /// <summary>
    /// 带名字的Image（UI元素Image）列表，用于对应Image引用和定义名，作用与字典类似但可以在Inspector里修改
    /// </summary>
    [Serializable]
    public class NamedImage
    {
        public string dictionaryName;
        public Image image;
        public NamedImage() { }
        public NamedImage(string name, Image image)
        {
            this.dictionaryName = name;
            this.image = image;
        }
    }
    public List<NamedImage> imgs;
    public NamedImageList()
    {
        imgs = new List<NamedImage>();
    }
    public Image GetImageByDictionaryName(string name)
    {
        foreach (NamedImage namedImage in imgs)
        {
            if (namedImage.dictionaryName == name)
                return namedImage.image;
        }
        return null;
    }
}




///<summary>
///可扩展的DialogueSystemUI，配合DialogueSystem使用以显示对话
///</summary>
public class DialogueSystemUI : MyUI
{

    [Header("DialogueSystem 默认UI组件")]
    public Dialogue liveDialogue;
    public bool showingIenmRunning;
    [SerializeField] private Content content;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Text sentenceText;
    [SerializeField] private string sentence;
    [SerializeField] private Text nameText;
    [SerializeField] private Button continueButton;
    [SerializeField] private bool showReplyInSentenceBox;//是否在选择选择肢后，在语句框里复述选项中的文字
    [SerializeField] private AudioSource audioSource;
    private bool isShowingSelectedReply;
    private int lastSelectedButtonIndex;
    [SerializeField] private CanvasGroup replyBox;//回复框
    [SerializeField] private GameObject refReplyButton;

    [Header("可选项")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private float backgroundImgFadeDuration = .5f;
    [SerializeField] private NamedImageList floatingImgs = new NamedImageList();
    [SerializeField] private bool globalSearchFloatingImgName = false;
    //[SerializeField] private SerializedDictionary<string, Image> floatingImgs = new SerializedDictionary<string, Image>();


    private void Start()
    {
        group = GetComponent<CanvasGroup>();
        if (group == null)
            group = this.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0;
        FadeIn(group, .8f);

        sentenceText.text = "";
        continueButton.onClick.AddListener(OnContinueButtonClick);
        // StartCoroutine(LateStart());

        //隐藏默认按钮
        refReplyButton.transform.parent = transform;
        refReplyButton.gameObject.SetActive(false);
        StartDialogue(liveDialogue);
    }

    public void StartDialogue(Dialogue Dialogue)
    {
        ///<summary>
        ///DialogueSystem在创建UI实例时调用
        ///</summary>
        liveDialogue = Dialogue;
        Content content = liveDialogue.GetLiveContent();
        StartCoroutine(ShowContent(content));
    }
    public void OnSelectionButtonClick(int buttonIndex)
    {
        ///<summary>
        ///定义了当选择肢按钮按下时的行为，默认为开启对应的下一句话
        ///</summary>
        Debug.Log($"SelectionButton{buttonIndex}Clicked");
        if (!showReplyInSentenceBox)
            MoveToNextContent(buttonIndex);
        else
        {
            FadeOut(replyBox, .3f);
            StartCoroutine(WriteSentenceText(sentenceText, content.contentSentence[buttonIndex]));

            if (audioSource != null)
            {
                if (content.clips[buttonIndex] != null)
                {
                    audioSource.clip = content.clips[buttonIndex];
                    audioSource.Play();
                }
            }
            lastSelectedButtonIndex = buttonIndex;
            isShowingSelectedReply = true;
        }
    }
    void MoveToNextContent(int selectionIndex)
    {
        ///<summary>
        ///根据选择肢的选择，跳转下一句话
        ///selectionIndex是选中的回复按钮的Index，记录在按钮上所附的DialogueButton.index上
        ///targetContentIndex 是 content.nextContentIndex[selectionIndex]，也就是下一句话的IndexIndex
        ///</summary>
        int targetContentIndex = 0;//下一句话的编号，-1时结束对话
        //找到选项对应的目标Content编号
        if (selectionIndex < content.contentSentence.Count)
        {
            targetContentIndex = content.nextContentIndex[selectionIndex];
        }
        else
        {
            Debug.LogWarning("selectionIndex超出了List<int> content.nextContentIndex的长度，读取中止");
            return;
        }
        if (targetContentIndex == -1)//遇到-1则结束对话
        {
            StartCoroutine(EndDialogue());
            Debug.Log("对话结束");
            return;
        }

        // Debug.Log($"这句话：Content{content.contentIndex}的下一句话：Content{targetContentIndex}.");
        //根据目标Content编号找到目标Content
        bool contentMatchToTargetIndexExist = false;
        foreach (Content content1 in liveDialogue.contents)
        {
            if (content1.contentIndex == targetContentIndex)
            {
                contentMatchToTargetIndexExist = true;
                liveDialogue.liveContentIndex = content1.contentIndex;
                content = content1;
                break;
            }
        }
        if (!contentMatchToTargetIndexExist)
        {
            Debug.LogError("在liveDialogue.contents中找不到contentIndex为" + targetContentIndex + "的Content");
            return;
        }
        StartCoroutine(ShowContent(content));
        //将新的Content替换到UI上
    }
    IEnumerator ShowContent(Content content)
    {
        ///<summary>
        ///将Content中的文本和图片填入UI中
        ///</summary>
        showingIenmRunning = true;
        Debug.Log($"正在展示content{content.contentIndex},contentSelective:{content.selective}");

        audioSource.Pause();
        yield return null;
        //非选项部分
        if (content.backGroundSprite != null)//读取并替换背景图
        {
            yield return StartCoroutine(ReplaceImg(backgroundImage, content.backGroundSprite, backgroundImgFadeDuration));
        }

        if (content.namedSprites != null && content.namedSprites.namedSprites != null)//读取并替换浮动图
        {
            NamedSpriteList namedSprites = content.namedSprites;
            foreach (NamedSpriteList.NamedSprite namedSprite in namedSprites.namedSprites)
            {
                Debug.Log($"试图在floatingImg中找到对应于{namedSprite.dictionaryName}的Image对象……");
                Image floatingimg = floatingImgs.GetImageByDictionaryName(namedSprite.dictionaryName);
                if (floatingimg != null)
                {
                    // floatingimg.sprite = namedSprite.sprite;
                    yield return StartCoroutine(ReplaceImg(floatingimg, namedSprite.sprite, .3f));
                    Debug.Log("……成功在NamedImages中找到了对应对象！");
                }
                else if (globalSearchFloatingImgName)
                {
                    Debug.Log("……无法在NamedImages中找到对应对象，尝试在全局查找……");
                    GameObject imgObject = GameObject.Find(namedSprite.dictionaryName);
                    if (imgObject != null)
                    {
                        Image image = imgObject.GetComponent<Image>();
                        if (image == null)
                        {
                            image = imgObject.AddComponent<Image>();
                        }
                        yield return StartCoroutine(ReplaceImg(image, namedSprite.sprite, .3f));
                        // image.sprite = namedSprite.sprite;
                        Debug.Log("……成功！");
                    }
                    else
                    {
                        Debug.LogWarning("……失败！");
                    }
                }
                else
                {
                    Debug.LogWarning("……失败！");
                }
            }
        }

        nameText.text = content.speaker;//替换说话人姓名

        //选项部分
        if (!content.selective)
        {
            // sentenceText.text = content.contentSentence[0];
            sentence = content.contentSentence[0];

            if (audioSource != null)
            {
                audioSource.Pause();
                if (content.clips!=null && content.clips[0] != null)
                {
                    audioSource.clip = content.clips[0];
                    audioSource.Play();
                }
            }
            StartCoroutine(WriteSentenceText(sentenceText, sentence));
            FadeOut(replyBox, 0.3f);
        }
        else
        {
            Debug.Log($"Content{content.contentIndex} is Selective");
            FadeIn(replyBox, 0.3f);
            // for (int p = 0; p < replyButtons.Length; p++)
            // {
            //     Destroy(replyButtons[p]);
            // }
            // replyButtons = new DialogueButton[content.GetSelectionCount()];
            for (int i = 0; i < content.GetSelectionCount(); i++)
            {
                Button button = Instantiate(refReplyButton, replyBox.transform).GetComponent<Button>();
                button.gameObject.SetActive(true);
                DialogueButton replyButton = button.AddComponent<DialogueButton>();
                button.onClick.AddListener(replyButton.OnButtonClick);
                replyButton.buttonIndex = i;
                replyButton.dialogueSystemUI = this;
                // replyButtons[i] = replyButton;//目前其实没有用

                Text text = button.transform.GetChild(0).GetComponent<Text>();
                text.text = content.contentSentence[i];
            }
            sentence = "……";
            StartCoroutine(WriteSentenceText(sentenceText, sentence));
        }
        showingIenmRunning = false;
        OnDispalyContent(content);
    }

    IEnumerator EndDialogue()
    {
        ///<summary>
        ///定义了结束对话的行为
        ///</summary>
        yield return StartCoroutine(HandleFadeOut(group, .8f, 0));
        Destroy(this.gameObject);
    }
    void OnContinueButtonClick()
    {
        ///<summary>
        ///定义了按下【继续对话】按钮的行为
        ///</summary>
        Debug.Log("ContinueButtonClick");
        if (isTypeWriting)
        {
            StopAllCoroutines();
            sentenceText.text = sentence;
            isTypeWriting = false;
        }//打字状态下再次点击continuebutton直接显示整句话
        else
        {
            content = liveDialogue.GetLiveContent();
            if (isShowingSelectedReply)
            {
                isShowingSelectedReply = false;
                MoveToNextContent(lastSelectedButtonIndex);
                return;
            }
            if (!content.selective)
                MoveToNextContent(0);
            else Debug.LogWarning($"当前Content:Content{content.contentIndex}有选择肢，ContinueButton无效");
        }
    }

    //可以自由修改的协程
    IEnumerator WriteSentenceText(Text sentenceText, string sentence)
    {
        /// <summary>
        /// 在这个协程中修改文本淡入效果，默认是调用MyUI的打字机效果淡入
        /// </summary>
        yield return StartCoroutine(TypeWrite(sentenceText, sentence, 0.03f));
    }
    IEnumerator ReplaceImg(Image image, Sprite source, float duration)
    {
        ///<summary>
        ///用于替换图片的方法，先复制图片Object，将拷贝的sprite替换后，淡入拷贝
        ///淡入完成后，替换原图片的sprite，删除拷贝
        ///</summary>

        if (image != null && source != null)
        {
            Debug.Log($"正在替换Image{image}的sprite为{source}");
            CanvasGroup imageCanvasGroup = image.GetComponent<CanvasGroup>();
            if (imageCanvasGroup == null)
                imageCanvasGroup = image.gameObject.AddComponent<CanvasGroup>();
            Image newImage = Instantiate(image.gameObject, image.transform.parent).GetComponent<Image>();//复制背景图
            AspectRatioFitter newfitter = newImage.gameObject.GetComponent<AspectRatioFitter>();
            if (newfitter != null)
                newfitter.aspectRatio = source.rect.width / source.rect.height;
            newImage.sprite = source;
            CanvasGroup newImageCanvasGroup = newImage.GetComponent<CanvasGroup>();
            newImageCanvasGroup.alpha = 0;
            // yield return StartCoroutine(HandleFadeOut(newImageCanvasGroup, .01f, 0));
            yield return StartCoroutine(HandleFadeIn(newImageCanvasGroup, duration, 0));
            yield return StartCoroutine(HandleFadeOut(imageCanvasGroup, duration, 0));
            image.sprite = source;
            AspectRatioFitter fitter = image.gameObject.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.aspectRatio = source.rect.width / source.rect.height;
            imageCanvasGroup.alpha = 1;
            Destroy(newImage.gameObject);
        }
    }

    //公共接口
    public void OnDispalyContent(Content content) { }
    public void OnDialogueEnd() { }
}