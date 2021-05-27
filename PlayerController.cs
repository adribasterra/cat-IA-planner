﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject axe;
    [Range(1, 5)]
    public float walkSpeed;

    [HideInInspector]
    public bool hasArrived;

    private List<NodePathfinding> path;
    private int i = 0;

    // Start is called before the first frame update
    void Start()
    {
        hasArrived = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (path != null)
        {
            if(this.transform.position == path[i].mWorldPosition)
            {
                i++;
                if(i >= path.Count)
                {
                    hasArrived = true;
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
    }
    
    public void ActiveAxe()
    {
        axe.SetActive(true);
    }
}
