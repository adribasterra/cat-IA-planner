﻿using UnityEngine;
using System.Collections;

public class ActionPlanning
{
    public ActionType mActionType;
    public World.WorldState mPreconditions;
    public World.WorldState mNegativePreconditions;
    public World.WorldState mEffects;
    public World.WorldState mNegativeEffects;
    public float mCost;
    public float mDynamicCost;
    public string mName;


    /***************************************************************************/

    public enum ActionType
    {
        ACTION_TYPE_NONE = -1,
        ACTION_TYPE_PICK_UP_AXE,
        ACTION_TYPE_HAUNT,
        ACTION_TYPE_FELL_TREE,
        ACTION_TYPE_EAT_RAW_MEAT,
        ACTION_TYPE_EAT_COOKED_MEAT,
        ACTION_TYPE_BUILD_COTTAGE,

        //Actions of behaviour tree
        ACTION_TYPE_GO_TO_AXE,
        ACTION_TYPE_GO_TO_TREE,
        ACTION_TYPE_GO_TO_FOX,
        ACTION_TYPE_KILL_FOX,
        ACTION_TYPE_COLLECT_WOOD,
        ACTION_TYPE_LIGHT_FIRE,
        ACTION_TYPE_COOK_MEAT,
        ACTION_TYPE_DROP_OFF_AXE,
        ACTION_TYPES
    }

    /***************************************************************************/

    public ActionPlanning(ActionType actionType, World.WorldState preconditions, World.WorldState negativePreconditions, World.WorldState effects, World.WorldState negativeEffects, float cost, string name)
    {
        mActionType = actionType;
        mPreconditions = preconditions;
        mNegativePreconditions = negativePreconditions;
        mNegativeEffects = negativeEffects;
        mEffects = effects;
        mCost = cost;
        mName = name;
    }

    /***************************************************************************/

}
