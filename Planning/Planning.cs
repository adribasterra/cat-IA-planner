﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Planning : MonoBehaviour
{
    [HideInInspector] public SuperWorld superWorld;
    NodePlanning CurrentStartNode;
    NodePlanning CurrentTargetNode;

    World mWorld;

    /***************************************************************************/

    void Awake()
    {
        mWorld = GetComponent<World>();
        superWorld = this.GetComponent<SuperWorld>();
    }

    /***************************************************************************/

    public List<NodePlanning> GetPlan()
    {
        FindPlan(SuperWorld.WorldState.WORLD_STATE_NONE, SuperWorld.WorldState.WORLD_STATE_COTTAGE_BUILT);

        return mWorld.plan;
    }

    /***************************************************************************/

    void Update()
    {
    }

    /***************************************************************************/

    public List<NodePlanning> FindPlan(SuperWorld.WorldState startWorldState, SuperWorld.WorldState targetWorldState)
    {
        CurrentStartNode = new NodePlanning(startWorldState, null);
        CurrentTargetNode = new NodePlanning(targetWorldState, null);

        List<NodePlanning> openSet = new List<NodePlanning>();
        HashSet<NodePlanning> closedSet = new HashSet<NodePlanning>();
        openSet.Add(CurrentStartNode);
        mWorld.openSet = openSet;

        NodePlanning node = CurrentStartNode;
        while (openSet.Count > 0 && ((node.mWorldState & CurrentTargetNode.mWorldState) != CurrentTargetNode.mWorldState))
        {
            // Select best node from open list
            node = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < node.fCost || (openSet[i].fCost == node.fCost && openSet[i].hCost < node.hCost))
                {
                    node = openSet[i];
                }
            }

            // Manage open/closed list
            openSet.Remove(node);
            closedSet.Add(node);
            mWorld.openSet = openSet;
            mWorld.closedSet = closedSet;

            // Check destination
            if (((node.mWorldState & CurrentTargetNode.mWorldState) != CurrentTargetNode.mWorldState))
            {

                // Open neighbours
                foreach (NodePlanning neighbour in mWorld.GetNeighbours(node))
                {
                    if ( /*!neighbour.mWalkable ||*/ closedSet.Any(n => n.mWorldState == neighbour.mWorldState))
                    {
                        continue;
                    }
                    //Debug.Log(neighbour.mAction.mActionType);
                    if (neighbour.mAction.mActionType == ActionPlanning.ActionType.ACTION_TYPE_BUILD_COTTAGE)
                    {
                        superWorld.timesLeftToBuildCottage = 0;
                        //Debug.Log(mWorld.timesLeftToBuildCottage);
                    }

                    float newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
                    if (newCostToNeighbour < neighbour.gCost || !openSet.Any(n => n.mWorldState == neighbour.mWorldState))
                    {
                        neighbour.gCost = newCostToNeighbour;
                        neighbour.hCost = Heuristic(neighbour, CurrentTargetNode);
                        neighbour.mParent = node;

                        if (!openSet.Any(n => n.mWorldState == neighbour.mWorldState))
                        {
                            openSet.Add(neighbour);
                            mWorld.openSet = openSet;
                        }
                        else
                        {
                            // Find neighbour and replace
                            openSet[openSet.FindIndex(x => x.mWorldState == neighbour.mWorldState)] = neighbour;
                        }
                    }
                }
            }
            else
            {
                // Path found!

                // End node must be copied
                CurrentTargetNode.mParent = node.mParent;
                CurrentTargetNode.mAction = node.mAction;
                CurrentTargetNode.gCost = node.gCost;
                CurrentTargetNode.hCost = node.hCost;

                RetracePlan(CurrentStartNode, CurrentTargetNode);

                Debug.Log("Statistics:");
                Debug.LogFormat("Total nodes:  {0}", openSet.Count + closedSet.Count);
                Debug.LogFormat("Open nodes:   {0}", openSet.Count);
                Debug.LogFormat("Closed nodes: {0}", closedSet.Count);
            }
        }


        if (mWorld.plan != null)
        {
            // Log plan
            Debug.Log("PLAN FOUND!");

            for (int i = 0; i < mWorld.plan.Count; ++i)
            {
                Debug.LogFormat("{0} Action cost: {1}, Accumulated cost: {2}", mWorld.plan[i].mAction.mName, mWorld.plan[i].mAction.mCost, mWorld.plan[i].gCost);
            }
        }
        else
        {
            Debug.Log("ERROR: IMPOSSIBLE TO FIND PLAN");
        }

        return mWorld.plan;
    }

    /***************************************************************************/

    void RetracePlan(NodePlanning startNode, NodePlanning endNode)
    {
        List<NodePlanning> plan = new List<NodePlanning>();

        NodePlanning currentNode = endNode;

        while (currentNode != startNode)
        {
            plan.Add(currentNode);
            currentNode = currentNode.mParent;
        }
        plan.Reverse();

        mWorld.plan = plan;
    }

    /***************************************************************************/

    float GetDistance(NodePlanning nodeA, NodePlanning nodeB)
    {
        // Distance function
        return nodeB.mAction.mCost;
    }

    /***************************************************************************/

    public SuperWorld GetWorld()
    {
        return superWorld;
    }

    /***************************************************************************/

    float Heuristic(NodePlanning nodeA, NodePlanning nodeB)
    {
        // Heuristic function
        return -World.PopulationCount((int)(nodeA.mWorldState | nodeB.mWorldState)) - World.PopulationCount((int)(nodeA.mWorldState & nodeB.mWorldState));
    }

    /***************************************************************************/

}
