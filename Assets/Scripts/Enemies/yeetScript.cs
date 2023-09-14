using System.Collections;
using UnityEngine;

public class yeetScript : MonoBehaviour {
	EnemyCorpseSpawner eneCorpse;
	Animator anim;
	Rigidbody2D rb;
	public Vector2 boxCastSize = Vector2.one;
	public Vector3 boxCastOffset;
	public LayerMask whatIsPlayer;
	public LayerMask whatIsBox;
	public LayerMask whatIsGround;
	//todo set this to false and set true when player in range.
	bool playerBehaviour = false;
	public bool debug = true;
	public float distance = 1f;
	bool holdingObject = false;
	MovementAI ai;
	float speed = 3;
	Transform blockParent;
	bool castCoolDown = false;
	public Vector2 strength = new Vector2(27,15);
	float throwStrength = 1;
	float throwHeight;
	Transform player;
	EnemyOffScreenDisabler eneOff;
	AnimatorSoundPlayer sound;
	// Use this for initialization
	void Start ()
	{
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();
		ai = GetComponent<MovementAI>();
		player = GameObject.Find("Player_main").transform;
		sound = GetComponent<AnimatorSoundPlayer>();
		blockParent = transform.GetChild(0).GetChild(0);
		eneCorpse = GetComponent<EnemyCorpseSpawner>();
		eneOff = GetComponent<EnemyOffScreenDisabler>();
	}
	// Update is called once per frame
	void Update ()
	{
		if(eneOff.visible)
		{
		blockParent.localScale = new Vector3(-transform.localScale.x,blockParent.localScale.y,blockParent.localScale.z);
		if(anim.speed!=1)
		anim.speed = 1;
		anim.SetFloat("horSpeed",Mathf.Abs(rb.velocity.x));
		//see if player is in range
		if(!castCoolDown)
		{
			raycastWall();
			if(playerBehaviour&&!holdingObject)
			tryToFindBlock();
		}

		deathBlockDrop();
		}
		else
		{
			if(anim.speed!=0)
			anim.speed = 0;
		}
	}
	void raycastWall()
	{
		//print("raycastWall");
		RaycastHit2D ray,ray1p,ray2p;

		if(player.position.x>transform.position.x)
		{
			ray = Physics2D.Raycast(new Vector3(transform.position.x,transform.position.y+transform.up.y,transform.position.z),
			transform.right,distance,whatIsGround);

			if(ray.collider!=null)
			{
				ray1p = Physics2D.Raycast(new Vector3(transform.position.x,transform.position.y,transform.position.z),
				transform.right,distance,whatIsGround);
				ray2p = Physics2D.Raycast(new Vector3(transform.position.x,transform.position.y,transform.position.z),
				-transform.right,distance,whatIsGround);

				if(ray1p.collider!=null&&ray1p.collider.transform.name=="PlayerCollider"||
				ray2p.collider!=null&&ray2p.collider.transform.name=="PlayerCollider"||
				player.position.x<ray.point.x) playerBoxCast();
				if(debug)
				{
					Debug.DrawLine(new Vector3(transform.position.x,transform.position.y+transform.up.y,transform.position.z),ray.point,Color.green,0.1f);
					if(ray1p.collider!=null)
					Debug.DrawLine(new Vector3(transform.position.x,transform.position.y,transform.position.z),ray1p.point,Color.blue);
					if(ray2p.collider!=null)
					Debug.DrawLine(new Vector3(transform.position.x,transform.position.y,transform.position.z),ray2p.point,Color.blue);
				}
			}
			else playerBoxCast();
		}

		else if(player.position.x<transform.position.x)
		{
			ray = Physics2D.Raycast(new Vector3(transform.position.x,transform.position.y+transform.up.y,transform.position.z),
			-transform.right,distance,whatIsGround);

			if(ray.collider!=null)
			{
				ray1p = Physics2D.Raycast(new Vector3(transform.position.x,transform.position.y,transform.position.z),
				transform.right,distance,whatIsGround);
				ray2p = Physics2D.Raycast(new Vector3(transform.position.x,transform.position.y,transform.position.z),
				-transform.right,distance,whatIsGround);

				if(ray1p.collider!=null&&ray1p.collider.transform.name=="PlayerCollider"||
				ray2p.collider!=null&&ray2p.collider.transform.name=="PlayerCollider"||
				player.position.x>ray.point.x) playerBoxCast();
				if(debug)
				{
					Debug.DrawLine(new Vector3(transform.position.x,transform.position.y+transform.up.y,transform.position.z),ray.point,Color.green,0.1f);
					if(ray1p.collider!=null)
					Debug.DrawLine(new Vector3(transform.position.x,transform.position.y,transform.position.z),ray1p.point,Color.blue);
					if(ray2p.collider!=null)
					Debug.DrawLine(new Vector3(transform.position.x,transform.position.y,transform.position.z),ray2p.point,Color.blue);
				}
			}
			else playerBoxCast();
		}
	}
	void playerBoxCast()
	{
		//print("boxcast");
		Collider2D playerCollider =
		Physics2D.OverlapBox(transform.position
		+new Vector3(boxCastOffset.x*transform.localScale.x,boxCastOffset.y*transform.localScale.y,0),boxCastSize,0,whatIsPlayer);
		
		if(playerCollider!=null&&playerCollider.name.ToLower().Contains("playercollider"))
		{
			if(!playerBehaviour)
			{
				playerBehaviour = true;
			}
			else if(playerBehaviour&&blockParent.childCount!=0&&holdingObject&&!castCoolDown)
			{
				castCoolDown = true;
				//Debug.Log("Throw, Distance: ("+(Mathf.Clamp(Mathf.Abs(transform.position.x-playerCollider.transform.position.x),0,9))+", "+(Mathf.Clamp(transform.position.y-playerCollider.transform.position.y-0.5f,0,5))+")");
				throwStrength = Mathf.Clamp(Mathf.Abs(transform.position.x-playerCollider.transform.position.x),0,9)/distance;
				throwHeight = Mathf.Clamp(playerCollider.transform.position.y-transform.position.y-0.5f,0,1.5f);
				if(transform.localScale.y==-1)
				throwHeight = -throwHeight;
				//Debug.Log(throwHeight);
				//mid throw
				anim.SetTrigger("throw");
			}
		}
		else
		{
			if(playerBehaviour)playerBehaviour = false;
		}
	}
	void tryToFindBlock()
	{
		if(debug)
		{
			print("trying to find block");
		}
		RaycastHit2D ray = Physics2D.Raycast(transform.position,transform.right,1.2f,whatIsBox);
		RaycastHit2D ray1 = Physics2D.Raycast(transform.position,-transform.right,1.2f,whatIsBox);
		if(debug)
		{
			if(ray.collider!=null)print(ray.collider.tag);
			if(ray1.collider!=null)print(ray1.collider.tag);
		}
		if(ray.collider!=null&&ray.collider.tag =="blockHoldable"||
		ray1.collider!=null&&ray1.collider.tag =="blockHoldable")
		{
			//Debug.Log("Found block");
			//Debug.DrawLine(new Vector3(transform.position.x+(0.5f*transform.localScale.x),transform.position.y,transform.position.z),ray.collider.transform.position,Color.blue,1f);

			//object facing right
			Rigidbody2D rb;
			if(ray.collider!=null)rb = ray.collider.transform.parent.GetComponent<Rigidbody2D>();
			else  rb = ray1.collider.transform.parent.GetComponent<Rigidbody2D>();
			if(rb==null||Mathf.Abs(rb.velocity.x)<=0.5f)
			if(transform.localScale.x==-1)
			{
				if(ray.collider!=null&&ray.collider.transform.position.x>transform.position.x||ray1.collider!=null&&ray1.collider.transform.position.x>transform.position.x)
				transform.localScale=new Vector3(1,1,1);

				Transform tr;
				if(ray.collider!=null)tr = ray.collider.transform;
				else tr = ray1.collider.transform;
				pickUpBlock(tr);
			}
			else
			{
				if(ray.collider!=null&&ray.collider.transform.position.x<=transform.position.x||ray1.collider!=null&&ray1.collider.transform.position.x<=transform.position.x)
				transform.localScale=new Vector3(1,1,1);

				Transform tr;
				if(ray.collider!=null)tr = ray.collider.transform;
				else tr = ray1.collider.transform;
				pickUpBlock(tr);
			}
		}
	}
	void deathBlockDrop()
	{
		if(eneCorpse.stompFlag)
		{
			eneCorpse.createCorpseFlipped = true;
			eneCorpse.spawnCorpse();
		}
	}
	void pickUpBlock(Transform other)
	{
		holdingObject = true;
		//stops to pick up block
		//Debug.Log(other.parent);
		Transform block = other.parent;
		holdableBlockScript blockScript = block.GetComponent<holdableBlockScript>();
		block.transform.GetChild(0).tag = "Untagged";
		block.transform.tag = "Untagged";
		rb.velocity = new Vector2(0,rb.velocity.y);
		ai.speed = 0;
		anim.SetTrigger("pickup");
		if(block.position.x<transform.position.x)
		{
			ai.changeDirection(-1);
		}
		else ai.changeDirection(1);
		
		blockScript.unparentAll();
		block.SetParent(blockParent);
		block.localPosition = Vector3.zero;
		block.GetComponent<holdableBlockScript>().blockParent(true,Vector3.zero,Vector2.zero,false);
		eneCorpse.objectsToUnParentOnDeath = new GameObject[blockParent.childCount];
		for(int i = 0;i<blockParent.childCount;i++)
		{
			eneCorpse.objectsToUnParentOnDeath[i]=blockParent.GetChild(i).gameObject;
		}

		StartCoroutine(coolDown());
	}
	public void throwBlock()
	{
		turnToPlayer();
		Transform objec = blockParent.GetChild(0);
		holdableBlockScript block = objec.GetComponent<holdableBlockScript>();
		block.yeetThrow = true;
		if(objec.GetComponent<Gravity>().maxVelocities.y>0)
		objec.GetComponent<Gravity>().maxVelocities = new Vector2(100,30);
		else objec.GetComponent<Gravity>().maxVelocities = new Vector2(100,-30);
		block.dropLogic = false;
		if(block.explosive)
		{
			block.canExplode = true;
		}
		else block.canShatter = true;
		block.isHazard = true;
		block.transform.tag = "Harm";
		block.transform.GetChild(0).tag = "Harm";
		sound.playSound(1);
		if(objec!=null)
		{
			if(transform.localScale.y==1)
			{	
				if(throwStrength<1)
				block.blockParent(false,Vector3.zero,new Vector2(transform.localScale.x*(strength.x+((strength.x*3f)*throwStrength)),15+(3*throwHeight)),false);
				else block.blockParent(false,Vector3.zero,new Vector2(transform.localScale.x*(strength.x*4.5f),15+(3*throwHeight)),false);
			}
			else
			{
				if(throwStrength<1)
				block.blockParent(false,Vector3.zero,new Vector2(transform.localScale.x*(strength.x+((strength.x*2.7f)*throwStrength)),-(15+(3*throwHeight))),false);
				else block.blockParent(false,Vector3.zero,new Vector2(transform.localScale.x*(strength.x*4.5f),-(15+(3*throwHeight))),false);
			}
		}
		StartCoroutine(returnToNormal());
		holdingObject = false;
	}
	IEnumerator coolDown()
	{
		castCoolDown = true;
		yield return new WaitForSeconds(0.8f);
		castCoolDown = false;
	}
	public void turnToPlayer()
	{
		if(player.position.x>transform.position.x)
		ai.changeDirection(1);
		else
		ai.changeDirection(-1);
		blockParent.localScale = new Vector3(-transform.localScale.x,blockParent.localScale.y,blockParent.localScale.z);
	}
	IEnumerator returnToNormal()
	{
		castCoolDown = true;
		yield return new WaitForSeconds(1f);
		ai.speed = speed;
		castCoolDown = false;
	}

	#if UNITY_EDITOR
	void OnDrawGizmos()
    {
		if(debug)
		{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.position+new Vector3(boxCastOffset.x*transform.localScale.x,boxCastOffset.y*transform.localScale.y,0),boxCastSize);
		}
	}
	#endif
}
