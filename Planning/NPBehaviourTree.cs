using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NPBehave;

public class NPBehaviourTree : MonoBehaviour
{
    private Root mBehaviorTree;

    int mPlanSteps = 0;
    int mCurrentAction = -1;

    [HideInInspector]
    public Planning mPlanner;
    [HideInInspector]
    public List<NodePlanning> mPlan;
    [HideInInspector]
    public Pathfinding pathfinding;

    private PlayerController playerMovement;

    float mTimeStartAction = 0.0f;
    float mTimeActionLast = 2.0f;
    int step = 1;

    /****************************************************************************/

    void Awake()
    {
        pathfinding = this.GetComponent<Pathfinding>();
        playerMovement = mPlanner.GetWorld().feller.GetComponent<PlayerController>();
    }

    // Use this for initialization
    void Start()
    {
        mBehaviorTree = new Root(
            new Sequence(
                new Action((bool planning) =>
                {
                    //Llamar al planificador y pedirle el plan
                    mPlan = mPlanner.GetPlan();

                    mPlanSteps = mPlan.Count;
                    mCurrentAction = -1;

                    Debug.Log("Planned in " + mPlanSteps + " steps");

                    if (mPlan.Count > 0)
                    {
                        return Action.Result.SUCCESS;
                    }
                    else
                    {
                        return Action.Result.FAILED;
                    }
                })
                { Label = "Planning" },
                
                new Repeater(-1,
                    new Sequence(
                        // POP UP next action
                        new Action((bool nextActionAvailable) =>
                        {
                            mCurrentAction++;
                            mTimeStartAction = Time.time;
                            
                            if (mCurrentAction >= mPlan.Count)
                            {
                                return Action.Result.FAILED;
                            }
                            else
                            {
                                return Action.Result.SUCCESS;
                            }
                        }),

                        // EXECUTE action in corresponding one
                        new Selector(
                          new Sequence(                             // HAUNTING
                            new Action((bool reachFox) =>
                            {
                                if (mPlan[mCurrentAction].mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_HAUNT)
                                {
                                    // GO_TO_FOX action - preconditions & effects
                                    SuperWorld.WorldState preconditions   = SuperWorld.WorldState.WORLD_STATE_NONE;
                                    SuperWorld.WorldState positiveEffects = SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_FOX;
                                    SuperWorld.WorldState negativeEffects = SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_AXE | SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_TREE | SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_COTTAGE;

                                    // Check preconditions
                                    if ((mPlanner.GetWorld().mWorldState & preconditions) == preconditions)
                                    {
                                        if(pathfinding.GetGrid().path == null)
                                        {
                                            // Set default layer for the pathfinding to be able to find a path
                                            mPlanner.GetWorld().fox.layer = 0;
                                            pathfinding.GetGrid().UpdateGrid();

                                            if(pathfinding.InitPathfinding(mPlanner.GetWorld().feller.transform, mPlanner.GetWorld().fox.transform))
                                            {
                                                //Set path to feller
                                                playerMovement.SetPath(pathfinding.GetGrid().path);
                                            }

                                            // Reset to unwalkable layer
                                            mPlanner.GetWorld().fox.layer = 8;
                                            pathfinding.GetGrid().UpdateGrid();
                                        }

    // TODO: check if feller has arrived
                                        if (playerMovement.hasArrived)
                                        {
                                            Debug.Log("Near fox");
                                            playerMovement.hasArrived = false;
                                            pathfinding.GetGrid().path = null;

                                            // Apply positive & negative effects
                                            mPlanner.GetWorld().mWorldState |= positiveEffects;
                                            mPlanner.GetWorld().mWorldState &= ~negativeEffects;

                                            mTimeStartAction = Time.time;
                                            return Action.Result.SUCCESS;
                                        }
                                        else
                                        {
                                            // Action in progress
                                            return Action.Result.PROGRESS;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("--- ERROR: PLAN FAILED ---");
                                        return Action.Result.FAILED;
                                    }
                                }
                                else
                                {
                                    return Action.Result.FAILED;
                                }
                            })
                            { Label = "Go to fox" },
                            // Action 1
                            new Action((bool hauntFox) =>
                            {
                                if (mPlan[mCurrentAction].mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_HAUNT)
                                {
                                    // KILL_FOX action - preconditions & effects
                                    SuperWorld.WorldState preconditions   = SuperWorld.WorldState.WORLD_STATE_AXE_OWNED | SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_FOX;
                                    SuperWorld.WorldState positiveEffects = SuperWorld.WorldState.WORLD_STATE_FOX_DEAD  | SuperWorld.WorldState.WORLD_STATE_HAS_RAW_MEAT;
                                    SuperWorld.WorldState negativeEffects = SuperWorld.WorldState.WORLD_STATE_NONE;

                                    // Check preconditions
                                    if ((mPlanner.GetWorld().mWorldState & preconditions) == preconditions)
                                    {
                                        // If execution succeeded return "success". Otherwise return "failed".
                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            Debug.Log("Fox killed");

                                            mPlanner.GetWorld().fox.SetActive(false);
                                            mPlanner.GetWorld().fox.layer = 0;
                                            pathfinding.GetGrid().UpdateGrid();

                                            // Apply positive & negative effects
                                            mPlanner.GetWorld().mWorldState |= positiveEffects;
                                            mPlanner.GetWorld().mWorldState &= ~negativeEffects;

                                            mTimeStartAction = Time.time;
                                            return Action.Result.SUCCESS;
                                        }
                                        else
                                        {
                                            // Action in progress
                                            return Action.Result.PROGRESS;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("--- ERROR: PLAN FAILED ---");
                                        return Action.Result.FAILED;
                                    }
                                }
                                else
                                {
                                    return Action.Result.FAILED;
                                }
                            })
                            { Label = "Kill fox" }
                            // Action 2
                          ),// Sequence - HAUNTING

                          new Sequence(                             // FELL TREE
                            new Action((bool reachTree) =>
                            {
                                if (mPlan[mCurrentAction].mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_FELL_TREE)
                                {
                                    // GO_TO_TREE action - preconditions & effects
                                    SuperWorld.WorldState preconditions   = SuperWorld.WorldState.WORLD_STATE_NONE;
                                    SuperWorld.WorldState positiveEffects = SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_TREE;
                                    SuperWorld.WorldState negativeEffects = SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_AXE | SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_FOX | SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_COTTAGE;

                                    // Check preconditions
                                    if ((mPlanner.GetWorld().mWorldState & preconditions) == preconditions)
                                    {
                                        if (pathfinding.GetGrid().path == null)
                                        {
                                            GameObject nearestTree = mPlanner.GetWorld().GetNearestTreePosition();
                                            mPlanner.GetWorld().SetFellingTree(nearestTree);

                                            // Set default layer for the pathfinding to be able to find a path
                                            nearestTree.transform.GetChild(0).GetChild(1).gameObject.layer = 0;
                                            pathfinding.GetGrid().UpdateGrid();

                                            // Set default layer for the pathfinding to be able to find a path
                                            pathfinding.GetGrid().UpdateGrid();

                                            if (pathfinding.InitPathfinding(mPlanner.GetWorld().feller.transform, nearestTree.transform))
                                            {
                                                Debug.Log("Path set");
                                                //Set path to feller
                                                playerMovement.SetPath(pathfinding.GetGrid().path);
                                            }
                                            else
                                            {
                                                Debug.Log("There is no path");
                                            }

                                            // Reset to unwalkable layer
                                            nearestTree.transform.GetChild(0).GetChild(1).gameObject.layer = 8;
                                            pathfinding.GetGrid().UpdateGrid();
                                        }

                                        // TODO: check if feller has arrived

                                        // If execution succeeded return "success". Otherwise return "failed".
                                        if (playerMovement.hasArrived)
                                        {
                                            Debug.Log("Near tree");
                                            playerMovement.hasArrived = false;
                                            pathfinding.GetGrid().path = null;

                                            // Apply positive & negative effects
                                            mPlanner.GetWorld().mWorldState |= positiveEffects;
                                            mPlanner.GetWorld().mWorldState &= ~negativeEffects;

                                            mTimeStartAction = Time.time;
                                            return Action.Result.SUCCESS;
                                        }
                                        else
                                        {
                                            // Action in progress
                                            return Action.Result.PROGRESS;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("--- ERROR: PLAN FAILED ---");
                                        return Action.Result.FAILED;
                                    }
                                }
                                else
                                {
                                    return Action.Result.FAILED;
                                }
                            })
                            { Label = "Go to tree" },
                            // Action 1
                            new Action((bool fellTree) =>
                            {
                                if (mPlan[mCurrentAction].mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_FELL_TREE)
                                {
                                    // FELL_TREE action - preconditions & effects
                                    SuperWorld.WorldState preconditions   = SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_TREE | SuperWorld.WorldState.WORLD_STATE_AXE_OWNED;
                                    SuperWorld.WorldState positiveEffects = SuperWorld.WorldState.WORLD_STATE_TREE_FELLED;
                                    SuperWorld.WorldState negativeEffects = SuperWorld.WorldState.WORLD_STATE_NONE;

                                    // Check preconditions
                                    if ((mPlanner.GetWorld().mWorldState & preconditions) == preconditions)
                                    {
                                        mPlanner.GetWorld().feller.GetComponent<Animator>().SetBool("lumbering", true);
                                        // If execution succeeded return "success". Otherwise return "failed".
                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            Debug.Log("Tree felled");
                                            mPlanner.GetWorld().feller.GetComponent<Animator>().SetBool("lumbering", false);

                                            mPlanner.GetWorld().FellTree();
                                            mPlanner.GetWorld().SetWood();

                                            // Apply positive & negative effects
                                            mPlanner.GetWorld().mWorldState |= positiveEffects;
                                            mPlanner.GetWorld().mWorldState &= ~negativeEffects;

                                            mTimeStartAction = Time.time;
                                            return Action.Result.SUCCESS;
                                        }
                                        else
                                        {
                                            // Action in progress
                                            return Action.Result.PROGRESS;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("--- ERROR: PLAN FAILED ---");
                                        return Action.Result.FAILED;
                                    }
                                }
                                else
                                {
                                    return Action.Result.FAILED;
                                }
                            })
                            { Label = "Fell tree" },
                            // Action 2
                            new Action((bool collectWood) =>
                            {
                                if (mPlan[mCurrentAction].mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_FELL_TREE)
                                {
                                    // COLLECT_WOOD action - preconditions & effects
                                    SuperWorld.WorldState preconditions   = SuperWorld.WorldState.WORLD_STATE_TREE_FELLED;
                                    SuperWorld.WorldState positiveEffects = SuperWorld.WorldState.WORLD_STATE_HAS_WOOD;
                                    SuperWorld.WorldState negativeEffects = SuperWorld.WorldState.WORLD_STATE_NONE;

                                    // Check preconditions
                                    if ((mPlanner.GetWorld().mWorldState & preconditions) == preconditions)
                                    {
                                        // If execution succeeded return "success". Otherwise return "failed".
                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            Debug.Log("Wood collected");
                                            mPlanner.GetWorld().CollectWood();
                                            mPlanner.GetWorld().feller.GetComponent<PlayerController>().SetBagActive(true);

                                            // Apply positive & negative effects
                                            mPlanner.GetWorld().mWorldState |= positiveEffects;
                                            mPlanner.GetWorld().mWorldState &= ~negativeEffects;

                                            mTimeStartAction = Time.time;
                                            return Action.Result.SUCCESS;
                                        }
                                        else
                                        {
                                            // Action in progress
                                            return Action.Result.PROGRESS;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("--- ERROR: PLAN FAILED ---");
                                        return Action.Result.FAILED;
                                    }
                                }
                                else
                                {
                                    return Action.Result.FAILED;
                                }
                            })
                            { Label = "Collect wood" }
                            // Action 3
                          ),// Sequence - FELL TREE

                          new Sequence(                             // EAT RAW MEAT
                            new Action((bool eatRawMeat) =>
                            {
                                if (mPlan[mCurrentAction].mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_EAT_RAW_MEAT)
                                {
                                    // EAT_RAW_MEAT action - preconditions & effects
                                    SuperWorld.WorldState preconditions   = SuperWorld.WorldState.WORLD_STATE_HAS_RAW_MEAT;
                                    SuperWorld.WorldState positiveEffects = SuperWorld.WorldState.WORLD_STATE_NONE;
                                    SuperWorld.WorldState negativeEffects = SuperWorld.WorldState.WORLD_STATE_HAS_RAW_MEAT;

                                    // Check preconditions
                                    if ((mPlanner.GetWorld().mWorldState & preconditions) == preconditions)
                                    {
                                        // If execution succeeded return "success". Otherwise return "failed".
                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            Debug.Log("Ate raw meat");

                                            // Apply positive & negative effects
                                            mPlanner.GetWorld().mWorldState |= positiveEffects;
                                            mPlanner.GetWorld().mWorldState &= ~negativeEffects;

                                            mTimeStartAction = Time.time;
                                            return Action.Result.SUCCESS;
                                        }
                                        else
                                        {
                                            // Action in progress
                                            return Action.Result.PROGRESS;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("--- ERROR: PLAN FAILED ---");
                                        return Action.Result.FAILED;
                                    }
                                }
                                else
                                {
                                    return Action.Result.FAILED;
                                }
                            })
                            { Label = "Eat raw meat" }
                            // Action 1
                          ),// Sequence - EAT RAW MEAT

                          new Sequence(                             // EAT COOKED MEAT
                            new Action((bool lightFire) =>
                            {
                                if (mPlan[mCurrentAction].mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_EAT_COOKED_MEAT)
                                {
                                    // LIGHT_FIRE action - preconditions & effects
                                    SuperWorld.WorldState preconditions   = SuperWorld.WorldState.WORLD_STATE_HAS_WOOD;
                                    SuperWorld.WorldState positiveEffects = SuperWorld.WorldState.WORLD_STATE_FIRE_ON;
                                    SuperWorld.WorldState negativeEffects = SuperWorld.WorldState.WORLD_STATE_HAS_WOOD;

                                    // Check preconditions
                                    if ((mPlanner.GetWorld().mWorldState & preconditions) == preconditions)
                                    {

                                        // If execution succeeded return "success". Otherwise return "failed".
                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            Debug.Log("Light fire");

                                            mPlanner.GetWorld().feller.GetComponent<PlayerController>().SetBagActive(false);

                                            // Apply positive & negative effects
                                            mPlanner.GetWorld().mWorldState |= positiveEffects;
                                            mPlanner.GetWorld().mWorldState &= ~negativeEffects;

                                            mTimeStartAction = Time.time;
                                            return Action.Result.SUCCESS;
                                        }
                                        else
                                        {
                                            // Action in progress
                                            return Action.Result.PROGRESS;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("--- ERROR: PLAN FAILED ---");
                                        return Action.Result.FAILED;
                                    }
                                }
                                else
                                {
                                    return Action.Result.FAILED;
                                }
                            })
                            { Label = "Light fire" },
                            // Action 1
                            new Action((bool cookMeat) =>
                            {
                                if (mPlan[mCurrentAction].mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_EAT_COOKED_MEAT)
                                {
                                    // COOK_MEAT action - preconditions & effects
                                    SuperWorld.WorldState preconditions   = SuperWorld.WorldState.WORLD_STATE_HAS_RAW_MEAT;
                                    SuperWorld.WorldState positiveEffects = SuperWorld.WorldState.WORLD_STATE_HAS_COOKED_MEAT;
                                    SuperWorld.WorldState negativeEffects = SuperWorld.WorldState.WORLD_STATE_FIRE_ON | SuperWorld.WorldState.WORLD_STATE_HAS_RAW_MEAT;

                                    // Check preconditions
                                    if ((mPlanner.GetWorld().mWorldState & preconditions) == preconditions)
                                    {
                                        // If execution succeeded return "success". Otherwise return "failed".
                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            Debug.Log("Meat cooked");

                                            // Apply positive & negative effects
                                            mPlanner.GetWorld().mWorldState |= positiveEffects;
                                            mPlanner.GetWorld().mWorldState &= ~negativeEffects;

                                            mTimeStartAction = Time.time;
                                            return Action.Result.SUCCESS;
                                        }
                                        else
                                        {
                                            // Action in progress
                                            return Action.Result.PROGRESS;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("--- ERROR: PLAN FAILED ---");
                                        return Action.Result.FAILED;
                                    }
                                }
                                else
                                {
                                    return Action.Result.FAILED;
                                }
                            })
                            { Label = "Cook meat" },
                            // Action 2
                            new Action((bool eatCookedMeat) =>
                            {
                                if (mPlan[mCurrentAction].mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_EAT_COOKED_MEAT)
                                {
                                    // EAT_COOKED_MEAT action - preconditions & effects
                                    SuperWorld.WorldState preconditions   = SuperWorld.WorldState.WORLD_STATE_HAS_COOKED_MEAT;
                                    SuperWorld.WorldState positiveEffects = SuperWorld.WorldState.WORLD_STATE_NONE;
                                    SuperWorld.WorldState negativeEffects = SuperWorld.WorldState.WORLD_STATE_HAS_COOKED_MEAT;

                                    // Check preconditions
                                    if ((mPlanner.GetWorld().mWorldState & preconditions) == preconditions)
                                    {
                                        // If execution succeeded return "success". Otherwise return "failed".
                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            Debug.Log("Ate cooked meat");

                                            // Apply positive & negative effects
                                            mPlanner.GetWorld().mWorldState |= positiveEffects;
                                            mPlanner.GetWorld().mWorldState &= ~negativeEffects;

                                            mTimeStartAction = Time.time;
                                            return Action.Result.SUCCESS;
                                        }
                                        else
                                        {
                                            // Action in progress
                                            return Action.Result.PROGRESS;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("--- ERROR: PLAN FAILED ---");
                                        return Action.Result.FAILED;
                                    }
                                }
                                else
                                {
                                    Debug.Log("--- ERROR: PLAN FAILED ---");
                                    return Action.Result.FAILED;
                                }
                            })
                            { Label = "Eat cooked meat" }
                            // Action 3
                          ),// Sequence - EAT COOKED MEAT

                          new Sequence(                             // BUILD COTTAGE
                            new Action((bool reachCottage) =>
                            {
                                if (mPlan[mCurrentAction].mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_BUILD_COTTAGE)
                                {
                                    // GO_TO_COTTAGE action - preconditions & effects
                                    SuperWorld.WorldState preconditions   = SuperWorld.WorldState.WORLD_STATE_NONE;
                                    SuperWorld.WorldState positiveEffects = SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_COTTAGE;
                                    SuperWorld.WorldState negativeEffects = SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_AXE | SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_TREE | SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_FOX;

                                    // Check preconditions
                                    if ((mPlanner.GetWorld().mWorldState & preconditions) == preconditions)
                                    {
                                        if (pathfinding.GetGrid().path == null)
                                        {
                                            // Set default layer for the pathfinding to be able to find a path
                                            mPlanner.GetWorld().cottage.layer = 0;
                                            pathfinding.GetGrid().UpdateGrid();

                                            if (pathfinding.InitPathfinding(mPlanner.GetWorld().feller.transform, mPlanner.GetWorld().cottage.transform))
                                            {
                                                //Set path to feller
                                                playerMovement.SetPath(pathfinding.GetGrid().path);
                                            }

                                            // Reset to unwalkable layer
                                            mPlanner.GetWorld().cottage.layer = 8;
                                            pathfinding.GetGrid().UpdateGrid();
                                        }

    // TODO: check if feller has arrived

                                        // If execution succeeded return "success". Otherwise return "failed".
                                        if (playerMovement.hasArrived)
                                        {
                                            Debug.Log("Near cottage");
                                            playerMovement.hasArrived = false;
                                            pathfinding.GetGrid().path = null;

                                            // Apply positive & negative effects
                                            mPlanner.GetWorld().mWorldState |= positiveEffects;
                                            mPlanner.GetWorld().mWorldState &= ~negativeEffects;

                                            mTimeStartAction = Time.time;
                                            return Action.Result.SUCCESS;
                                        }
                                        else
                                        {
                                            // Action in progress
                                            return Action.Result.PROGRESS;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("--- ERROR: PLAN FAILED ---");
                                        return Action.Result.FAILED;
                                    }
                                }
                                else
                                {
                                    return Action.Result.FAILED;
                                }
                            })
                            { Label = "Go to cottage" },
                            // Action 1
                            new Action((bool buildCottage) =>
                            {
                                if (mPlan[mCurrentAction].mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_BUILD_COTTAGE)
                                {
                                    // BUILD_COTTAGE action - preconditions & effects
                                    SuperWorld.WorldState preconditions   = SuperWorld.WorldState.WORLD_STATE_HAS_WOOD;
                                    SuperWorld.WorldState positiveEffects = SuperWorld.WorldState.WORLD_STATE_COTTAGE_BUILT;
                                    SuperWorld.WorldState negativeEffects = SuperWorld.WorldState.WORLD_STATE_HAS_WOOD;

                                    // Check preconditions
                                    if ((mPlanner.GetWorld().mWorldState & preconditions) == preconditions)
                                    {
                                        mPlanner.GetWorld().feller.GetComponent<Animator>().SetBool("lumbering", true);

                                        // If execution succeeded return "success". Otherwise return "failed".
                                        if (Time.time > (mTimeStartAction + mTimeActionLast * step / 4))
                                        {
                                            GameObject stepObject = mPlanner.GetWorld().cottage.transform.GetChild(step - 1).gameObject;
                                            stepObject.GetComponent<MeshRenderer>().enabled = true;
                                            mPlanner.GetWorld().feller.GetComponent<PlayerController>().SetBagActive(false);
                                            Debug.Log(step++);
                                            
                                            mTimeStartAction = Time.time;
                                        }

                                        if (step > 4)
                                        {
                                            Debug.Log("Cottage built");
                                            step = 1;
                                            mPlanner.GetWorld().feller.GetComponent<Animator>().SetBool("lumbering", false);

                                            // Apply positive & negative effects
                                            mPlanner.GetWorld().mWorldState |= positiveEffects;
                                            mPlanner.GetWorld().mWorldState &= ~negativeEffects;

                                            mTimeStartAction = Time.time;
                                            return Action.Result.SUCCESS;
                                        }
                                        else
                                        {
                                            // Action in progress
                                            return Action.Result.PROGRESS;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log(mPlanner.GetWorld().mWorldState);
                                        Debug.Log(preconditions);
                                        Debug.Log("--- ERROR: PLAN FAILED ---");
                                        return Action.Result.FAILED;
                                    }
                                }
                                else
                                {
                                    return Action.Result.FAILED;
                                }
                            })
                            { Label = "Build cottage" }
                            // Action 2
                          ),// Sequence - BUILD COTTAGE

                          new Sequence(                             // PICK UP AXE
                            new Action((bool reachAxe) =>
                            {
                                if (mPlan[mCurrentAction].mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_PICK_UP_AXE)
                                {
                                    // GO_TO_AXE action - preconditions & effects
                                    SuperWorld.WorldState preconditions   = SuperWorld.WorldState.WORLD_STATE_NONE;
                                    SuperWorld.WorldState positiveEffects = SuperWorld.WorldState.WORLD_STATE_CLOSE_TO_AXE;
                                    SuperWorld.WorldState negativeEffects = SuperWorld.WorldState.WORLD_STATE_NONE;

                                    // Check preconditions
                                    if ((mPlanner.GetWorld().mWorldState & preconditions) == preconditions)
                                    {
                                        if (pathfinding?.GetGrid()?.path == null)
                                        {
                                            // Set default layer for the pathfinding to be able to find a path
                                            mPlanner.GetWorld().axe.layer = 0;
                                            pathfinding.GetGrid().UpdateGrid();

                                            if(pathfinding.InitPathfinding(mPlanner.GetWorld().feller.transform, mPlanner.GetWorld().axe.transform))
                                            {
                                                //Set path to feller
                                                playerMovement.SetPath(pathfinding.GetGrid().path);
                                            }
                                            
                                            // Reset to unwalkable layer
                                            mPlanner.GetWorld().axe.layer = 8;
                                            pathfinding.GetGrid().UpdateGrid();
                                        }

                                        // If execution succeeded return "success". Otherwise return "failed".
                                        if (playerMovement.hasArrived)
                                        {
                                            Debug.Log("Near axe");
                                            playerMovement.hasArrived = false;
                                            pathfinding.GetGrid().path = null;

                                            // Apply positive & negative effects
                                            mPlanner.GetWorld().mWorldState |= positiveEffects;
                                            mPlanner.GetWorld().mWorldState &= ~negativeEffects;

                                            mTimeStartAction = Time.time;
                                            return Action.Result.SUCCESS;
                                        }
                                        else
                                        {
                                            // Action in progress
                                            return Action.Result.PROGRESS;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("--- ERROR: PLAN FAILED ---");
                                        return Action.Result.FAILED;
                                    }
                                }
                                else
                                {
                                    return Action.Result.FAILED;
                                }
                            })
                            { Label = "Go to axe" },
                            // Action 1
                            new Action((bool pickUpAxe) =>
                            {
                                if (mPlan[mCurrentAction].mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_PICK_UP_AXE)
                                {
                                    // PICK_UP_AXE action - preconditions & effects
                                    SuperWorld.WorldState preconditions   = SuperWorld.WorldState.WORLD_STATE_NONE;
                                    SuperWorld.WorldState positiveEffects = SuperWorld.WorldState.WORLD_STATE_AXE_OWNED;
                                    SuperWorld.WorldState negativeEffects = SuperWorld.WorldState.WORLD_STATE_NONE;

                                    // Check preconditions
                                    if ((mPlanner.GetWorld().mWorldState & preconditions) == preconditions)
                                    {
                                        // If execution succeeded return "success". Otherwise return "failed".
                                        if (Time.time > mTimeStartAction + mTimeActionLast)
                                        {
                                            Debug.Log("Axe owned");

                                            // Grab axe
                                            mPlanner.GetWorld().axe.SetActive(false);
                                            mPlanner.GetWorld().feller.GetComponent<PlayerController>().ActiveAxe();
                                            mPlanner.GetWorld().axe.layer = 0;
                                            pathfinding.GetGrid().UpdateGrid();


                                            // Apply positive & negative effects
                                            mPlanner.GetWorld().mWorldState |= positiveEffects;
                                            mPlanner.GetWorld().mWorldState &= ~negativeEffects;

                                            mTimeStartAction = Time.time;
                                            return Action.Result.SUCCESS;
                                        }
                                        else
                                        {
                                            // Action in progress
                                            return Action.Result.PROGRESS;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("--- ERROR: PLAN FAILED ---");
                                        return Action.Result.FAILED;
                                    }
                                }
                                else
                                {
                                    return Action.Result.FAILED;
                                }
                            })
                            { Label = "Pick up axe" }
                            // Action 2
                          ) // Sequence - PICK UP AXE
                        ) // Selector
                    ) // Secuence - popup action + execute it
                ) // Repeater (until fail)
            ) // Sequence - planning + repeater
        );

        // attach the debugger component if executed in editor (helps to debug in the inspector) 
#if UNITY_EDITOR
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = mBehaviorTree;
#endif

        mBehaviorTree.Start();
    }

    /****************************************************************************/

    // Update is called once per frame
    void Update()
    {

    }

    /****************************************************************************/

    public void OnDestroy()
    {
        StopBehaviorTree();
    }


    /****************************************************************************/

    public void StopBehaviorTree()
    {
        if (mBehaviorTree != null && mBehaviorTree.CurrentState == Node.State.ACTIVE)
        {
            mBehaviorTree.Stop();
        }
    }

    /****************************************************************************/
}
