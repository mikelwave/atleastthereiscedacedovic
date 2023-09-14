using UnityEngine;
using System.Collections;

public class spikeyBallScript : MonoBehaviour {
	[HideInInspector]
	public cultist_AI cultist;
	public bool spawned = false;
	public LayerMask whatIsGround;
	public int ID;
	public float direction = 1;
	void OnEnable()
	{
		GetComponent<Gravity>().enabled = true;
		if(c!=null)StopCoroutine(c);
	}
	void OnDisable()
	{
		GetComponent<Gravity>().enabled = false;
		spawned = false;
	}
	void turnIntoEnemy()
	{
		cultist.spawnSpikey(new Vector3(transform.position.x,Mathf.RoundToInt(transform.position.y),transform.position.z),ID,direction);
	}
	void OnCollisionStay2D(Collision2D other)
	{
		if(other.gameObject.tag == "Ground"||other.gameObject.tag == "Harm"||other.gameObject.tag=="semiSolid")
		{
			if(!spawned)
			{
				RaycastHit2D ray = Physics2D.Raycast(transform.position+new Vector3(-0.5f,0,0),Vector2.down,1.5f,whatIsGround);
				RaycastHit2D ray2 = Physics2D.Raycast(transform.position+new Vector3(0.5f,0,0),Vector2.down,1.5f,whatIsGround);
				if(ray.collider!=null||ray2.collider!=null)
				{
					//Debug.Log("spikyBugActivated");
					if(!spawned)
					turnIntoEnemy();
					spawned = true;
				}
			}
		}
	}
	Coroutine c;
	IEnumerator waitforDisappear()
	{
		yield return new WaitForSeconds(0.5f);
		gameObject.SetActive(false);
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name=="deathZone"&&c==null)
		{
			//print("a");
			if(c!=null)StopCoroutine(c);
			c = StartCoroutine(waitforDisappear());
		}
	}
}
