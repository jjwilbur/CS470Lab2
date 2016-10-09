using UnityEngine;

using System.Collections;

public class ObjectBuilderScript : MonoBehaviour 
{
	bool beenBuilt;
    //Your grid stuff
    public Node[,] nodes = new Node[10, 10];
	//this gets called when you press the button in the unity inspector

	public void BuildObject()
		{beenBuilt = true;
        //this is where you initialize all of your nodes/grid

        for(var i = 0; i < 10; i++)
        {
            for(var j = 0; j < 10; j++)
            {
                nodes[i, j] = new Node();
                nodes[i, j].location.x = (float)(((i * 4.2) + 2.1) - 21);
                nodes[i, j].location.y = 3.0f;
                nodes[i, j].location.z = (float)(((j * 4.2) + 2.1) - 21);
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
                    Gizmos.DrawWireSphere(nodes[i, j].location, 0.1f);
                }
            }
		}
	}
}

public class Node : MonoBehaviour
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