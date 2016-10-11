using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Priority_Queue;

public class node {
    public Vector2 point;
    public List<edge> edgesOut;
    public List<edge> edgesIn;

    public node() {
        point = new Vector2();
        edgesOut = new List<edge>();
        edgesIn = new List<edge>();
    }
    public node(Vector2 gPoint) {
        edgesOut = new List<edge>();
        edgesIn = new List<edge>();
        point = gPoint;
    }
    public void setPoint(Vector2 gPoint) {
        point = gPoint;
    }
    public float distance(Vector2 w) {
        return Mathf.Sqrt(Mathf.Pow((point.x - w.x), 2.0f) + Mathf.Pow((point.y - w.y), 2.0f));
    }
}

public class edge {
    public int start;
    public int end;
    public edge() {
        start = 0;
        end = 0;
    }
    public edge(int s, int e) {
        start = s;
        end = e;
    }
}


public class RobotController : MonoBehaviour {

	public float speed;
    GameObject grid;
	//Used to keep track of "forward direction" in manual control
	float rotationAngle = 0;
	//Indicates which way is "forward" in manual control
	GameObject followArrow;
	public RunType runtype; 
	public enum RunType{one, two, three};
    //Variables For rrt
    public List<node> nodes;
    System.Random rand;

    Node[,] gridNodes;
    Node curNode;
    SimplePriorityQueue<Node> aStar = new SimplePriorityQueue<Node>();
    Field goal;

    public Vector2[,] rrtNodes;
    public List<Vector2> trail;
    int percisionOfRrt;
    int atFollowPoint;
    float percisionOfRrtFloat;
    bool followRRT;
    bool first;


    // Use this for initialization
    void Start () {
        grid = GameObject.Find("GameObject");
        var builder = grid.GetComponent<ObjectBuilderScript>();
        gridNodes = builder.BuildObject();

		followArrow = GameObject.FindObjectOfType<ObjectFollow> ().gameObject;

        //rrtInit
        nodes = new List<node>();
        trail = new List<Vector2>();
        rand = new System.Random();


        if(runtype == RunType.three)
        {
            goal = getFieldofType(1)[0];
            Node n = new Node();
            n.distanceTraveled = 0;
            n.location = myLocation();
            n.visited = true;
            n.remainingDistance = distance(goal.getLocation());
            aStar.Enqueue(n, n.distanceTraveled + n.remainingDistance);
            makeAStar();
        }

        percisionOfRrt = 50;
        percisionOfRrtFloat = 50.0f;
        followRRT = true;
        rrtNodes = new Vector2[percisionOfRrt + 1, percisionOfRrt + 1];
        first = true;
        //makeRRt();
    }

    void makeAStar()
    {
        bool done = false;
        Node curLoc;
        while (!done)
        {
            curLoc = aStar.Dequeue();
            if(gridNodes[curLoc.i +1,curLoc.j] != null && !gridNodes[curLoc.i + 1, curLoc.j].visited)
            {
                Node n = gridNodes[curLoc.i + 1, curLoc.j];
                n.distanceTraveled = curLoc.distanceTraveled + distance(curNode.location, n.location);
                //n.location = myLocation();
                n.visited = true;
                n.prevNode = curLoc;
                n.remainingDistance = distance(goal.getLocation());
                aStar.Enqueue(n, n.distanceTraveled + n.remainingDistance);
            }
        }
    }

    //This is the start of RRT Code
    void makeTrail() {
        bool notAtEnd = true;
        node traverseNode = nodes.ElementAt(nodes.Count - 1);
        edge nextEdge = traverseNode.edgesIn.ElementAt(0);
        int nextNode = nextEdge.end;
        while (notAtEnd) {
            if (nextNode == 0) {
                notAtEnd = false;
            } else {
                trail.Add(traverseNode.point);
                traverseNode = nodes.ElementAt(nextNode);
                nextEdge = traverseNode.edgesIn.ElementAt(0);
                nextNode = nextEdge.end;
            }
        }
    }

    public bool pointInShapeTwo(Vector2 v, Vector2 w, Vector2 pt, float tolerance) {
        Vector2 lineVector = new Vector2(w.x - v.x, w.y - v.y);
        Vector2 normalized = new Vector2(w.x - v.x, w.y - v.y);
        normalized.Normalize();
        Vector2 startToPoint = (pt - v);
        float t = (startToPoint.x * normalized.x) + (startToPoint.y * normalized.y);
        t = Mathf.Max(0.0f, t);
        t = Mathf.Min(t, lineVector.magnitude);
        Vector2 projection = new Vector2();

        projection = v + ((t) * normalized);

        Vector2 distanceVect = new Vector2();
        distanceVect = pt - projection;

        if (distanceVect.magnitude < tolerance) {
            return true;
        } else {
            return false;
        }
    }

    void generateRrtNodes() {
        for (int i = 0; i < percisionOfRrt + 1; i++) {
            for (int j = 0; j < percisionOfRrt + 1; j++) {
                rrtNodes[i, j] = new Vector2(((42.0f / percisionOfRrtFloat) * j) - 21.0f, ((42.0f / percisionOfRrtFloat) * i) - 21.0f);
            }
        }
    }



    void makeRRt() {
        Vector3 start = myLocation();
        node starter = new node(new Vector2(start.x, start.z));
        nodes.Add(starter);
        bool success = false;
        bool hit = false;
        int closest;

        float hitDistance = 2.8f;
        List<Field> fields = getFieldofType(2);
        float dist;
        float xMax = -18.9f;
        float yMax = -18.9f;
        float distMax = 500.0f;
        Vector2 closePoint = new Vector2();
        while (!success) {
            Vector2 randpoint = getRandom();
            closest = findClosest(randpoint);
            hit = false;
            foreach (Field field in fields) {
                if (field.fieldType != 1) {
                    bool distLine = pointInShapeTwo(randpoint, nodes.ElementAt(closest).point, new Vector2(field.getLocation().x, field.getLocation().z), hitDistance);
                    if (distLine) {
                        hit = true;
                    }
                }
            }
            if (!hit) {
                int count = nodes.Count;
                node addNode = new node(randpoint);
                addNode.edgesOut.Add(new edge(closest, count));
                addNode.edgesIn.Add(new edge(count, closest));
                nodes.Add(addNode);
                nodes.ElementAt(closest).edgesOut.Add(new edge(closest, count));
                dist = distanceToEnd(addNode.point);
                if (randpoint.x > xMax) {
                    xMax = randpoint.x;
                }
                if (randpoint.y > yMax) {
                    yMax = randpoint.y;
                }
                if (dist < distMax) {
                    distMax = dist;
                    closePoint.x = randpoint.x;
                    closePoint.y = randpoint.y;
                }
                if (dist < 1.4f) {
                    success = true;
                }
            }
        }
    }

    public float distanceToEnd(Vector2 testPoint) {
        return Mathf.Sqrt(Mathf.Pow((18.9f - testPoint.x), 2.0f) + Mathf.Pow((18.9f - testPoint.y), 2.0f));
    }

    public float distance(Vector3 point)
    {
        return Mathf.Sqrt(Mathf.Pow((myLocation().x - point.x), 2.0f) + Mathf.Pow((myLocation().z - point.z), 2.0f));

    }

    public float distance (Vector3 first, Vector3 second)
    {
        return Mathf.Sqrt(Mathf.Pow((first.x - second.x), 2.0f) + Mathf.Pow((first.z - second.z), 2.0f));

    }

    Vector2 getRandom() { // randrom through X = -21 and y = -21 and x = 21 ,y = 21
        bool finished = false;
        bool pointNotChosen;
        float offset = 0.01f;
        int x;
        int z;
        Vector2 result = new Vector2();
        while (!finished) {
            pointNotChosen = true;
            x = rand.Next() % percisionOfRrt;
            z = rand.Next() % percisionOfRrt;
            result = rrtNodes[x, z];
            foreach (node point in nodes) {
                /*
                if (point.point.x == result.x && point.point.y > result.y) {
                    pointNotChosen = false;
                }
                 */
                if (point.point.x < (result.x + offset) && point.point.x > (result.x - offset) && point.point.y < (result.y + offset) && point.point.y > (result.y - offset)) {
                    pointNotChosen = false;
                }

            }
            if (pointNotChosen) {
                finished = true;
            }
        }
        return result;
    }

    int findClosest(Vector2 testNode) {
        int numberInArray = 0;
        float distance = float.MaxValue;
        for (int i = 0; i < nodes.Count; i++) {
            node test = nodes.ElementAt(i);
            if (test.distance(testNode) < distance) {
                distance = test.distance(testNode);
                numberInArray = i;
            }
        }
        return numberInArray;
    }

    float robotDistanceToPoint(Vector2 testPoint) {
        return Mathf.Sqrt(Mathf.Pow((myLocation().x - testPoint.x), 2.0f) + Mathf.Pow((myLocation().z - testPoint.y), 2.0f));
    }
    //This is the end of RRT Code









    // Update is called once per frame
    void Update () {
        //RRT Init
        if (first && followRRT) {
            first = false;
            generateRrtNodes();
            makeRRt();
            makeTrail();
            atFollowPoint = trail.Count - 1;
        }


        Vector3 toMove = new Vector3();// getManualInput ();
	
		//Alter the toMove variable here based on the varius fields in the scene.
		//You can set the runtype in Unity in the Inspector Window
		switch (runtype) {
		case RunType.one:
                toMove = new Vector3();
                var fields = getFieldsInRange();
                foreach(var field in fields) {

                    if(field.fieldType == 1) {
                            Vector3 vel = this.GetComponent<Rigidbody>().velocity;

                            toMove += new Vector3(-vel.x, -vel.y, -vel.z);
                            toMove += field.getLocation() - myLocation();
                       
                    }else if (field.fieldType == 2) {
                        
                        var angle = Mathf.Atan((field.getLocation().x - myLocation().x) / (field.getLocation().z - myLocation().z));
                        if (myLocation().x >= field.getLocation().x) {
                            Debug.Log("Above");
                            if (angle < 0) {
                                Debug.Log("Right");
                                toMove.x = toMove.x - Mathf.Sin(angle) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                                toMove.z = toMove.z - Mathf.Cos(angle) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                            } else {
                                toMove.x = toMove.x + Mathf.Sin(angle) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                                toMove.z = toMove.z + Mathf.Cos(angle) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                            }
                        } else {
                            Debug.Log("Below");
                            if (angle > 0) {
                                Debug.Log("Right");
                                toMove.x = toMove.x - Mathf.Sin(angle) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                                toMove.z = toMove.z - Mathf.Cos(angle) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                            } else {
                                toMove.x = toMove.x + Mathf.Sin(angle) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                                toMove.z = toMove.z + Mathf.Cos(angle) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                            }
                        }
                    }else if (field.fieldType == 3) {
                        var angle = Mathf.Atan((field.getLocation().x - myLocation().x) / (field.getLocation().z - myLocation().z));
                        if (myLocation().x >= field.getLocation().x) {
                            if (angle < 0) {
                                toMove.x = toMove.x - Mathf.Sin(angle + Mathf.PI/2) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                                toMove.z = toMove.z - Mathf.Cos(angle + Mathf.PI / 2) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                            } else {
                                toMove.x = toMove.x + Mathf.Sin(angle + Mathf.PI / 2) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                                toMove.z = toMove.z + Mathf.Cos(angle + Mathf.PI / 2) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                            }
                        } else {
                            if (angle > 0) {
                                toMove.x = toMove.x - Mathf.Sin(angle + Mathf.PI / 2) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                                toMove.z = toMove.z - Mathf.Cos(angle + Mathf.PI / 2) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                            } else {
                                toMove.x = toMove.x + Mathf.Sin(angle + Mathf.PI / 2) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                                toMove.z = toMove.z + Mathf.Cos(angle + Mathf.PI / 2) * (field.radius / (Vector3.Distance(field.getLocation(), myLocation()) / field.radius));
                            }
                        }
                    }
                }
                if (GetComponent<Rigidbody>().velocity != Vector3.zero)
                {
                    followArrow.transform.rotation = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity) * Quaternion.Euler(0, -90, 0);
                }
                break;
		case RunType.two:
                //For Part 2 of the lab
                //	readSonars ();
                var sonars = readSonars();
                for(var i = 0; i < sonars.Count; i++)
                {
                    float angle = i * 22;
                    if (sonars[i] < 4.0)
                    {
                        float x;
                        float z;
                        Debug.Log(angle);
                        CalcDistance(angle, sonars[i], out x, out z);
                        if((i * 22 - 90) < 0)
                        {
                            Vector3 AddVector = AddDistanceForce(angle, myLocation(), x, z);
                            toMove.x -= AddVector.x;
                            toMove.z += AddVector.z;
                        }
                        else if(( i * 22 - 180) < 0)
                        {
                            Vector3 AddVector = AddDistanceForce(angle, myLocation(), x, z);
                            toMove.x += AddVector.x;
                            toMove.z -= AddVector.z;
                        }
                        else if((i * 22 - 270) < 0)
                        {
                            Vector3 AddVector = AddDistanceForce(angle, myLocation(), x, z);
                            toMove.x += AddVector.x;
                            toMove.z -= AddVector.z;
                        }
                        else
                        {
                            Vector3 AddVector = AddDistanceForce(angle, myLocation(), x, z);
                            toMove.x -= AddVector.x;
                            toMove.z += AddVector.z;
                        }
                    }
                }
                toMove += getManualInput();
			break;

		case RunType.three:
               //toMove = new Vector3(0, 0, 0);
                //For whatever else
                //feel free to add more 
                //toMove = new Vector3(0, 0, 1);

                //This is the part of RRT Move
                if (followRRT && atFollowPoint != -1) {
                    Vector2 goPoint = trail.ElementAt(atFollowPoint);
                    toMove.x = goPoint.x - myLocation().x;
                    toMove.z = goPoint.y - myLocation().z;
                    if (robotDistanceToPoint(goPoint) < 0.5f) {
                        this.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        this.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                        atFollowPoint--;
                        if (atFollowPoint >= 0) {
                            goPoint = trail.ElementAt(atFollowPoint);
                            toMove.x = goPoint.x - myLocation().x;
                            toMove.z = goPoint.y - myLocation().z;
                            int moveAhead = 90;
                            toMove *= 10.0f;
                            for (int i = 0; i < moveAhead; i++) {
                                move(toMove);
                            }
                        }

                    }
                    if (GetComponent<Rigidbody>().velocity != Vector3.zero) {
                        followArrow.transform.rotation = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity) * Quaternion.Euler(0, -90, 0);
                    }
                }
                break;

		}

        Debug.Log(toMove);
		move (toMove);

        

	}

    public Vector3 moveTowards(Vector3 location) {
        Vector3 toMove = new Vector3();
        Vector3 vel = this.GetComponent<Rigidbody>().velocity;

        toMove += new Vector3(-vel.x, -vel.y, -vel.z);
        toMove += location - myLocation();

        return toMove;
    }

    public void stop() {
        Vector3 vel = GetComponent<Rigidbody>().velocity;
        vel = new Vector3(vel.x, vel.y,vel.z);
        Vector3 orig = new Vector3(vel.x, vel.y, vel.z);
        bool stopped = false;
        Vector3 toStop;
        while (!stopped) {
            toStop = -vel.normalized;// * Time.deltaTime;// * speed;
            vel += toStop;
            if (orig.x +.1 >= vel.x && orig.y + .1 >= vel.y && orig.z + .1 >= vel.z) {
                stopped = true;
            }
            //vel += toStop;
            move(toStop);
        }


    }


	// add a force to me in the given direction, this input force is normalized (made to have a magnitude of 1) for your convenience
	// you can control its total acceleration by altering the speed if you like.
	public void move(Vector3 direction)
	{
		this.GetComponent<Rigidbody>().AddForce( direction.normalized * Time.deltaTime * speed);

	}



	// Returns a List of 16 floats of the distance to a given object, the first float is point in the north direction,
	//and then they increment clockwise going around the circle. if nothing is detected, it will be a number of size 25
	// Example, an object to the north would be in the 0 index, a object to the east (90 degrees) would be in the 4th index and so on
	public List<float> readSonars() {

		List<float> myContacts = new List<float> ();
		//Vector3 loc = myLocation ();
		Vector3 fwd = new Vector3(1,0,0);

		for (int i = 0; i < 16; i++) {

			RaycastHit hitobj ;
			float distance = 20;
			if (Physics.Raycast ( transform.position, fwd, out hitobj, 25)) {
				Vector3 hitlocation = hitobj.point;
				distance = Vector3.Distance (hitlocation, transform.position);

				//Debug.Log (i + " hit at "+ (360 / 16) *(i));

			}
			myContacts.Add (distance);
			//Rotate the raycast by 22ish degrees.
			fwd = Quaternion.AngleAxis ((360 / 16), Vector3.up) * fwd;
		

		}
		return myContacts;



	}

	//Distance between me and a given field
	public float getDistance(Field f)
	{
		return Vector3.Distance(f.getLocation(), myLocation());
	}


	public Vector3 myLocation()
		{return this.transform.position;
		}


	//Get all Fields.
	public List<Field> getPowerFields()
	{List<Field> myList = new List<Field> ();
		foreach (Field f in GameObject.FindObjectsOfType<Field> ()) {
			myList.Add (f);

		}
		return myList;
	}

	//Only get fields that match the n fieldType in range of the robot .
	public List<Field> getFieldsInRange(int n)
	{
		List<Field> myList = new List<Field> ();
		foreach (Field f in GameObject.FindObjectsOfType<Field> ()) {
			if (f.getFieldType() == n  && Vector3.Distance(myLocation(), f.getLocation()) <= f.getRadius()) {
				myList.Add (f);			
			}
		}

		return myList;

	}
	//get all fields in range of the robot.
	public List<Field> getFieldsInRange()
	{
		List<Field> myList = new List<Field> ();
		foreach (Field f in GameObject.FindObjectsOfType<Field> ()) {
			if (Vector3.Distance(myLocation(), f.getLocation()) <= f.getRadius()) {
				myList.Add (f);			
			}
		}

		return myList;

	}
	//get all fields that match this type.
	public List<Field> getFieldofType(int n)
	{
		List<Field> myList = new List<Field> ();
		foreach (Field f in GameObject.FindObjectsOfType<Field> ()) {
			if (f.getFieldType() == n) {
				myList.Add (f);			
			}
		}

		return myList;

	}


	// get input from the AWSD keys.
	public Vector3 getManualInput()
	{Vector3 MoveDirection = Vector3.zero;
		if (Input.GetKey (KeyCode.W)) {
			MoveDirection += (Quaternion.Euler(0,rotationAngle,0)* Vector3.forward * Time.deltaTime * speed/2);
		}

		else if (Input.GetKey (KeyCode.S)) {
			MoveDirection += (Quaternion.Euler(0,rotationAngle,0)* Vector3.forward * Time.deltaTime * -speed/2);
		}

		if (Input.GetKey (KeyCode.D)) {
			rotationAngle += 180 * Time.deltaTime;
		}
		else if (Input.GetKey (KeyCode.A)) {
			rotationAngle -= 180 * Time.deltaTime;
		}
		followArrow.transform.rotation = Quaternion.Euler (0, rotationAngle -90, 0);

		return MoveDirection * 5;
	}


    public void CalcDistance(float angle, float distance, out float x, out float z)
    {
        angle %= 90;
        z = distance * Mathf.Sin(angle);
        x = distance * Mathf.Cos(angle);
    }


    public Vector3 AddDistanceForce(float angle, Vector3 myLocation, float x, float z)
    {
        Vector3 result = new Vector3();
        float radius = 4.0f;
        //(myLocation().x >= field.getLocation().x
        if (angle >= 90 && angle <= 270)
        {
            //Debug.Log("above");
            if (angle >= 180)
            {
                //Debug.Log("Right");
                result.x = result.x - Mathf.Sin((angle * Mathf.PI) / 180) * (Mathf.Pow(radius,2) / Mathf.Sqrt(Mathf.Pow(x,2) + Mathf.Pow(z,2)));
                result.z = result.z - Mathf.Cos((angle * Mathf.PI) / 180) * (Mathf.Pow(radius, 2) / Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(z, 2)));
            }
            else
            {
                //Debug.Log("Left");
                result.x = result.x + Mathf.Sin((angle * Mathf.PI) / 180) * (Mathf.Pow(radius, 2) / Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(z, 2)));
                result.z = result.z + Mathf.Cos((angle * Mathf.PI) / 180) * (Mathf.Pow(radius, 2) / Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(z, 2)));
            }
        }
        else
        {
            //Debug.Log("below");
            if (angle >= 180)
            {
                //Debug.Log("Right");
                result.x = result.x - Mathf.Sin((angle * Mathf.PI) / 180) * (Mathf.Pow(radius, 2) / Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(z, 2)));
                result.z = result.z - Mathf.Cos((angle * Mathf.PI) / 180) * (Mathf.Pow(radius, 2) / Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(z, 2)));
            }
            else
            {
                //Debug.Log("Left");

                result.x = result.x + Mathf.Sin((angle * Mathf.PI) / 180) * (Mathf.Pow(radius, 2) / Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(z, 2)));
                result.z = result.z + Mathf.Cos((angle * Mathf.PI) / 180) * (Mathf.Pow(radius, 2) / Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(z, 2)));
            }
        }
        //Debug.Log(result);
        return result;
    }

}
