using UnityEngine;

using System.Collections;

public class ObjectBuilderScript : MonoBehaviour 
{


	bool beenBuilt;

    //Your grid stuff
    public GameObject[,] nodes = new GameObject[10, 10];
	//this gets called when you press the button in the unity inspector

	public void BuildObject()
		{beenBuilt = true;
        //this is where you initialize all of your nodes/grid

        for(var i = 0; i < 10; i++)
        {
            for(var j = 0; j < 10; j++)
            {

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
                    //Gizmos.DrawWireSphere(nodes[i, j].location, 1.0f);
                }
            }
			//Gizmos.DrawWireSphere (nodes[i, j].position, .5f);

		}
	}
}

public class Node : MonoBehaviour
{
    public Vector3 location;
    public bool showNode;

    public Vector3 getLocation()
    {
        return location;
    }
}