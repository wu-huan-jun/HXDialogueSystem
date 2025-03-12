using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueButton : MyButton
{
    public DialogueSystemUI dialogueSystemUI;
    public int buttonIndex;
    public void OnButtonClick()
    {
        Debug.Log("ButtonClick");
        if (dialogueSystemUI != null)
            dialogueSystemUI.OnSelectionButtonClick(buttonIndex);
    }
}
