using UnityEngine;

public class Gravity : MonoBehaviour {
	public Vector2 pushForces = new Vector2(0,-0.4f);
	public Vector2 savedPushForces = new Vector2(0,-0.4f);
	public Vector2 maxVelocities = new Vector2(10,10);
	public Vector2 savedMaxVelocities = new Vector2(10,10);
	public Rigidbody2D rb;
	float divider = 1;
	// Use this for initialization
	void Start () {
		if(rb==null)
		rb=GetComponent<Rigidbody2D>();
		savedPushForces = pushForces;
		savedMaxVelocities = maxVelocities;
		divider = GameObject.Find("_GM").GetComponent<GameData>().gravityDivider;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		if(Time.timeScale!=0)
		{
			rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x,-maxVelocities.x,maxVelocities.x),Mathf.Clamp(rb.velocity.y,-maxVelocities.y,maxVelocities.y));

			if(rb.velocity.y < maxVelocities.y && rb.velocity.y > -maxVelocities.y)
			{
				rb.velocity = new Vector2(rb.velocity.x,rb.velocity.y+(pushForces.y/divider));
				//rb.AddForce(new Vector2(0,pushForces.y),ForceMode2D.Impulse);
			}
			if(rb.velocity.x < maxVelocities.x && rb.velocity.x > -maxVelocities.x)
			{
				rb.velocity = new Vector2(rb.velocity.x+(pushForces.x/divider),rb.velocity.y);
			}
			if(rb.velocity.y >= maxVelocities.y)
			{
				rb.velocity = new Vector2(rb.velocity.x,rb.velocity.y-0.1f);
			}

		}
	}
}
