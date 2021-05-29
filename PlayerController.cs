using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject playersAxe;
    public GameObject bag;

    [Range(1, 5)]
    public float walkSpeed;

    [HideInInspector] public bool hasArrived;
    [HideInInspector] public Animator animator;

    private List<NodePathfinding> path;
    private int i = 0;

    // Start is called before the first frame update
    void Start()
    {
        hasArrived = false;
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (path != null)
        {
            if (Vector3.Distance(this.transform.position, path[i].mWorldPosition) < 1f)
            {
                i++;
                if(i >= path.Count)
                {
                    hasArrived = true;
                    animator.SetBool("walking", false);
                    path = null;
                    return;
                }
            }

            this.transform.position = Vector3.MoveTowards(this.transform.position, path[i].mWorldPosition, walkSpeed * Time.deltaTime);
            this.transform.LookAt(path[path.Count - 1].mWorldPosition);
        }
    }

    public void SetPath(List<NodePathfinding> path)
    {
        this.path = path;
        i = 0;
        animator.SetBool("walking", true);
    }
    
    public void ActiveAxe()
    {
        playersAxe.SetActive(true);
    }

    public void SetBagActive(bool value)
    {
        bag.SetActive(value);
    }
}