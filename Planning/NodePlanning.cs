using UnityEngine;
using System.Collections;

public class NodePlanning
{
    public SuperWorld superWorld;

    public ActionPlanning mAction;

    public float gCost;
    public float hCost;

    public NodePlanning mParent;

    /***************************************************************************/

    public NodePlanning(SuperWorld superWorld, ActionPlanning action)
    {
        this.superWorld = new SuperWorld(superWorld);
        mAction = action;

        gCost = 0.0f;
        hCost = 0.0f;
        mParent = null;
    }

    /***************************************************************************/

    public float fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    /***************************************************************************/

    public bool Equals(NodePlanning other)
    {
        return (this.superWorld.IsEqualTo(other.superWorld));
    }

    /***************************************************************************/

}
