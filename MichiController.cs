using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MichiController : MonoBehaviour
{
    public Pathfinding pathfinding;
    private Animator animator;
    private List<NodePathfinding> path;
    private int i;
    private Vector3 newPos;

    [Range(1, 5)] public float walkSpeed;

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        i = 0;
        path = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (path == null)   // Create new path
        {
            if(i != 0 && !animator.GetAnimatorTransitionInfo(0).IsName("miau -> idle")) return;
            // Calculate new random position
            float xDist = Random.Range(0.0f, 15.0f);
            float zDist = Random.Range(0.0f, 15.0f);

            newPos = new Vector3(xDist, 0.26f, zDist);
            //Debug.Log("michi newpos: " + newPos);
            animator.SetBool("walking", true);
            path = new List<NodePathfinding>();
            //path = pathfinding.FindPath(this.transform.position, newPos, -1);
        }
        else
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, newPos, walkSpeed * Time.deltaTime);
            this.transform.LookAt(newPos);

            if (Vector3.Distance(transform.position, newPos) < 4f)
            {
                path = null;
                animator.SetBool("walking", false);
                animator.SetTrigger("hasArrived");
            }

            //if (Vector3.Distance(this.transform.position, path[i].mWorldPosition) < 1f)
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
            //this.transform.LookAt(path[i].mWorldPosition);
        }
    }
}
