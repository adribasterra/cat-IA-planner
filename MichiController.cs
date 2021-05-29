using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MichiController : MonoBehaviour
{
    public Pathfinding pathfinding;
    private Animator animator;
    private List<NodePathfinding> path;
    private bool hasArrived;
    private int i;
    private Vector3 newPos;

    [Range(1, 5)] public float walkSpeed;

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        hasArrived = false;
        i = 0;
        path = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasArrived)   // Create new path
        {
            hasArrived = false;
            // Calculate new random position
            float xDist = Random.Range(0.0f, 15.0f);
            float zDist = Random.Range(0.0f, 15.0f);

            newPos = new Vector3(xDist, 0.2f, zDist);
            Debug.Log("michi newpos: " + newPos);
            animator.SetBool("walking", true);

            //Transform target = this.transform;
            //target.position = newPos;

            //if(pathfinding.InitPathfinding(this.transform, target))
            //{
            //    path = pathfinding.GetGrid().path;
            //    animator.SetBool("walking", true);
            //}
        }
        else
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, newPos, walkSpeed * Time.deltaTime);
            this.transform.LookAt(newPos);

            if(transform.position == newPos)
            {
                hasArrived = true;
                animator.SetBool("walking", false);
                animator.SetTrigger("hasArrived");
            }

            //if (this.transform.position == path[i].mWorldPosition)
            //{
            //    i++;
            //    if (i >= path.Count)
            //    {
            //        animator.SetBool("walking", false);
            //        animator.SetTrigger("hasArrived");
            //        path = null;
            //        i = 0;
            //        return;
            //    }
            //}

            //this.transform.position = Vector3.MoveTowards(this.transform.position, path[i].mWorldPosition, walkSpeed * Time.deltaTime);
            //this.transform.LookAt(path[path.Count - 1].mWorldPosition);

        }
    }
}
