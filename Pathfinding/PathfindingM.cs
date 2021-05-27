using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathfindingM : MonoBehaviour
{

    public Transform mSeeker;
    public Transform mTarget;

    NodePathfinding CurrentStartNode;
    NodePathfinding CurrentTargetNode;

    Grid Grid;

    public int Iterations = -1;
    float LastStepTime = 0.0f;
    float TimeBetweenSteps = 0.01f;

    public bool EightConnectivity = true;
    public bool SmoothingPath = true;

    /***************************************************************************/

    void Awake()
    {
        Grid = GetComponent<Grid>();

        Iterations = -1;
        LastStepTime = 0.0f;
    }

    /***************************************************************************/

    void Update()
    {
        /*// Positions changed?
        if (PathInvalid())
        {
            // Remove old path
            if (Grid.path != null)
            {
                Grid.path.Clear();
            }
            // Start calculating path again
            Iterations = 0;
            if (TimeBetweenSteps == 0.0f)
            {
                Iterations = -1;
            }
            FindPath(mSeeker.position, mTarget.position, Iterations);
        }
        else
        {
            // Path found?
            if (Iterations >= 0)
            {
                // One or more iterations?
                if (TimeBetweenSteps == 0.0f)
                {
                    // One iteration, look until path is found
                    Iterations = -1;
                    FindPath(mSeeker.position, mTarget.position, Iterations);
                }
                else if (Time.time > LastStepTime + TimeBetweenSteps)
                {
                    // Iterate increasing depth every time step
                    LastStepTime = Time.time;
                    Iterations++;
                    FindPath(mSeeker.position, mTarget.position, Iterations);
                }
            }
        }*/
    }

    /***************************************************************************/
    public void DoPathfinding(Transform origin, Transform target)
    {
        mSeeker = origin;
        mTarget = target;
        if (PathInvalid())
        {
            // Remove old path
            if (Grid.path != null)
            {
                Grid.path.Clear();
            }
            // Start calculating path again
            Iterations = -1;
            if (TimeBetweenSteps == 0.0f)
            {
                Iterations = -1;
            }
            FindPath(mSeeker.position, mTarget.position, Iterations);
        }
        else
        {
            // Path found?
            if (Iterations >= 0)
            {
                // One or more iterations?
                if (TimeBetweenSteps == 0.0f)
                {
                    // One iteration, look until path is found
                    Iterations = -1;
                    FindPath(mSeeker.position, mTarget.position, Iterations);
                }
                else if (Time.time > LastStepTime + TimeBetweenSteps)
                {
                    // Iterate increasing depth every time step
                    LastStepTime = Time.time;
                    Iterations++;
                    FindPath(mSeeker.position, mTarget.position, Iterations);
                }
            }
        }
    }
    bool PathInvalid()
    {
        return CurrentStartNode != Grid.NodeFromWorldPoint(mSeeker.position) || CurrentTargetNode != Grid.NodeFromWorldPoint(mTarget.position);
    }

    /***************************************************************************/

    void FindPath(Vector3 startPos, Vector3 targetPos, int iterations)
    {
        CurrentStartNode = Grid.NodeFromWorldPoint(startPos);
        CurrentTargetNode = Grid.NodeFromWorldPoint(targetPos);

        List<NodePathfinding> openSet = new List<NodePathfinding>();
        HashSet<NodePathfinding> closedSet = new HashSet<NodePathfinding>();
        openSet.Add(CurrentStartNode);
        Grid.openSet = openSet;

        int currentIteration = 0;
        NodePathfinding node = CurrentStartNode;
        while (openSet.Count > 0 && node != CurrentTargetNode && (iterations == -1 || currentIteration < iterations))
        {
            // Select best node from open list
            node = openSet[0];

            /****/
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost <= node.fCost && openSet[i].hCost < node.hCost)
                {
                    node = openSet[i];
                }
            }
            /****/

            // Manage open/closed list
            openSet.Remove(node);
            closedSet.Add(node);
            Grid.openSet = openSet;
            Grid.closedSet = closedSet;



            // Check destination
            if (node != CurrentTargetNode)
            {

                // Open neighbours
                foreach (NodePathfinding neighbour in Grid.GetNeighbours(node, EightConnectivity))
                {
                    /****/
                    if (!neighbour.mWalkable || closedSet.Contains(neighbour)) continue;

                    float newMovementCostToNeighbour = node.gCost /* * node.mCostMultiplier */ + Heuristic(node, neighbour) * node.mCostMultiplier;
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = Heuristic(neighbour, CurrentTargetNode);
                        neighbour.mParent = node;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                    /****/

                }
                currentIteration++;
            }
            else
            {
                // Path found!
                RetracePath(CurrentStartNode, CurrentTargetNode);

                // Path found
                Iterations = -1;
                /*Debug.Log("Pathfinfding Statistics:");
                Debug.LogFormat("Total nodes:  {0}", openSet.Count + closedSet.Count);
                Debug.LogFormat("Open nodes:   {0}", openSet.Count);
                Debug.LogFormat("Closed nodes: {0}", closedSet.Count);
                Debug.LogFormat("Path nodes: {0}", Grid.path.Count);*/
            }
        }
    }

    /***************************************************************************/

    void RetracePath(NodePathfinding startNode, NodePathfinding endNode)
    {
        List<NodePathfinding> path = new List<NodePathfinding>();

        /****/
        NodePathfinding currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.mParent;
        }
        path.Reverse();
        if (SmoothingPath) SmoothPath(path);
        /****/
        Grid.path = path;
    }

    /***************************************************************************/

    float GetDistance(NodePathfinding nodeA, NodePathfinding nodeB)
    {
        // Distance function
        float dstX = nodeA.mGridX - nodeB.mGridX;
        float dstY = nodeA.mGridY - nodeB.mGridY;
        if (EightConnectivity)
        {
            /****/
            return Mathf.Sqrt(dstX * dstX + dstY * dstY);
            /****/
        }
        else
        {
            /****/
            //return dstX * dstX + dstY * dstY;
            return Mathf.Abs(dstX) + Mathf.Abs(dstY);
            /****/
        }
    }

    /***************************************************************************/

    float Heuristic(NodePathfinding nodeA, NodePathfinding nodeB)
    {
        // Heuristic function
        float dstX = nodeA.mGridX - nodeB.mGridX;
        float dstY = nodeA.mGridY - nodeB.mGridY;
        /****/
        if (EightConnectivity)
        {
            /****/
            return Mathf.Sqrt(dstX * dstX + dstY * dstY);
            /****/
        }
        else
        {
            /****/
            //return dstX * dstX + dstY * dstY;
            return Mathf.Abs(dstX) + Mathf.Abs(dstY);
            /****/
        }

        /****/
    }

    /***************************************************************************/
    void SmoothPath(List<NodePathfinding> path)
    {
        List<int> smoothNodes = new List<int>();
        NodePathfinding node = path[0];
        smoothNodes.Add(0);
        int i = path.Count;
        while (node != path[path.Count - 1])
        {
            i--;
            if (BresenhamWalkable(node.mGridX, node.mGridY, path[i].mGridX, path[i].mGridY, node.hCost))
            {
                smoothNodes.Add(i);
                node = path[i];
                i = path.Count;
            }
        }

        for (int j = path.Count - 1; j > 0; j--)
        {
            if (!smoothNodes.Contains(j))
            {
                path.Remove(path[j]);
            }
        }
    }
    /*private void OnDrawGizmos()
    {
        if (Grid.path != null && SmoothingPath)
        {
            for (int i = 0; i < Grid.path.Count; i++)
            {
                if (i < Grid.path.Count - 1)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(Grid.path[i].mWorldPosition, Grid.path[i + 1].mWorldPosition);
                }
            }
        }
    }*/
    /***************************************************************************/

    public bool BresenhamWalkable(int x, int y, int x2, int y2, float originalCost)
    {
        // TODO: 4 Connectivity
        // TODO: Cost
        Debug.Log("Original Cost:" + originalCost);
        float totalCost = 0f;

        int xDist = x2 - x;
        int yDist = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        // dx
        if (xDist < 0) dx1 = -1; else if (xDist > 0) dx1 = 1;
        if (xDist < 0) dx2 = -1; else if (xDist > 0) dx2 = 1;
        // dy
        if (yDist < 0) dy1 = -1; else if (yDist > 0) dy1 = 1;

        int longest = Mathf.Abs(xDist);
        int shortest = Mathf.Abs(yDist);
        if (!(longest > shortest))
        {
            longest = Mathf.Abs(yDist);
            shortest = Mathf.Abs(xDist);
            if (yDist < 0)
            {
                dy2 = -1;
            }
            else if (yDist > 0)
            {
                dy2 = 1;
            }
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            if (!Grid.GetNode(x, y).mWalkable)
            {
                return false;
            }

            //////Add cost
            totalCost += Grid.GetNode(x, y).mCostMultiplier;

            if (totalCost > originalCost)
            {
                return false;
            }

            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }
        Debug.Log("Total Cost:" + totalCost);
        //Debug.Log("Total: " + totalCost + "\n\tOriginal: " + originalCost);


        return true;
    }

    public Grid GetGrid()
    {
        return Grid;
    }
}
