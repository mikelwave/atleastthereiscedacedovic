using UnityEngine;

//Used by IMJ for the bullet spray attack
public class RaycastLaser : MonoBehaviour {
	PlayerScript player;
	public LayerMask whatIsHittable;
	public Vector3 offset = new Vector3(0,0.1f,0);
	public bool debug = false;
	// Use this for initialization
	void Start ()
	{
		player = GameObject.Find("Player_main").GetComponent<PlayerScript>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		fireRay();
	}
	void fireRay()
	{
		RaycastHit2D ray = Physics2D.Raycast(transform.position,-transform.up,50f,whatIsHittable);
		RaycastHit2D ray2 = Physics2D.Raycast(transform.position,-(transform.up+offset),50f,whatIsHittable);
		RaycastHit2D ray3 = Physics2D.Raycast(transform.position,-(transform.up-offset),50f,whatIsHittable);
		if(ray.collider!=null&&ray.collider.tag=="Player"
		||ray2.collider!=null&&ray2.collider.tag=="Player"
		||ray3.collider!=null&&ray3.collider.tag=="Player")
		{
			if(player.invFrames==0)
			player.Damage(true,false);
		}

		if(debug)
		{
		if(ray.collider!=null)
		Debug.DrawLine(transform.position,ray.point,Color.red);
		else
		Debug.DrawLine(transform.position,transform.position-(transform.up*20),Color.red);

		if(ray2.collider!=null)
		Debug.DrawLine(transform.position,ray2.point,Color.green);
		else
		Debug.DrawLine(transform.position,transform.position-((transform.up+offset)*20),Color.green);

		if(ray3.collider!=null)
		Debug.DrawLine(transform.position,ray3.point,Color.yellow);
		else
		Debug.DrawLine(transform.position,transform.position-((transform.up-offset)*20),Color.yellow);
		}
	}
}
