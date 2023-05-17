using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    bool doubleMoveToggle = false;
    bool canDoubleMove = false;

    protected override bool MoveToCell(Vector2Int position)
    {
        if (!CanMoveToCell(position)) { return false; }

        return base.MoveToCell(position);
    }

    protected override bool CanMoveToCell(Vector2Int position)
    {
        Vector2Int direction = position - GetPosition();
        bool isDoubleMove = direction.magnitude == 2;
        Debug.Log("isdoulbe" + isDoubleMove);

        if (!base.CanMoveToCell(position)) return false;
        if (isDoubleMove && gridManager.HasEntity(position)) return false;

        return true;
    }

    public override ObjectType GetObjectType()
    {
        return ObjectType.PLAYER;
    }

    private Vector2Int GetInputDirection()
    {
        int x = (Input.GetKeyDown(KeyCode.D)) ? 1 : (Input.GetKeyDown(KeyCode.A)) ? -1 : 0;
        int y = (Input.GetKeyDown(KeyCode.W)) ? 1 : (Input.GetKeyDown(KeyCode.S)) ? -1 : 0;

        return new Vector2Int(x, y);
    }

    public override bool ExecuteTurn() // Rename to request turn
    {
        Vector2Int direction = GetInputDirection();

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (canDoubleMove)
            {
                doubleMoveToggle = !doubleMoveToggle;
            }
            else
            {
                // Negative feedback sound/ui
            }
        }

        if (direction == Vector2Int.zero) return false;
        if (blockedDirections.Contains(direction) || !moveDirections.Contains(direction)) return false;

        if (doubleMoveToggle) direction *= 2;

        if (Move(direction))
        {
            blockedDirections.Clear();

            if (doubleMoveToggle)
            {
                doubleMoveToggle = false;
                canDoubleMove = false;
            }

            return true;
        }

        return false;
    }

    public void SetCanDoubleMove(bool condition)
    {
        canDoubleMove = condition;
    }
}
