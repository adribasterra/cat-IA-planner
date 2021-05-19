using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class World : MonoBehaviour
{
    public Transform[] trees;


    /***************************************************************************/

    [System.Flags]
    public enum WorldState
    {
        //El hacha está cogida
        //Quedan x árboles sin talar

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

    }

    /***************************************************************************/

    public static int PopulationCount(int n)
    {
        return System.Convert.ToString(n, 2).ToCharArray().Count(c => c == '1');
    }

    /***************************************************************************/

}