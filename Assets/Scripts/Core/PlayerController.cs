﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance = null;
    public const int layerMaskInteractible = (1 << 9);
    public const int layerMaskStatic = (1 << 10);

    public GameObject canvas;
    public List<ISelectable> selectionList = new List<ISelectable>();
    public ISelectable currentSelection = null;
    public ActionRuntimeSet revertActionSet;
    public ActionRuntimeSet redoActionSet;
    public ActionHandler actionHandler;

    private Plane groundPlane = new Plane(Vector3.forward, Vector3.up);
    private IInteractible interactible;
    private ConstructionController constructionController;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            this.revertActionSet.items.Clear();
            this.redoActionSet.items.Clear();
            this.actionHandler = ScriptableObject.CreateInstance("ActionHandler") as ActionHandler;
        }
        else if (instance != this)
            Destroy(this.gameObject);
    }

    private void Start()
    {
        constructionController = ConstructionController.instance;
        this.actionHandler.Initialize();
    }



    void Update()
    {
        //DEBUG

        Vector3 pos;
        RaycastHit hit;

        if (this.constructionController.MouseRayCast(out pos, out hit))
            Debug.DrawLine(Camera.main.transform.position, pos);
        if (Input.GetKeyDown(KeyCode.L))
            ReactProxy.instance.ExportScene();
        //END DEBUG

        //Undo - Redo
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
            RedoAction();//TODO
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
            RevertAction();

        //Selection
        if (this.constructionController.currentState == ConstructionController.ConstructionState.Off)
        {
            if (Input.GetMouseButtonDown(0))
                SelectBuilding();
            else if (Input.GetKey(KeyCode.Delete))
                DestroySelection();
            return;
        }

        //Ghost Handle
        if (this.constructionController.currentState == ConstructionController.ConstructionState.Editing)
        {
            if (constructionController.editionState == ConstructionController.EditionType.Off)
            {
                if (Input.GetMouseButtonDown(0))
                    SelectBuilding();
                else if (Input.GetMouseButton(0) && interactible != null)
                    this.constructionController.UpdateGhostEditing(interactible);
                if (interactible != null && Input.GetMouseButtonUp(0))
                {
                    interactible.EndDrag();
                    interactible = null;
                }
            }
            else if (constructionController.editionState == ConstructionController.EditionType.Position)
            {
                if (constructionController.MouseRayCast(out pos, out hit))
                    constructionController.EditPosition(pos);
            }
            if (Input.GetMouseButtonDown(0) && constructionController.editionState != ConstructionController.EditionType.Off)
                actionHandler.ActionComplete(revertActionSet);
        }
        else
            this.constructionController.UpdateGhost();
    }


    //Actions
    public void CreateAction(ConstructionController.EditionType type)
    {
        constructionController.currentState = ConstructionController.ConstructionState.Editing;
        constructionController.editionState = ConstructionController.EditionType.Position;
        redoActionSet.items.Clear();

        switch (type)
        {
            case ConstructionController.EditionType.Position:
            {
                actionHandler.EditPositioning(currentSelection);
                break;
            }
            
            default:
                break;
        }
        constructionController.SetGhost(currentSelection.GetGameObject().GetComponent<GhostHandler>());//TODO might change
    }

    //Actions
    private void RedoAction()
    {
        Action action = redoActionSet.GetLastAction();
        if (action != null)
        {
            action.ReDo();
            revertActionSet.Add(action);
            redoActionSet.Remove(action);
            if (currentSelection != null)
                currentSelection.DeSelect();
            currentSelection = action.GetGameObject().GetComponent<ISelectable>();
            if (currentSelection != null)
                currentSelection.Select(ConstructionController.ConstructionState.Off);
        }
        else
            ErrorHandler.instance.ErrorMessage("No action to cancel");
    }

    private void RevertAction()
    {
        Action action = revertActionSet.GetLastAction();
        if (action != null)
        {
            action.Revert();
            redoActionSet.Add(action);
            revertActionSet.Remove(action);
            if (currentSelection != null)
                currentSelection.DeSelect();
            currentSelection = action.GetGameObject().GetComponent<ISelectable>();
            if (currentSelection != null)
                currentSelection.Select(ConstructionController.ConstructionState.Off);
        }
        else
            ErrorHandler.instance.ErrorMessage("No action to revert");
    }

    //Selection Handle
    private void SelectBuilding()
    {
        Vector3 pos;
        RaycastHit hit;

        if (IsPointerOnUi())
            return;

        DeSelect();
        if (constructionController.MouseRayCast(out pos, out hit, layerMaskStatic))
        {
            ISelectable selectable = hit.collider.gameObject.GetComponent<ISelectable>();

            if (selectable != null)
            {
                if (this.constructionController.currentState == ConstructionController.ConstructionState.Editing)
                    this.constructionController.currentState = ConstructionController.ConstructionState.Off;
                this.selectionList.Add(selectable);
                if (Input.GetKey(KeyCode.LeftControl))
                    this.selectionList.AddRange(selectable.SelectWithNeighbor());
                else
                {
                    this.currentSelection = selectable;
                    selectable.Select(constructionController.currentState);
                }
            }
        }
    }

    public void SpawnFlowerBedMesh()
    {
        Camera.main.GetComponent<UIController>().Cancel();
        SpawnController.instance.SpawnFlowerBed();
    }

    public void ForcedSelection(ISelectable elem)
    {
        foreach (ISelectable item in this.selectionList)
            item.DeSelect();
        this.selectionList.Clear();
        this.selectionList.Add(elem);
    }

    private void DeSelect(bool forced = false)
    {
        if (this.selectionList.Count > 0 && (!Input.GetKey(KeyCode.LeftShift)) || forced)
        {
            foreach (ISelectable elem in this.selectionList)
                elem.DeSelect();
            this.selectionList.Clear();
        }
        if (currentSelection != null)
        {
            this.currentSelection.DeSelect();
            this.currentSelection = null;
        }
    }

    private void DestroySelection()
    {
        if (this.selectionList != null)
        {
            for (int i = 0; i < this.selectionList.Count; i++)
                GameObject.Destroy(this.selectionList[i].GetGameObject());
            this.selectionList.Clear();
        }
    }

    private bool IsPointerOnUi()
    {
        return (EventSystem.current.IsPointerOverGameObject());
    }
}
