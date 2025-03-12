using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NPOI;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using Newtonsoft;
using Newtonsoft.Json;
using UnityEngine.Rendering;

[Serializable]
public class NamedSpriteList
{
    /// <summary>
    /// 带名字的Sprite（图片Sprite）列表，用于对应Sprite引用和定义名，作用与字典类似但可以在Inspector里修改
    /// </summary>
    [Serializable]
    public class NamedSprite
    {
        public string dictionaryName;
        public Sprite sprite;
        public NamedSprite() { }
        public NamedSprite(string name, Sprite sprite)
        {
            this.dictionaryName = name;
            this.sprite = sprite;
        }
        public float Getaspect()
        {
            return sprite.rect.width / sprite.rect.height;
        }
    }
    public List<NamedSprite> namedSprites;
    public NamedSpriteList()
    {
        namedSprites = new List<NamedSprite>();
    }
    public Sprite GetImageByDictionaryName(string name)
    {
        foreach (NamedSprite namedSprite in namedSprites)
        {
            if (namedSprite.dictionaryName == name)
                return namedSprite.sprite;
        }
        return null;
    }
    public int AddNamedSprite(string name, Sprite sprite)
    {
        namedSprites.Add(new NamedSprite(name, sprite));
        return namedSprites.Count;
    }
}
[Serializable]
public class Dialogue
{
    ///<summary>
    ///对话类
    ///<summary>
    public int sceneIndex;//活动场景编号
    public int DialogueIndex;//对话编号
    public int liveContentIndex = 0;//活动的句子
    public List<Content> contents = new List<Content>();//句子列表
    public Dictionary<string, string> speakerNames;//对话人名列表
    public Dialogue(int sceneIndex, int DialogueIndex)
    {
        this.sceneIndex = sceneIndex;
        this.DialogueIndex = DialogueIndex;
    }
    public Dialogue() { liveContentIndex = 1; }
    public Dialogue Copy()
    {
        Dialogue copy = new Dialogue();
        copy.sceneIndex = sceneIndex;
        copy.DialogueIndex = DialogueIndex;
        copy.liveContentIndex = liveContentIndex;
        copy.contents = new List<Content>();
        for (int i = 0; i < contents.Count; i++)
        {
            copy.contents.Add(contents[i]);
        }
        return copy;
    }
    public int GetContentCount()
    { return contents.Count; }
    public void AddContent(Content content)
    { contents.Add(content); }
    public Content GetLiveContent()
    {
        Debug.Log($"访问contents{liveContentIndex}");
        return GetContentByIndex(liveContentIndex);
    }
    public Content GetContentByIndex(int index)
    {
        foreach (Content content in contents)
        {
            if (content.contentIndex == liveContentIndex)
            {
                return content;
            }
        }
        Debug.LogWarning($"Index为{index}的Content不存在！");
        return null;
    }


}
[Serializable]
public class Content
{
    ///<summary>
    ///对话中的某一句话
    ///若非必要，请勿对Content进行写操作，因为Dialogue的Content[]在Dialogue复制时并不会自我复制，
    ///因此即使是修改Dialogue拷贝中的Content，也是有风险的。
    ///<summary>
    public int contentIndex;
    public string speaker;//说话人
    // public Vector3 cameraPosition;//移动相机到位置，为空则不变
    // public Quaternion cameraRotation; //移动相机到旋转，为空则不变
    public string animationClipName;
    public Sprite backGroundSprite;//背景图
    // public SerializedDictionary<string, Sprite> floatingImgNameToSpriteKeys;//浮动图GameObject名对应Sprite字典
    public NamedSpriteList namedSprites;

    //选择肢相关
    public bool selective;//是否先弹出选择框，一般用于多选内容
    public int GetSelectionCount()
    { return contentSentence.Count; }
    public List<string> contentSentence = new List<string>(0);//句子的内容，
    //                                若单选，这个列表就只有一个元素。
    //                                若多选，这个列表有≥一个元素
    public List<int> nextContentIndex = new List<int>(0);//为-1则结束对话
    public List<string> command;

    public Content()
    {
        selective = false;
        //其他初始化
    }
}
public class DialogueSystem : MonoBehaviour
{
    //单例
    private static DialogueSystem _instance;
    public static DialogueSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                // 尝试在场景中找到现有的实例
                _instance = FindObjectOfType<DialogueSystem>();

                if (_instance == null)
                {
                    // 如果没有实例，则动态创建一个新对象
                    GameObject singletonObject = new GameObject("DialogueSystem");
                    _instance = singletonObject.AddComponent<DialogueSystem>();
                }
            }
            return _instance;
        }
    }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // 保持跨场景的持久性
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // 如果已经有实例存在，销毁多余的实例
        }
    }
    //单例






    [Header("DialogueFile")]
    [SerializeField] private DialogueSystemUI UIPrefab;
    [SerializeField] private string path; //
    public Dialogue[] dialogues;
    [Header("UI")]
    DialogueSystemUI dialogueSystemUI; //

    void Start()
    {
        // Dictionary<string, string> keys = new Dictionary<string, string>();
        // keys["0"] = "path1";
        // keys["1"] = "path2";
        // string json = JsonConvert.SerializeObject(keys);
        // Debug.Log(json);
        // Dictionary<string, string> keys1 = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        // Debug.Log(keys1.Values);
        // foreach (KeyValuePair<string, string> kvp in keys1)
        // {
        //     Debug.Log($"Key = {kvp.Key}, Value = {kvp.Value}");
        // }
    }
    public void Save()
    {
    }
    public void LoadFromSave()
    {
    }
    public void TriggerDialogue(int dialogueIndex)//按钮只能调用含≤1个参的公共函数
    {
        TriggerDialogue(dialogueIndex, true);
    }
    public void TriggerDialogue(int dialogueIndex, bool deliverCopy)
    {
        ///<summary>
        ///触发一段对话的方法
        ///实例化一个DialogueSystemUI Prefab
        ///并将此scene下对应index的dialogue传给UI实例
        ///</summary>
        foreach (Dialogue Dialogue in dialogues)
        {
            if (Dialogue.sceneIndex == SceneManager.GetActiveScene().buildIndex && Dialogue.DialogueIndex == dialogueIndex)
            {
                Debug.Log($"开始对话{dialogueIndex}");
                //TODO 实例化对话UIprefab
                dialogueSystemUI = Instantiate(UIPrefab);
                if (deliverCopy)
                    dialogueSystemUI.liveDialogue = Dialogue.Copy();//传给DialogueUI的Dialogue是一个副本，这样在UI修改dialogue.liveContentIndex时就不会更改原来的dialogue了
                else
                    dialogueSystemUI.liveDialogue = Dialogue;//直接传入Dialogue，这样在UI修改dialogue.liveContentIndex时就更改原来的dialogue了
                return;
            }
        }
        Debug.LogWarning($"当前场景下不存在编号为{dialogueIndex}的对话，考虑检查此场景是否已经添加到build列表？");
    }
    public void ReloadAlldialoguesFromFile()
    {
        ///<summary>
        ///从文件夹加载所有对话文本
        ///<summary>
#if UNITY_EDITOR

        string[] filePaths = Directory.GetFiles(path, "*.xlsx");

        dialogues = new Dialogue[filePaths.Length];
        Debug.Log($"从{path}读取了{filePaths.Length}个xlsx文件");
        for (int i = 0; i < filePaths.Length; i++)
        {
            dialogues[i] = loadDialogueFromFile(filePaths[i]);
        }
#endif
    }
    public static Dialogue loadDialogueFromFile(string filePath)//从文件加载对话文本
    {

        ///<summary>
        ///从xlsx文件读取对话的静态方法
        ///依赖NOPI和NewtonSoft库，若不使用xlsx存储对话，且不想安装NOPI和NewtonSoft，请注释掉函数体并return null
        ///目前仅用在Editor中生效,请在加载完成后复制DialogueSystem组件，退出PlayMode并重新粘贴DialogueSystem组件
        ///<summary>
        Debug.LogWarning("从表格读取Dialogue的功能目前仅用在Editor中生效,\n请在读取完成后复制DialogueSystem组件，\n退出PlayMode并重新粘贴DialogueSystem组件");
        Dialogue dialogue = new Dialogue();
        string path = filePath;
        if (!File.Exists(path))
        {
            Debug.LogWarning($"文件{path}不存在！");
            return null;
        }
        //读取excel，需要通过filestream读取excel，然后文件流对象作为参数传给workbook构造函数参数
        FileStream fs_read = File.OpenRead(path);

        //将文件流中的eexcel文件数据读取到workbook对象中
        IWorkbook wbk = new XSSFWorkbook(fs_read);

        //获取wbksheet数量
        int sheetCnt = wbk.NumberOfSheets;
        //激活第一个sheet
        wbk.SetActiveSheet(0);

        //获取sheet对象
        ISheet sh = wbk.GetSheetAt(0);
        //获取最后一行的行index
        int rowCnt = sh.LastRowNum;
        List<string> lst = new List<string>();
        Dictionary<string, int> headerRowNameToIndexDictionary = new Dictionary<string, int>();



        //读取表格的第二行，即表头部分
        IRow currRow = sh.GetRow(1);
        int cellCnt = currRow.LastCellNum;//获取当前行的单元格数量，注意，这个数字是列数，不是最后一个单元格索引
        for (int k = 0; k < cellCnt; k++)
        {
            if (currRow.GetCell(k) != null)
            {
                string headerName = currRow.GetCell(k).ToString();
                headerRowNameToIndexDictionary[headerName] = k;
                Debug.Log($"表头第{k}列的列名为{headerName}");
            }
            else
            {
                Debug.LogWarning("表头缺失！读取终止！");
                return null;
            }
        }

        //读取第三行的sceneIndex和dialogueIndex
        int SceneBuildIndex = -2;
        int dialogueIndex = -2;
        currRow = sh.GetRow(2);
        int p = headerRowNameToIndexDictionary["SceneBuildIndex"];
        if (currRow.GetCell(p) != null && currRow.GetCell(p).ToString() != "")
        {
            SceneBuildIndex = int.Parse(currRow.GetCell(p).ToString());
        }
        p = headerRowNameToIndexDictionary["DialogueIndex"];
        if (currRow.GetCell(p) != null && currRow.GetCell(p).ToString() != "")
        {
            dialogueIndex = int.Parse(currRow.GetCell(p).ToString());
        }

        if (dialogueIndex != -2 && SceneBuildIndex != -2)
        {
            Debug.Log($"SceneBuildIndex: {SceneBuildIndex},DialogueIndex: {dialogueIndex}");
            dialogue.DialogueIndex = dialogueIndex;
            dialogue.sceneIndex = SceneBuildIndex;
        }
        else
        {
            Debug.LogWarning("表格缺失SceneBuildIndex或者DialogueIndex！读取终止！");
            return null;
        }



        //从表格的第三(Index==2)行开始读取主要的对话数据
        for (int i = 2; i <= rowCnt; i++)
        {
            //当前行赋值
            currRow = sh.GetRow(i);
            Content content = new Content();
            int selectionCount = 1;
            string cellString;

            //读取ContentIndex
            p = headerRowNameToIndexDictionary["ContentIndex"];
            if (currRow.GetCell(p) != null && currRow.GetCell(p).ToString() != "")
            {
                cellString = currRow.GetCell(p).ToString();
                content.contentIndex = int.Parse(cellString);
            }
            else
            {
                Debug.LogWarning($"因为缺失ContentIndex,跳过第{i + 1}行的读取");
                continue;
            }

            //读取Speaker
            p = headerRowNameToIndexDictionary["Speaker"];
            if (currRow.GetCell(p) != null && currRow.GetCell(p).ToString() != "")
            {
                cellString = currRow.GetCell(p).ToString();
                content.speaker = cellString;
            }
            else
            {
                content.speaker = "";
                Debug.LogWarning($"{i + 1}行没有Speaker,请确认是否为旁白");
                // continue;
            }

            //读取BackgroundImg
            p = headerRowNameToIndexDictionary["BackgroundSpritePath"];
            if (currRow.GetCell(p) != null && currRow.GetCell(p).ToString() != "")
            {
                path = currRow.GetCell(p).ToString();
                content.backGroundSprite = GetSpriteResourceFromPath(path);
            }
            else
            {
                content.backGroundSprite = null;
                Debug.Log($"{i + 1}行Background为空，说这句话时将沿用上一句的背景");
            }

            //读取FloatingSpirtePath
            p = headerRowNameToIndexDictionary["FloatingSpirtePathJson"];
            if (currRow.GetCell(p) != null && currRow.GetCell(p).ToString() != "")
            {
                string json = currRow.GetCell(p).ToString();
                Dictionary<string, string> floatingImgNameToPathKeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                NamedSpriteList namedSprites = new NamedSpriteList();
                if (floatingImgNameToPathKeys != null)
                {
                    foreach (KeyValuePair<string, string> kvp in floatingImgNameToPathKeys)
                    {
                        Debug.Log($"Key = {kvp.Key}, Value = {kvp.Value}");
                        Sprite sprite = GetSpriteResourceFromPath(kvp.Value);//可能返回空
                        if (sprite != null)
                            namedSprites.AddNamedSprite(kvp.Key, sprite);
                    }
                    // Debug.LogWarning($"{i + 1}行FloatingSprites读取失败，说这句话时保持上一句话所有FloatingSprite不变");
                }
                content.namedSprites = namedSprites;
            }
            else
            {
                // content.floatingSprites = null;
                Debug.Log($"{i + 1}行FloatingSprites为空，说这句话时保持上一句话所有FloatingSprite不变");
            }

            //读取ContentSentence
            p = headerRowNameToIndexDictionary["ContentSentence"];
            if (currRow.GetCell(p) != null && currRow.GetCell(p).ToString() != "")
            {
                cellString = currRow.GetCell(p).ToString();
                content.contentSentence = cellString.Split("|").ToList<string>();
                selectionCount = content.contentSentence.Count;
                Debug.Log($"contentSentence的长度:{selectionCount}");
                if (selectionCount > 1)
                    content.selective = true;
            }
            else
            {
                Debug.LogWarning($"因为缺失ContentSentence,跳过第{i + 1}行的读取");
                continue;
            }


            //读取NextContentIndex
            p = headerRowNameToIndexDictionary["NextContentIndex"];
            if (currRow.GetCell(p) != null && currRow.GetCell(p).ToString() != "")
            {
                cellString = currRow.GetCell(p).ToString();
                string[] selectionIndex = cellString.Split("|");
                int[] sl = new int[selectionCount];
                List<int> s = sl.ToList<int>();
                if (selectionIndex.Length == 0)
                {
                    Debug.LogWarning($"因为缺失nextContentIndex,跳过第{i + 1}行的读取");
                    continue;
                }
                if (selectionIndex.Length == selectionCount)
                {
                    for (int q = 0; q < selectionCount; q++)
                    {
                        Debug.Log($"Q={q},s.Count ={s.Count}");
                        s[q] = int.Parse(selectionIndex[q]);
                    }
                }
                else//如果nextContent的长度短于contentSentence的长度，则用最后一个选项补全
                {
                    Debug.LogWarning($"nextContent的长度:{selectionIndex.Length}和contentSentence的长度:{selectionCount}不一致，用最后一个选项补全");
                    for (int q = 0; q < selectionCount; q++)
                    {
                        if (q < selectionIndex.Length)
                            s[q] = int.Parse(selectionIndex[q]);
                        else
                            s[q] = int.Parse(selectionIndex[selectionIndex.Length - 1]);
                    }
                }
                content.nextContentIndex = s;
            }
            else
            {
                Debug.Log($"因为缺失NextContentIndex,跳过第{i + 1}行的读取");
                continue;
            }
            dialogue.AddContent(content);
        }

        fs_read.Close();
        return dialogue;
    }
    public static Sprite GetSpriteResourceFromPath(string path)
    {
        Sprite sprite;
        if (path == "clear" || path == "Clear" || path == "None" || path == "none" || path == "null")
        {
            sprite = Resources.Load<Sprite>("DialogueImgs/Empty");
            Debug.LogWarning($"试图清除图片，请注意");
            Debug.Log(sprite);
            return sprite;
        }
        else
        {
            if (path[0] == '/')//删除首字的“/”
            {
                path = path.Trim('/');
            }
            if (path.Substring(0, 5) == "[bkg]")
                path = "DialogueImgs/Backgrounds/" + path.Remove(0, 5);
            if (path.Substring(0, 5) == "[spk]")
                path = "DialogueImgs/Speakers/" + path.Remove(0, 5);
            Debug.Log($"尝试从{path}加载Sprite");
            sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                Debug.Log("……成功！");
                return sprite;
            }
            else Debug.LogWarning("失败！:文件名不匹配，请检查是否已将图片改成Sprite");
        }
        return null;
    }
}
