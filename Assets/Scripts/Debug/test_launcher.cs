using UnityEngine;

public class test_launcher : MonoBehaviour {
	public bool launch;
	public bool reset;
	public Vector2 velocity;
	public float angVelocity;
	Rigidbody2D rb;
	Vector3 position;
	// Use this for initialization
	void Start ()
	{
		rb = GetComponent<Rigidbody2D>();
		position = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if(launch)
		{
			launch = false;
			rb.velocity = velocity;
			rb.angularVelocity = angVelocity;
		}
		if(reset)
		{
			reset = false;
			transform.position = position;
			rb.velocity = Vector2.zero;
			rb.angularVelocity = 0;
		}
	}
}
