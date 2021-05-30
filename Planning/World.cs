using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class World : MonoBehaviour
{
    [HideInInspector] public List<ActionPlanning> mActionList;
    [HideInInspector] public HashSet<NodePlanning> closedSet;
    [HideInInspector] public List<NodePlanning> openSet;
    [HideInInspector] public List<NodePlanning> plan;
    [HideInInspector] public SuperWorld superWorld;

    private Pathfinding pathfinding;

    /***************************************************************************/

    void Awake()
    {
        pathfinding = this.GetComponent<Pathfinding>();
        superWorld = this.GetComponent<SuperWorld>();

        mActionList = new List<ActionPlanning>();
        /************************************************/
        //  Actions of Ebakitzaile
        /************************************************/

        #region Planner actions

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_PICK_UP_AXE,
        SuperWorld.WorldState.WORLD_STATE_NONE,
        SuperWorld.WorldState.WORLD_STATE_AXE_OWNED,
        SuperWorld.WorldState.WORLD_STATE_AXE_OWNED,
        SuperWorld.WorldState.WORLD_STATE_NONE,
        5.0f, "Pick up axe")
        );

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_FELL_TREE,
        SuperWorld.WorldState.WORLD_STATE_AXE_OWNED,
        SuperWorld.WorldState.WORLD_STATE_NONE,
        SuperWorld.WorldState.WORLD_STATE_HAS_WOOD,
        SuperWorld.WorldState.WORLD_STATE_NONE,                // PODRÍA PONER QUE SE ROMPA EL HACHA Y A BUSCARTE LA VIDA BUDDY
        /*CalculateDynamicCost(GetNearestTreePosition(), 10.0f)*/10f, "Fell tree")
        );

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_HAUNT,
        SuperWorld.WorldState.WORLD_STATE_AXE_OWNED,
        SuperWorld.WorldState.WORLD_STATE_FOX_DEAD,
        SuperWorld.WorldState.WORLD_STATE_FOX_DEAD | SuperWorld.WorldState.WORLD_STATE_HAS_MEAT,
        SuperWorld.WorldState.WORLD_STATE_NONE,
        20.0f, "Kill fox")
        );

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_EAT_RAW_MEAT,
        SuperWorld.WorldState.WORLD_STATE_HAS_MEAT,
        SuperWorld.WorldState.WORLD_STATE_NONE,
        SuperWorld.WorldState.WORLD_STATE_NONE,              // HUNGER++
        SuperWorld.WorldState.WORLD_STATE_HAS_MEAT,
        5.0f, "Eat raw meat")
        );

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_EAT_COOKED_MEAT,
        SuperWorld.WorldState.WORLD_STATE_HAS_MEAT | SuperWorld.WorldState.WORLD_STATE_HAS_WOOD,
        SuperWorld.WorldState.WORLD_STATE_NONE,
        SuperWorld.WorldState.WORLD_STATE_NONE,              // HUNGER++++
        SuperWorld.WorldState.WORLD_STATE_HAS_MEAT,
        15.0f, "Eat cooked meat")
        );

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_BUILD_COTTAGE,
        SuperWorld.WorldState.WORLD_STATE_HAS_WOOD,
        SuperWorld.WorldState.WORLD_STATE_NONE,
        SuperWorld.WorldState.WORLD_STATE_NONE,           // COTTAGE++
        SuperWorld.WorldState.WORLD_STATE_HAS_WOOD,
        25.0f, "Building cottage")
        );
        #endregion
        
        /************************************************/
        //  Actions of Basapizti
        /************************************************/
    }

    /***************************************************************************/

    public List<NodePlanning> GetNeighbours(NodePlanning node)
    {
        List<NodePlanning> neighbours = new List<NodePlanning>();

        foreach (ActionPlanning action in mActionList)
        {
            // If preconditions are met we can apply effects and the new state is valid
            if ((node.superWorld.mWorldState & action.mPreconditions) == action.mPreconditions &&
                (node.superWorld.mWorldState & action.mNegativePreconditions) == SuperWorld.WorldState.WORLD_STATE_NONE &&
                 node.superWorld.hunger < 100f)
                //Aquí van los efectos del world absoluto y de los worlds hijos
            {
                SuperWorld aux = new SuperWorld(superWorld);
                aux.hunger += action.mCost;

                // Calculate dynamic costs
                GameObject target = null;
                switch (action.mActionType)
                {
                    case ActionPlanning.ActionType.ACTION_TYPE_PICK_UP_AXE:
                        target = this.superWorld.axe;
                        break;
                    //case ActionPlanning.ActionType.ACTION_TYPE_FELL_TREE:
                    //    target = this.superWorld.GetNearestTreePosition();
                    //    break;
                    case ActionPlanning.ActionType.ACTION_TYPE_HAUNT:
                        target = this.superWorld.fox;
                        break;

                    // Update global vars
                    case ActionPlanning.ActionType.ACTION_TYPE_EAT_COOKED_MEAT:
                        aux.hunger -= 40;
                        break;
                    case ActionPlanning.ActionType.ACTION_TYPE_EAT_RAW_MEAT:
                        aux.hunger -= 10;
                        break;
                }
                if(target != null) action.mCost = CalculateDynamicCost(aux, target, action.mCost); Debug.Log(action.mActionType + " :" + action.mCost);
                
                // Apply positive effects
                SuperWorld.WorldState positiveEffectsWorldState = (node.superWorld.mWorldState | action.mEffects);

                if (action.mActionType == ActionPlanning.ActionType.ACTION_TYPE_BUILD_COTTAGE/* && node.superWorld.timesLeftToBuildCottage > 0*/)
                {
                    aux.timesLeftToBuildCottage--;

                    if (node.superWorld.timesLeftToBuildCottage <= 0)
                    {
                        positiveEffectsWorldState |= SuperWorld.WorldState.WORLD_STATE_COTTAGE_BUILT;
                    }
                }

                // Apply negative effects
                aux.mWorldState = positiveEffectsWorldState & ~action.mNegativeEffects;

                NodePlanning newNodePlanning = new NodePlanning(aux, action);
                Debug.Log(action.mActionType + ": " + aux.mWorldState);
                neighbours.Add(newNodePlanning);
            }
        }

        return neighbours;
    }

    /***************************************************************************/

    public static int PopulationCount(int n)
    {
        return System.Convert.ToString(n, 2).ToCharArray().Count(c => c == '1');
    }

    /***************************************************************************/

    private float CalculateDynamicCost(SuperWorld superWorld, GameObject target, float baseCost)
    {
        float result = baseCost;
        if (target)
        {
            result = baseCost + Mathf.Abs(Vector3.Distance(superWorld.feller.transform.position, target.transform.position)) * (superWorld.hunger < 1 ? 1: superWorld.hunger);
        }
        else Debug.Log("TARGET IS NULL");
        Debug.Log("Cost result: " + result);
        return result;
    }

    /***************************************************************************/
    
}