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
        CalculateDynamicCost(superWorld.axe, 5.0f), "Pick up axe")
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
        CalculateDynamicCost(superWorld.fox, 20.0f), "Kill fox")
        );

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_EAT_RAW_MEAT,
        SuperWorld.WorldState.WORLD_STATE_HAS_MEAT,
        SuperWorld.WorldState.WORLD_STATE_NONE,
        SuperWorld.WorldState.WORLD_STATE_MEAT_EATEN,              // HUNGER++
        SuperWorld.WorldState.WORLD_STATE_HAS_MEAT,
        5.0f, "Eat raw meat")
        );

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_EAT_COOKED_MEAT,
        SuperWorld.WorldState.WORLD_STATE_HAS_MEAT | SuperWorld.WorldState.WORLD_STATE_HAS_WOOD,
        SuperWorld.WorldState.WORLD_STATE_NONE,
        SuperWorld.WorldState.WORLD_STATE_MEAT_EATEN,              // HUNGER++++
        SuperWorld.WorldState.WORLD_STATE_HAS_MEAT,
        15.0f, "Eat cooked meat")
        );

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_BUILD_COTTAGE,
        SuperWorld.WorldState.WORLD_STATE_HAS_WOOD,
        SuperWorld.WorldState.WORLD_STATE_NONE,
        SuperWorld.WorldState.WORLD_STATE_COTTAGE_BUILT,           // COTTAGE++
        SuperWorld.WorldState.WORLD_STATE_HAS_WOOD,
        CalculateDynamicCost(superWorld.cottage, 25.0f), "Building cottage")
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
            if (action.mActionType == ActionPlanning.ActionType.ACTION_TYPE_EAT_COOKED_MEAT) superWorld.hunger += 20;
            if (action.mActionType == ActionPlanning.ActionType.ACTION_TYPE_BUILD_COTTAGE) superWorld.timesLeftToBuildCottage--;

            if (action.mActionType == ActionPlanning.ActionType.ACTION_TYPE_GO_TO_TREE) {
                superWorld.GetNearestTreePosition();
            }



            // If preconditions are met we can apply effects and the new state is valid
            if ((node.superWorld.mWorldState & action.mPreconditions) == action.mPreconditions &&
                (node.superWorld.mWorldState & action.mNegativePreconditions) == SuperWorld.WorldState.WORLD_STATE_NONE )
                //Aquí van los efectos del world absoluto y de los worlds hijos
            {
                SuperWorld aux = new SuperWorld(superWorld);
                aux.hunger += action.mCost;

                if (action.mActionType == ActionPlanning.ActionType.ACTION_TYPE_EAT_COOKED_MEAT)
                {
                    aux.hunger -= 40;
                }
                if (action.mActionType == ActionPlanning.ActionType.ACTION_TYPE_EAT_RAW_MEAT)
                {
                    aux.hunger -= 10;
                }
                if (action.mActionType == ActionPlanning.ActionType.ACTION_TYPE_BUILD_COTTAGE && superWorld.timesLeftToBuildCottage < 4)
                {
                    aux.timesLeftToBuildCottage--;
                }

                // Apply positive effects
                SuperWorld.WorldState positiveEffectsWorldState = (node.superWorld.mWorldState | action.mEffects);
                // Apply negative effects
                aux.mWorldState = positiveEffectsWorldState & ~action.mNegativeEffects;

                NodePlanning newNodePlanning = new NodePlanning(aux, action);

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

    private float CalculateDynamicCost(GameObject target, float baseCost)
    {
        float result = baseCost;
        if (target)
        {
            result = baseCost + Mathf.Abs(Vector3.Distance(superWorld.feller.transform.position, target.transform.position)) * (superWorld.hunger < 1 ? 1: superWorld.hunger);
        }
        else Debug.Log("TARGET IS NULL");
        return result;
    }

    /***************************************************************************/
    
}