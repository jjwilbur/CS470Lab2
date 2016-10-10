using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

using System.Collections;

public class ObjectBuilderScript : MonoBehaviour 
{
	bool beenBuilt;
    //Your grid stuff
    public Node[,] nodes = new Node[10, 10];
	//this gets called when you press the button in the unity inspector

	public void BuildObject() {
        beenBuilt = true;
        nodes = new Node[10, 10];
        //this is where you initialize all of your nodes/grid

        for(var i = 0; i < 10; i++)
        {
            for(var j = 0; j < 10; j++)
            {
                nodes[i, j] = new Node();
                nodes[i, j].location.x = (float)(((i * 4.2) + 2.1) - 21);
                nodes[i, j].location.y = 1.5f;
                nodes[i, j].location.z = (float)(((j * 4.2) + 2.1) - 21);
                if (Physics.CheckSphere(nodes[i, j].location, .1f)) {
                    nodes[i, j] = null;
                }
            }
        }
	}

	void OnDrawGizmos()
	{
		if (beenBuilt) {
			// Do a double for loop and draw all of your nodes in your grid
            for(var i = 0; i < 10; i++)
            {
                for(var j = 0; j < 10; j++)
                {
                    if (nodes[i, j] != null)
                        Gizmos.DrawWireSphere(nodes[i, j].location, 0.1f);
                }
            }
        } else {
            BuildObject();
        }
	}
}

public class Node 
{
    public Vector3 location;
    public bool showNode;

    public Node()
    {
        location = new Vector3(0, 0, 0);
        showNode = true;
    }
    public Vector3 getLocation()
    {
        return location;
    }
}