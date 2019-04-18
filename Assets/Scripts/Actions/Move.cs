﻿using UnityEngine;

public class Move : Action
{
    private Vector3 oldPosition;
    private Vector3 newPosition;

    public override void Initialize(GameObject gameObject)
    {
        this.gameObject = gameObject;
        this.oldPosition = gameObject.transform.position;
    }

    public override void Complete()
    {
        newPosition = gameObject.transform.position;
    }

    public override void Revert()
    {
        GhostHandler ghost = this.gameObject.GetComponent<GhostHandler>();
        if (ghost != null)
            ghost.Move(oldPosition);
        else
            this.gameObject.transform.position = oldPosition;
    }

    public override void ReDo()
    {
        GhostHandler ghost = this.gameObject.GetComponent<GhostHandler>();
        if (ghost != null)
            ghost.Move(newPosition);
        else
            this.gameObject.transform.position = newPosition;
    }
}
