using UnityEngine;

public class simpleMovement : MonoBehaviour {
	public Vector3 speed = new Vector3(0,0,0);
	// Update is called once per frame
	void Update ()
	{
		transform.position+=speed*Time.timeScale;
	}
}
