﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;
using TMPro;

public class FlowerBedElement : GhostHandler, ISelectable, ISerializable
{
    public enum FlowerBedElementType { Flower01, Flower02, Flower03 };

    public SerializationController.ItemType type;
    public FlowerBedElementType subType;
    public Vector3 correctedRotation;

    private SerializedFBE serializableItem;

    void Start()
    {
        this.transform.eulerAngles += this.correctedRotation;
        if (ConstructionController.instance.flowerBeds.Count < 1)
        {
            MessageHandler.instance.ErrorMessage("flower_bed", "no_flowerbed");
            ConstructionController.instance.Cancel();
        }
    }

    public override void Select(ConstructionController.ConstructionState state)
    {
      UIController uIController = Camera.main.GetComponent<UIController>();
        TextMeshProUGUI[] labels = uIController.dataPanel.GetComponentsInChildren<TextMeshProUGUI>();

        if (ConstructionController.instance.currentState == ConstructionController.ConstructionState.Off)
        {
            Debug.Log(this.data.name);
            RectTransform menuTransform = uIController.extendMenu.RectTransform;
            RectTransform viewTransform = uIController.plantsViews[0].RectTransform;
            uIController.SpawnDynMenu(this, uIController.dynamicObjectMenu);
            if (!uIController.PlantsViewsDisplay())
                uIController.dataPanel.CustomStartAnchoredPosition = new Vector3(- menuTransform.sizeDelta.x + 0.3f, -33.46f, 0);
            else
                uIController.dataPanel.CustomStartAnchoredPosition = new Vector3(- menuTransform.sizeDelta.x + viewTransform.sizeDelta.x + 0.3f, -33.46f, 0);
            if (labels[labels.Length - 1].text != this.data.name || uIController.dataPanel.IsHidden)
                    uIController.SetDataPanel(this.data.name, "Fleur");
        }
    }

    public override void DeSelect()
    {
        if (Camera.main != null)
        {
            UIController uIController = Camera.main.GetComponent<UIController>();
            if (uIController.GetMenuScript() != null)
            {
                uIController.GetMenuScript().GetComponentInChildren<LabelScript>().ResetColor();
                uIController.GetMenuScript().DestroyMenu();
            }
        }
    }

    //Serialization
    [Serializable] //Create struct to stock in upper class
    public struct SerializedFBE
    {
        public FlowerBedElementType subID;
        public Vector3 position;
        public Quaternion rotation;
    }

    public SerializationData Serialize()
    {
        SerializationData tmp = new SerializationData();

        tmp.type = SerializationController.ItemType.FlowerBedElement;
        tmp.data = JsonUtility.ToJson(InnerSerialize());
        return tmp;
    }

    public SerializedFBE InnerSerialize()
    {
        SerializedFBE tmp;

        tmp.position = this.transform.position;
        tmp.rotation = this.transform.rotation;
        //TMPFRONT
        tmp.subID = (FlowerBedElementType)UnityEngine.Random.Range(0, 3);
        //tmp.subID = subType;
        return (tmp);
    }

    public void InnerDeSerialize(SerializedFBE elem)
    {
        this.subType = elem.subID;
        this.transform.position = elem.position;
        this.transform.rotation = elem.rotation;
    }

    public void DeSerialize(string json)
    {
        serializableItem = JsonUtility.FromJson<SerializedFBE>(json);
        this.transform.position = serializableItem.position;
        this.transform.rotation = serializableItem.rotation;
    }
}
