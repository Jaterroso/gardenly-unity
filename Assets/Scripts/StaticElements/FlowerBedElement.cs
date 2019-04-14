﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerBedElement : GhostHandler, ISelectable, ISerializable
{
    public enum FlowerBedElementType { Flower01 };

    public SerializationController.ItemType type;
    public FlowerBedElementType subType;
    public Vector3 correctedRotation;

    private SerializedFBE serializableItem;

    void Start()
    {
        this.transform.eulerAngles += this.correctedRotation;
        if (ConstructionController.instance.flowerbedCount < 1)
        {
            ErrorHandler.instance.ErrorMessage("You must place a flowerbed first");
            ConstructionController.instance.Cancel();
        }
    }

    void OnMouseDrag()
    {
        MenuScript menu = Camera.main.GetComponent<UIController>().GetMenuScript();

        if (menu != null && menu.rotateState)
            menu.RotateGhost();
    }

    public override void Select(ConstructionController.ConstructionState state)
    {
      UIController uIController = Camera.main.GetComponent<UIController>();
        if (state == ConstructionController.ConstructionState.Off)
        {
            uIController.SpawnDynMenu(this, uIController.dynamicObjectMenu);
            uIController.SetDataPanel(this);
        }
    }

    public override void DeSelect()
    {
      UIController uIController = Camera.main.GetComponent<UIController>();
      if (uIController.GetMenuScript() != null && uIController.GetMenuScript().rotateState)
      {
          uIController.GetMenuScript().rotateState = false;
          uIController.GetMenuScript().GetComponentInChildren<LabelScript>().ResetColor();
      }
        // TODO si le menu bloque le ray cast appel destroymenu
        //uIController.GetMenuScript().DestroyMenu();
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
        tmp.subID = subType;
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
