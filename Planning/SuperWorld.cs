using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperWorld : MonoBehaviour
{
    [HideInInspector] public GameObject[] trees;
    [HideInInspector] public SuperWorld.WorldState mWorldState;
    public Pathfinding pathfinding;

    [Header("Elements")]
    public GameObject fox;
    public GameObject axe;
    public GameObject cottage;
    public GameObject feller;

    private GameObject fellingTree;
    private GameObject tree;
    private GameObject trunk;
    private GameObject wood;

    public float hunger = 0f;
    public int timesLeftToBuildCottage = 4;

    /***************************************************************************/

    [System.Flags]
    public enum WorldState
    {
        WORLD_STATE_NONE                = 0,
        WORLD_STATE_AXE_OWNED           = 1,
        WORLD_STATE_CLOSE_TO_FOX        = 2,
        WORLD_STATE_CLOSE_TO_TREE       = 4,
        WORLD_STATE_HAS_WOOD            = 8,
        WORLD_STATE_COTTAGE_BUILT       = 16,
        WORLD_STATE_FOX_DEAD            = 32,
        WORLD_STATE_HAS_MEAT            = 64,
        WORLD_STATE_MEAT_EATEN          = 128,

        //Behaviour tree WS
        WORLD_STATE_CLOSE_TO_AXE        = 256,
        WORLD_STATE_FIRE_ON             = 512,
        WORLD_STATE_HAS_RAW_MEAT        = 1024,
        WORLD_STATE_HAS_COOKED_MEAT     = 2048,
        WORLD_STATE_TREE_FELLED         = 4096,
        WORLD_STATE_CLOSE_TO_COTTAGE    = 8192,
    }

    void Awake()
    {
        trees = GameObject.FindGameObjectsWithTag("Tree");
        fellingTree = null;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /***************************************************************************/

    public SuperWorld(SuperWorld superWorld)
    {
        this.trees = superWorld.trees;
        this.mWorldState = superWorld.mWorldState;
        this.fox = superWorld.fox;
        this.axe = superWorld.axe;
        this.cottage = superWorld.cottage;
        this.feller = superWorld.feller;
        this.fellingTree = superWorld.fellingTree;
        this.trunk = superWorld.trunk;
        this.wood = superWorld.wood;
        this.hunger = superWorld.hunger;
        this.timesLeftToBuildCottage = superWorld.timesLeftToBuildCottage;
    }

    /***************************************************************************/

    public bool IsEqualTo(SuperWorld superWorld)
    {
        bool result = false;

        if (this.trees == superWorld.trees &&
            this.mWorldState == superWorld.mWorldState &&
            this.fox == superWorld.fox &&
            this.axe == superWorld.axe &&
            this.cottage == superWorld.cottage &&
            this.feller == superWorld.feller &&
            this.fellingTree == superWorld.fellingTree &&
            this.trunk == superWorld.trunk &&
            this.wood == superWorld.wood &&
            this.hunger == superWorld.hunger &&
            this.timesLeftToBuildCottage == superWorld.timesLeftToBuildCottage)
        {
            result = true;
        }
        return result;
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

            if (pathfinding.FindPath(feller.transform.position, tree.transform.position.normalized, -1) != null)
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
