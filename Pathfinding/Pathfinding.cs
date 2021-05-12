using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    public Transform mSeeker;
    public Transform mTarget;

    public bool EightConnectivity = true;
    public bool smoothPath;

    NodePathfinding CurrentStartNode;
    NodePathfinding CurrentTargetNode;

    Grid Grid;

    int Iterations = 0;
    float LastStepTime = 0.0f;
    float TimeBetweenSteps = 0.0001f;

    /***************************************************************************/

	void Awake()
    {
        Grid = GetComponent<Grid> ();

        Iterations = 0;
        LastStepTime = 0.0f;
    }

    /***************************************************************************/

    void Update()
    {
        // Positions changed?
        if( PathInvalid() )
        {
            // Remove old path
            if( Grid.path != null )
            {
                Grid.path.Clear();
            }
            // Start calculating path again
            Iterations = 0;
            if( TimeBetweenSteps == 20.0f )
            {
                Iterations = -1;
            }
            FindPath( mSeeker.position, mTarget.position, Iterations );
        }
        else
        {
            // Path found?
            if( Iterations >= 0 )
            {
                // One or more iterations?
                if( TimeBetweenSteps == 0.0f )
                {
                    // One iteration, look until path is found
                    Iterations = -1;
                    FindPath(mSeeker.position, mTarget.position, Iterations );
                }
                else if( Time.time > LastStepTime + TimeBetweenSteps )
                {
                    // Iterate increasing depth every time step
                    LastStepTime = Time.time;
                    Iterations++;
                    FindPath(mSeeker.position, mTarget.position, Iterations );
                }   
            }
        }
	}

    /***************************************************************************/

	bool PathInvalid()
    {
        return CurrentStartNode != Grid.NodeFromWorldPoint(mSeeker.position) || CurrentTargetNode != Grid.NodeFromWorldPoint(mTarget.position) ;
    }

    /***************************************************************************/

	public void FindPath( Vector3 startPos, Vector3 targetPos, int iterations )
    {
		CurrentStartNode  = Grid.NodeFromWorldPoint(startPos);
		CurrentTargetNode = Grid.NodeFromWorldPoint(targetPos);

		List<NodePathfinding> openSet = new List<NodePathfinding>();
		HashSet<NodePathfinding> closedSet = new HashSet<NodePathfinding>();
		openSet.Add(CurrentStartNode);
        Grid.openSet = openSet;

        int currentIteration = 0;
        NodePathfinding node = CurrentStartNode;

		while( openSet.Count > 0 && node != CurrentTargetNode && ( iterations == -1 || currentIteration < iterations ) )
        {
            // Select best node from open list
            node = openSet[0];
            
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost <= node.fCost && openSet[i].hCost < node.hCost)
                {
                    node = openSet[i];
                }
            }
            
            // Manage open/closed list
            openSet.Remove(node);
            closedSet.Add(node);
            Grid.openSet    = openSet;
            Grid.closedSet  = closedSet;

            // Check destination
            if (node != CurrentTargetNode)
            {
                // Open neighbours
                foreach (NodePathfinding neighbour in Grid.GetNeighbours(node, EightConnectivity))
                {
                    float dist = node.gCost + GetDistance(node, neighbour) * node.mCostMultiplier;

                    if (!neighbour.mWalkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }
                    else if(dist < neighbour.gCost /*neighbour.fCost < node.fCost*/ || !openSet.Contains(neighbour))
                    {
                        // set fcost of neighbour through g & h costs
                        neighbour.gCost = dist;
                        neighbour.hCost = Heuristic(neighbour, CurrentTargetNode);

                        neighbour.mParent = node;
                        if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
                    }
                }

                currentIteration++;
            }
            else
            {
                // Path found!
                RetracePath(CurrentStartNode, CurrentTargetNode);

                // Path found
                Iterations = -1;

                Debug.Log("Statistics:");
                Debug.LogFormat("\tTotal nodes:  {0}", openSet.Count + closedSet.Count );
                Debug.LogFormat("\tOpen nodes:   {0}", openSet.Count );
                Debug.LogFormat("\tClosed nodes: {0}", closedSet.Count );
            }
		}
	}

    /***************************************************************************/

	void RetracePath(NodePathfinding startNode, NodePathfinding endNode)
    {
	    List<NodePathfinding> path = new List<NodePathfinding>();

        NodePathfinding node = endNode;
        while(node != startNode)
        {
            path.Add(node);
            node = node.mParent;
        }

        path.Reverse();

        if(smoothPath) SmoothPath(path);

        Grid.path = path;
    }

    /***************************************************************************/

	float GetDistance(NodePathfinding nodeA, NodePathfinding nodeB)
    {
        float distance = 0.0f;
        float distanceBtwNodes = 1f;
        // Distance function
        float distX = nodeA.mGridX - nodeB.mGridX;
        float distY = nodeA.mGridY - nodeB.mGridY;

        if ( EightConnectivity )
        {
            //Euclidean distance
            distance = distanceBtwNodes * Mathf.Sqrt(distX * distX + distY * distY);
        }
        else
        {
            //Euclidean distance
            distance = distanceBtwNodes * Mathf.Abs(distX * distX + distY * distY);
        }

        return distance;
    }

    /***************************************************************************/

	float Heuristic(NodePathfinding nodeA, NodePathfinding nodeB)
    {
        //He utilizado una heurística de distancia * 1.1 o 2
        float distance = 0.0f;
        float distanceBtwNodes = 1f;
        float distanceBtwDiagonals = Mathf.Sqrt(distanceBtwNodes * 2);

        // Heuristic function
        //nodeA.mGridX   nodeB.mGridX
        //nodeA.mGridY   nodeB.mGridY
        float distX = nodeA.mGridX - nodeB.mGridX;
        float distY = nodeA.mGridY - nodeB.mGridY;

        if (EightConnectivity)
        {
            //distance = Mathf.Sqrt(distX * distX + distY * distY);
            distance = distanceBtwNodes * (distX + distY) + (distanceBtwDiagonals - 2 * distanceBtwNodes) * Mathf.Min(distX, distY);
        }
        else
        {
            // Manhattan
            distance = distanceBtwNodes * (Mathf.Abs(distX) + Mathf.Abs(distY));
        }
        
        return distance;
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
            if (BresenhamWalkable(node.mGridX, node.mGridY, path[i].mGridX, path[i].mGridY))
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

    //void SmoothPath(List<NodePathfinding> path)
    //{
    //    List<int> removables = new List<int>();
    //    NodePathfinding node = path[0];
    //    removables.Add(0);

    //    for(int i = 1; i<path.Count; i++)
    //    {
    //        if(BresenhamWalkable(node.mGridX, node.mGridY, path[i].mGridX, path[i].mGridY))
    //        {
    //            if (i + 1 < path.Count && BresenhamWalkable(node.mGridX, node.mGridY, path[i + 1].mGridX, path[i + 1].mGridY))
    //            {
    //                removables.Add(i);
    //            }
    //        }
    //        else
    //        {
    //            // Reset node
    //            node = path[i];
    //            removables.Add(i);
    //        }
    //    }

    //    for (int j = removables.Count-1; j > 0 ; j--)
    //    {
    //        int index = removables[j];
    //        path.Remove(path[index]);
    //    }
    //}

    /***************************************************************************/

    public bool BresenhamWalkable(int x, int y, int x2, int y2)
    {
        float originalCost = Grid.GetNode(x2, y2).gCost - Grid.GetNode(x, y).gCost;
        //Debug.Log("x: " + x + "\n\ty: " + y);

        // TODO: 4 Connectivity
        // TODO: Cost
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

            //Add cost
            totalCost += Grid.GetNode(x, y).gCost;

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

        //Debug.Log("Total: " + totalCost + "\n\tOriginal: " + originalCost);

        if (totalCost > originalCost)
        {
            return false;
        }


        return true;
    }

    /***************************************************************************/

    private void OnDrawGizmos()
    {
        if (Grid != null && Grid.path != null && smoothPath)
        {
            for (int i = 0; i < Grid.path.Count; i++)
            {
                if (i < Grid.path.Count - 1)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawLine(Grid.path[i].mWorldPosition, Grid.path[i + 1].mWorldPosition);
                }
            }
        }
    }
}
