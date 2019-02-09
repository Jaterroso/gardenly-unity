﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LabelScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Text text;
    public Color color;
    public Color actionColor;

    protected Image image;
    protected bool pressed;
    // Start is called before the first frame update
    void Start()
    {
        image = transform.GetComponent<Image>();
        pressed = false;
    }

    public void isPressed()
    {
        if (pressed)
            pressed = false;
        else
            pressed = true;
    }
   
    public void ChangeColor()
    {
        isPressed();
        if (pressed)
        {
            text.color = actionColor;
            image.color = actionColor;
        }
        else
        {
            text.color = color;
            image.color = color;
        }
    }

    public void ResetColor()
    {
        text.color = color;
        image.color = color;
        pressed = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!pressed)
        {
            text.color = actionColor;
            image.color = actionColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!pressed)
        {
            text.color = color;
            image.color = color;
        }
    }
}
