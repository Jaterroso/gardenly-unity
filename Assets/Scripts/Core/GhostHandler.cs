﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GhostHandler : MonoBehaviour, ISelectable, ISnapable
{
    protected List<ISelectable> neighbors = new List<ISelectable>();

    void Start()
    {
        this.gameObject.layer = 0;
    }

    void OnDestroy()
    {
        foreach (ISelectable elem in neighbors)
            elem.RemoveFromNeighbor(this);
    }

    void Update()
    {
    }

    public void StartPreview(Vector3 position)
    {
    }

    public void EndPreview()
    {
        this.gameObject.layer = 9;
    }

    public void Preview(Vector3 position)
    {
        this.transform.position = position;
    }

    public void Rotate(float axisInput)
    {
        Vector3 rotateValue = new Vector3(0, (axisInput * 10f), 0);
        this.transform.eulerAngles += rotateValue;
    }


    //ISelectable && ISnapable
    public GameObject GetGameObject() { return (this.gameObject); }

    //ISelectable
    public virtual void Select()
    {
    }

    public virtual List<ISelectable> SelectWithNeighbor()
    {
        Select();
        List<ISelectable> tmp = new List<ISelectable>(neighbors);
        foreach (ISelectable item in tmp)
            item.Select();
        return tmp;
    }

    public virtual void DeSelect()
    {
    }

    public void AddNeighbor(ISelectable item)
    {
        if (!neighbors.Contains(item))
            neighbors.Add(item);
    }

    public void RemoveFromNeighbor(ISelectable item)
    {
        neighbors.Remove(item);
    }

    //ISnapable
    public virtual bool FindSnapPoint(ref Vector3 currentPos, float snapDistance)
    {
        if ((currentPos - this.transform.position).magnitude < snapDistance)
        {
            currentPos = this.transform.position;
            return true;
        }
        return false;
    }

    public virtual bool isLinkable()
    {
        return true;
    }
}