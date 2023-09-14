using System.Collections;
using UnityEngine;

public class skeletonPlantScript : MonoBehaviour {
	public bool startOut = false;
	bool playerNear = false;
	Vector2 bounds = new Vector2(-1.5f,1.5f);
	Vector3 startPoint = Vector3.zero;
	public Vector2 distance = new Vector2(0,2f);
	public float speed = 10f;
	bool active;
	bool setstart = false;
	public bool canAttack = false;
	Transform player;
	Coroutine cor;
	EnemyCorpseSpawner eneC;
	BoxCollider2D col;
	SpriteRenderer render;
	bool ignorePlayer = false;
	public Color hardColor = new Color(1,0.37348f,0.259434f,1);
	// Use this for initialization
	void Assign() {
		if(player==null)
		{
		player = GameObject.Find("Player_main").transform;
		eneC = transform.parent.GetComponent<EnemyCorpseSpawner>();
		if(render==null)
		render = GetComponent<SpriteRenderer>();
		//print(transform.localPosition);
		col = GetComponent<BoxCollider2D>();
		if(!startOut && !setstart)
		{
			setstart = true;
			transform.position = new Vector3(transform.position.x-distance.x,transform.position.y-distance.y,transform.position.z);
			if(distance.x==0)
			bounds+= new Vector2(transform.position.x,transform.position.x);
			else bounds+= new Vector2(transform.position.y,transform.position.y);
			startPoint = transform.position;
			col.enabled = false;
		}
		else if(startOut && !setstart)
		{
			setstart = true;
			bounds+= new Vector2(transform.position.x,transform.position.x);
			startPoint = transform.position;
		}
		}
	}
	public void hardMode()
	{
		if(render==null)render = GetComponent<SpriteRenderer>();
		render.color = hardColor;
		ignorePlayer = true;
	}
	void CheckForPlayer() {
		if(distance.x==0)
		{
			if(player.position.x>=bounds.x&&player.position.x<bounds.y&&!playerNear)
			{
				playerNear = true;
			}
			else if(player.position.x<bounds.x||player.position.x>bounds.y)
			{
				if(playerNear)
				playerNear = false;
			}
		}
		//if the distance is horizontal, check vertically
		else
		{
			if(player.position.y>=bounds.x&&player.position.y<bounds.y&&!playerNear)
			{
				playerNear = true;
			}
			else if(player.position.y<bounds.x||player.position.y>bounds.y)
			{
				if(playerNear)
				playerNear = false;
			}
		}
	}
	void OnEnable()
	{
		if(player == null)
			Assign();

		active = true;
		transform.position = startPoint;
		if(startOut)
		{
			eneC.canDie = true;
		}
		if(cor!=null)
		StopCoroutine(cor);
		cor = StartCoroutine(behaviour());
		
	}
	void OnDisable()
	{
		canAttack = false;
		eneC.canDie = false;
		if(cor!=null)
		StopCoroutine(cor);
		active = false;
	}
	/*void OnTriggerStay2D(Collider2D other)
	{
		print(other.name);
	}*/
	IEnumerator behaviour()
	{
		bool inside = true;
		if(startOut)
		{
			inside = false;
			eneC.canDie = true;
		}
		render.enabled = !inside;
		yield return new WaitForSeconds(1f);
		while(active)
		{
		Vector3 target = Vector3.zero;
		if(inside)
		{
			render.enabled = inside;
			if(!ignorePlayer)
			{
				CheckForPlayer();
				yield return 0;
				while(playerNear)
				{
					CheckForPlayer();
					yield return 0;
				}
			}
			target = new Vector3(transform.position.x+distance.x,transform.position.y+distance.y,transform.position.z);
		}
		else target = new Vector3(transform.position.x-distance.x,transform.position.y-distance.y,transform.position.z);

			while(transform.position!=target)
			{
				if((!inside && col.enabled)||(inside && !col.enabled))
				{
					if((!inside && Vector3.Distance(transform.position,target)<=1.4f) ||
						(inside && Vector3.Distance(transform.position,target)<=1.0f))
						col.enabled = !col.enabled;
				}
				transform.position = Vector3.MoveTowards(transform.position,target,speed*Time.deltaTime);
				yield return 0;
			}
		
		inside = !inside;
		if(!inside) canAttack = true;
		if(inside)
		{
			eneC.canDie = false;
			col.enabled = false;
		}
		else
		{
			eneC.canDie = true;
			col.enabled = true;
		}
		render.enabled = !inside;
		yield return new WaitForSeconds(1.5f);
		if(!eneC.canDie)eneC.canDie=true;
		canAttack = false;

		}
	}
}
