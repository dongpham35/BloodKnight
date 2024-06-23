using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class UiButtonHanlder : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public bool isButtonHeld;
    public bool isDoubleClick;
    private float lastClickTime;
    private float doubleClickTime = 0.3f;
    public void OnPointerClick(PointerEventData eventData)
    {
        isDoubleClick = false;
        if(Time.time - lastClickTime <= doubleClickTime)
        {
            isDoubleClick = true;
        }
        lastClickTime = Time.time;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isButtonHeld= true;
    }  

    public void OnPointerUp(PointerEventData eventData)
    {
        isButtonHeld = false;
    }
}
