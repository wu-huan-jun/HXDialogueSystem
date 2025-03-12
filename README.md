# HXDialogueSystem

这个库是无患用五天手搓的DialogueSystem的示例Unity项目，可以完成基础的对话、选项、背景图和角色图的替换。已测试打包安卓后可以正常工作

后续会更新存档和语音接口

你可以自由使用或修改示例项目中的所有脚本。所有注释都是用中文写的，所以如果你要作修改的话，建议看完注释再改。

示例项目Unity版本：2022.3.17 LTS

## 外部库相关

DialogueSystem.cs中记载了一个方法
` public void ReloadAlldialoguesFromFile() `,用于从.xslx中读取对话。但这个方法依赖于NPOI和Newtonsoft这两个库，如果你想在自己的项目中使用这个DialogueSystem，你需要通过Unity NuGet插件或Visual Studio下载这两个库；如果你在自己的项目中不需要从xlsx读取对话，你可以删除这两个库的using.

示例项目已安装这两个库和Unity NuGet

## 读取xlsx（excel文件）的格式

记载对话的xlsx文件的格式要求已载于 `Assets\Dialogues\TestDialogue.xlsx` 

## 谨记：读取xlsx只能在Editor中使用，在打包后则会失效！


如果你想要打包游戏而不是做一个在Editor玩的demo，目前的办法是读取xlsx后在PlayMode下复制DialogueSystem的Component，退出PlayMode，然后Paste Component Value回DialogueSystem上。

## 示例项目中的资产？
示例剧情中使用的对话是我正在做的旮旯game，因为女主现在只有建模没有立绘，所以为了美观性（？）我拿建模凹了几个pose喂给了Sd，就有了示例对话里的人物图片。

## 反馈和建议
如果你是上科大的同学，可以直接来创艺E511线下丹砂我。

……应该只有上科大的同学会看到这个库吧





