using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool pressed = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
    }
}
