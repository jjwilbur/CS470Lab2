using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public bool[,] buildGrid() {
        var size = GetComponent<Mesh>().bounds.size;
       // bool[,] grid = new bool[size.x,size.z];
        for (int i = 0; i <size.x; i++) {
            for (int j = 0; j < size.z; j++) {

            }
        }
        return null;
    }

}
