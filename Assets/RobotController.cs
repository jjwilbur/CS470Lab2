using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RobotController : MonoBehaviour {

	public float speed;

	//Used to keep track of "forward direction" in manual control
	float rotationAngle = 0;
	//Indicates which way is "forward" in manual control
	GameObject followArrow;
	public RunType runtype; 
	public enum RunType{one, two, three};

	// Use this for initialization
	void Start () {
		followArrow = GameObject.FindObjectOfType<ObjectFollow> ().gameObject;
	}



	// Update is called once per frame
	void Update () {

		Vector3 toMove = getManualInput ();
	
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
                toMove = new Vector3(1, 0, 1);
			//For whatever else
			//feel free to add more 

			break;

		}

        Debug.Log(toMove);
		move (toMove);

        

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
