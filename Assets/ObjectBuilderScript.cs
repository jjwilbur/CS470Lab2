using UnityEngine;

using System.Collections;

public class ObjectBuilderScript : MonoBehaviour 
{


	bool beenBuilt;

	//Your grid stuff




	//this gets called when you press the button in the unity inspector
	public void BuildObject()
		{beenBuilt = true;
		//this is where you initialize all of your nodes/grid




		}


	void OnDrawGizmos()
	{
		if (beenBuilt) {

			// Do a double for loop and draw all of your nodes in your grid

			//Gizmos.DrawWireSphere (nodes[i, j].position, .5f);

		}
	}
}