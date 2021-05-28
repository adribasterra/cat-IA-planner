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
    [HideInInspector] public WorldState mWorldState;
    [HideInInspector] public GameObject[] trees;

    [Header("Elements")]
    public GameObject fox;
    public GameObject axe;
    public GameObject cottage;
    public GameObject feller;

    private Pathfinding pathfinding;
    private World world;

    private GameObject fellingTree;
    private GameObject tree;
    private GameObject trunk;
    private GameObject wood;

    public float hunger = 0f;
    public int timesLeftToBuildCottage = 1;

    /***************************************************************************/

    [System.Flags]
    public enum WorldState
    {
        WORLD_STATE_NONE = 0,
        WORLD_STATE_AXE_OWNED = 1,
        WORLD_STATE_CLOSE_TO_FOX = 2,
        WORLD_STATE_CLOSE_TO_TREE = 4,
        WORLD_STATE_HAS_WOOD = 8,
        WORLD_STATE_COTTAGE_BUILT = 16,
        WORLD_STATE_FOX_DEAD = 32,
        WORLD_STATE_HAS_MEAT = 64,
        WORLD_STATE_MEAT_EATEN = 128,

        //Behaviour tree WS
        WORLD_STATE_CLOSE_TO_AXE = 256,
        WORLD_STATE_FIRE_ON = 512,
        WORLD_STATE_HAS_RAW_MEAT = 1024,
        WORLD_STATE_HAS_COOKED_MEAT = 2048,
        WORLD_STATE_TREE_FELLED = 4096,
        WORLD_STATE_CLOSE_TO_COTTAGE = 8192,
    }

    /***************************************************************************/

    void Awake()
    {
        pathfinding = this.GetComponent<Pathfinding>();
        trees = GameObject.FindGameObjectsWithTag("Tree");
        fellingTree = null;

        mActionList = new List<ActionPlanning>();
        /************************************************/
        //  Actions of Ebakitzaile
        /************************************************/

        #region Planner actions

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_PICK_UP_AXE,
        World.WorldState.WORLD_STATE_NONE,
        World.WorldState.WORLD_STATE_AXE_OWNED,
        World.WorldState.WORLD_STATE_AXE_OWNED,
        World.WorldState.WORLD_STATE_NONE,
        CalculateDynamicCost(axe, 5.0f), "Pick up axe")
        );

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_FELL_TREE,
        World.WorldState.WORLD_STATE_AXE_OWNED,
        World.WorldState.WORLD_STATE_NONE,
        World.WorldState.WORLD_STATE_HAS_WOOD,
        World.WorldState.WORLD_STATE_NONE,                // PODRÍA PONER QUE SE ROMPA EL HACHA Y A BUSCARTE LA VIDA BUDDY
        CalculateDynamicCost(GetNearestTreePosition(), 10.0f), "Fell tree")
        );

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_HAUNT,
        World.WorldState.WORLD_STATE_AXE_OWNED,
        World.WorldState.WORLD_STATE_FOX_DEAD,
        World.WorldState.WORLD_STATE_FOX_DEAD | World.WorldState.WORLD_STATE_HAS_MEAT,
        World.WorldState.WORLD_STATE_NONE,
        CalculateDynamicCost(fox, 20.0f), "Kill fox")
        );

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_EAT_RAW_MEAT,
        World.WorldState.WORLD_STATE_HAS_MEAT,
        World.WorldState.WORLD_STATE_NONE,
        World.WorldState.WORLD_STATE_MEAT_EATEN,              // HUNGER++
        World.WorldState.WORLD_STATE_HAS_MEAT,
        5.0f, "Eat raw meat")
        );

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_EAT_COOKED_MEAT,
        World.WorldState.WORLD_STATE_HAS_MEAT | World.WorldState.WORLD_STATE_HAS_WOOD,
        World.WorldState.WORLD_STATE_NONE,
        World.WorldState.WORLD_STATE_MEAT_EATEN,              // HUNGER++++
        World.WorldState.WORLD_STATE_HAS_MEAT,
        15.0f, "Eat cooked meat")
        );

        mActionList.Add(
        new ActionPlanning(
        ActionPlanning.ActionType.ACTION_TYPE_BUILD_COTTAGE,
        World.WorldState.WORLD_STATE_HAS_WOOD,
        World.WorldState.WORLD_STATE_NONE,
        World.WorldState.WORLD_STATE_COTTAGE_BUILT,           // COTTAGE++
        World.WorldState.WORLD_STATE_HAS_WOOD,
        CalculateDynamicCost(cottage, 25.0f), "Building cottage")
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
            bool hasEnoughEnergy = hunger + action.mCost < 100;
            
            hunger += action.mCost;
            //Debug.Log(hunger);

            if (action.mActionType == ActionPlanning.ActionType.ACTION_TYPE_EAT_COOKED_MEAT) hunger += 40;
            if (action.mActionType == ActionPlanning.ActionType.ACTION_TYPE_EAT_COOKED_MEAT) hunger += 20;
            if (action.mActionType == ActionPlanning.ActionType.ACTION_TYPE_BUILD_COTTAGE) timesLeftToBuildCottage--;


            if (action.mActionType == ActionPlanning.ActionType.ACTION_TYPE_GO_TO_TREE) {
                GetNearestTreePosition();
            }


            // If preconditions are met we can apply effects and the new state is valid
            if ((node.mWorldState & action.mPreconditions) == action.mPreconditions &&
                (node.mWorldState & action.mNegativePreconditions) == World.WorldState.WORLD_STATE_NONE )
                //Aquí van los efectos del world absoluto y de los worlds hijos
            {
                // Apply positive effects
                World.WorldState positiveEffectsWorldState = (node.mWorldState | action.mEffects);
                // Apply negative effects
                World.WorldState finalWorldState = positiveEffectsWorldState & ~action.mNegativeEffects;

                NodePlanning newNodePlanning = new NodePlanning(finalWorldState, action);

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

    public GameObject GetNearestTreePosition()
    {
        GameObject result = trees[0];
        float minDistance = float.MaxValue;
        foreach (GameObject tree in trees)
        {
            // Set destination and origin as walkable so taht the pathfinding can find path
            feller.layer = 0;
            tree.transform.GetChild(0).GetChild(1).gameObject.layer = 0;
            // TODO: TAMBIÉN LOS PUTOS HIJOS ME CAGO EN LA PUTA
            pathfinding.GetGrid().UpdateGrid();

            if (pathfinding.FindPath(feller.transform.position, tree.transform.position.normalized, 0))
            {
                float dist = Mathf.Abs(Vector3.Distance(feller.transform.position, tree.transform.position));
                if (minDistance > dist)
                {
                    minDistance = dist;
                    result = tree;
                }
            }
            else
            {
                Debug.Log("Inaccessible tree");
            }
            tree.transform.GetChild(0).GetChild(1).gameObject.layer = 8; // Unwalkable
            pathfinding.GetGrid().UpdateGrid();
        }
        return result;
    }

    /***************************************************************************/

    private float CalculateDynamicCost(GameObject target, float baseCost)
    {
        float result = baseCost;
        if (target)
        {
            result = baseCost + Mathf.Abs(Vector3.Distance(feller.transform.position, target.transform.position)) * (hunger < 1 ? 1: hunger);
        }
        else Debug.Log("TARGET IS NULL");
        return result;
    }

    /***************************************************************************/

    public void SetFellingTree(GameObject tree)
    {
        fellingTree = tree;
        this.tree = fellingTree.transform.Find("Tree").gameObject;
        wood = fellingTree.transform.Find("Wood").gameObject;
        trunk = fellingTree.transform.Find("Trunk").gameObject;
        //Debug.Log("Tree: " + this.tree + ", " + "Wood: " + this.wood + ", " + "Trunk: " + this.trunk + " ");
    }

    public void FellTree()
    {
        if (tree != null)
        {
            tree.SetActive(false);
            trunk.SetActive(true);
        }
        else Debug.Log("tree is NULL");
    }

    public void SetWood()
    {
        if (wood != null)
        {
            wood.SetActive(true);
        }
        else Debug.Log("wood is NULL");
    }

    public void CollectWood()
    {
        if (wood != null)
        {
            wood.SetActive(false);
            wood = null;
            fellingTree = null;
        }
        else Debug.Log("wood is NULL");
    }
}