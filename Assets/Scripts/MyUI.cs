using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyUI : MonoBehaviour
{
    [Header("MyUI组件")]
    public bool isTypeWriting;
    public IEnumerator TypeWrite(Text text, string message, float characterTime = 0.05f)
    {
        ///<summary>
        ///逐字将Message填入text
        ///</summary>
        if (text != null)
        {
            isTypeWriting = true;
            text.text = "";
            for (int i = 0; i < message.Length; i++)
            {
                text.text += message[i];
                yield return new WaitForSeconds(characterTime);
            }
            isTypeWriting = false;
        }
    }
    static public float KeepDecimal(float input, int n)//用于保留n位小数
    {
        string fn = "F" + n.ToString();
        float a = float.Parse(input.ToString(fn));
        return a;
    }
    static public string KeepDecimalAsString(float input, int n)//用于保留n位小数
    {
        string fn = "F" + n.ToString();
        return input.ToString(fn);
    }
    static public (float value, float mutiple) GetClosestStepValue(float input, float stepLength)
    {
        int mutiple = Mathf.RoundToInt(input / stepLength);
        return (mutiple * stepLength, mutiple);
    }
    public void FadeIn(CanvasGroup canvasGroup, float fadeTime = 1, float waitTime = 0)
    {
        StartCoroutine(HandleFadeIn(canvasGroup, fadeTime, waitTime));
    }
    public void FadeOut(CanvasGroup canvasGroup, float fadeTime = 1, float waitTime = 0)
    {
        StartCoroutine(HandleFadeOut(canvasGroup, fadeTime, waitTime));
    }
    public static IEnumerator HandleFadeIn(CanvasGroup canvasGroup, float fadeTime, float waitTime)
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Start FadeIn" + canvasGroup);
        float initialAlpha = canvasGroup.alpha;
        for (float i = 0; i < fadeTime; i += Time.fixedDeltaTime)
        {
            canvasGroup.alpha = initialAlpha + (i / fadeTime);
            yield return new WaitForFixedUpdate();
        }
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    public static IEnumerator HandleFadeOut(CanvasGroup canvasGroup, float fadeTime, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Start FadeOut" + canvasGroup);
        float initialAlpha = canvasGroup.alpha;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        for (float i = 0; i < fadeTime; i += Time.fixedDeltaTime)
        {
            canvasGroup.alpha = (1 - i / fadeTime) * initialAlpha;
            yield return new WaitForFixedUpdate();
        }
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
