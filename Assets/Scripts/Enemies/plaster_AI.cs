using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class plaster_AI : MonoBehaviour
{	
	public int pooledprojectiles = 1;
	public LayerMask whatIsGround;
	public LayerMask whatIsSolidGround;
	LayerMask playerMask,projectileMask;
	int updateTimer = 60;
	bool visible = false;
	bool touchingGround = false;
	bool grounded = false;
	Transform player;
	EnemyOffScreenDisabler eneOff;
	EnemyCorpseSpawner eneCorpse;
	public Vector3 startPosition;
	public bool returnToStartPointOnDeathZone = true;
	GameData data;
	Rigidbody2D rb;
	checkForSemiSolid checker;
	CompositeCollider2D semiSolid;
	public bool ignoreSemiSolid = false;
	Animator anim;
	public GameObject projectile;
	List <GameObject> projectiles;
	bool dying = false;
	float horizontalSpeed = 0f;
	bool Edge = false;
	float jumpForce = 1400f;
	public float jumpForceReduct = 1f;
	public float releaseJumpVelocity = 6f;
	Vector3 targetPoint = Vector3.zero;
	bool reachedPoint = true;
	bool ignorePlayer = false;
	public bool stunned = false;
	public bool inverted = false;
	bool firing = false;
	public bool sendContactInfo = false;
	public float collisionDetectOffset = 0.05f,crusherMaxHeight = 0;
	BoxCollider2D col;
	int jumpCooldown = 0;
	// Use this for initialization
	void Start ()
	{
		if(player==null)
		{
			player = GameObject.Find("Player_main").transform;
			col = GetComponent<BoxCollider2D>();
			playerMask |= (1 << LayerMask.NameToLayer("Player"));
			playerMask |= (1 << LayerMask.NameToLayer("Ground"));
			projectileMask |= (1 << LayerMask.NameToLayer("Friendly_projectile"));
			eneOff = GetComponent<EnemyOffScreenDisabler>();
			eneCorpse = GetComponent<EnemyCorpseSpawner>();
			startPosition = transform.position;
			data = GameObject.Find("_GM").GetComponent<GameData>();
			rb = GetComponent<Rigidbody2D>();
			if(transform.childCount>1)
			checker = transform.GetChild(1).GetComponent<checkForSemiSolid>();
			semiSolid = GameObject.Find("SemiSolidMap").GetComponent<CompositeCollider2D>();
			anim = GetComponent<Animator>();
			projectiles = new List<GameObject>();
			if(projectile!=null)
			for(int i = 0; i<pooledprojectiles;i++)
			{
				GameObject obj = Instantiate(projectile,transform.position,Quaternion.identity);
				obj.SetActive(false);
				obj.transform.SetParent(transform);
				//obj.GetComponent<Jumper>().data = data;
				//obj.GetComponent<Jumper>().setParent = transform;
				projectiles.Add(obj);
			}
			//Debug.Log("Start");
		}
	}
	void crusher()
	{
		RaycastHit2D rayLeft = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5,0),-transform.right,0.5f,whatIsGround);
		RaycastHit2D rayRight = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5,0),transform.right,0.5f,whatIsGround);
		RaycastHit2D rayDown = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5,0),-transform.up,0.3f,whatIsGround);
		RaycastHit2D rayUp = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5+crusherMaxHeight,0),transform.up,0.5f,whatIsSolidGround);

		if(rayLeft.collider!=null&rayRight.collider!=null&&rayLeft.collider.transform!=rayRight.collider.transform
		||rayUp.collider!=null&rayDown.collider!=null&&rayUp.collider.transform!=rayDown.collider.transform)
		{
			if(sendContactInfo)
			{
				print(transform.name+" crushed");
				if(rayUp.collider!=null)
				print(gameObject.name+" "+rayUp.transform.name);
				if(rayDown.collider!=null)
				print(gameObject.name+" "+rayDown.transform.name);
				if(rayLeft.collider!=null)
				print(gameObject.name+" "+rayLeft.transform.name);
				if(rayRight.collider!=null)
				print(gameObject.name+" "+rayRight.transform.name);
			}
			if(eneCorpse!=null)
			{
				data.playSoundOverWrite(20,transform.position);
				eneCorpse.createCorpseFlipped = true;
				eneCorpse.createCorpse = false;
				eneCorpse.spawnCorpse();
			}
			else
			{
				data.playSoundOverWrite(20,transform.position);
				Destroy(gameObject);
			}
		}
	}
	// Update is called once per frame
	void Update ()
	{
		if(eneOff.visible&&Time.timeScale!=0)
		{
			crusher();
			if(!stunned)
			{
				if(eneCorpse.stompFlag)
				{
					anim.SetTrigger("Stomp");
					reachedPoint = true;
					updateTimer = 60;
					horizontalSpeed = 0;
					anim.SetFloat("Speed",horizontalSpeed);
					eneCorpse.stompFlag = false;
				}
				anim.SetFloat("Velocity",rb.velocity.y);
				edgeDetect();
				if(!reachedPoint)
				wallDetect();
				groundDetect();
				//make the enemy dodge player's projectiles
				if(grounded)
				{
					lookforProjectiles();
					if(jumpCooldown>0)
					jumpCooldown--;
				}
				//enemy stands when he reaches the goal point, for 60 frames, he scans for player, if he doesn't see him, he makes a lazy turn
				if(reachedPoint)
				{
					if(updateTimer==0)
					{
						if(ignorePlayer)
						ignorePlayer = false;
						//Debug.Log("move now");
						findLazyWalkPoint();
						updateTimer = 60;
					}
					if(!firing&&!stunned)
					{
						updateTimer--;
						if(!ignorePlayer)
						tryFire();
					}
				}
				else
				{
					if(!Edge)
					{
						rb.velocity=new Vector2(transform.localScale.x*horizontalSpeed,rb.velocity.y);
						if(transform.localScale.x==1)
						{
							if(transform.position.x>=targetPoint.x)
							{
								reachedPoint = true;
								horizontalSpeed = 0;
								rb.velocity=new Vector2(0,rb.velocity.y);
								anim.SetFloat("Speed",horizontalSpeed);
							}
						}
						else
						{
							if(transform.position.x<=targetPoint.x)
							{
								reachedPoint = true;
								horizontalSpeed = 0;
								rb.velocity=new Vector2(0,rb.velocity.y);
								anim.SetFloat("Speed",horizontalSpeed);
							}
						}
					}
					else
					{
						//if lazy turn walk, just stop
						if(horizontalSpeed<=1f)
						{
							reachedPoint = true;
							horizontalSpeed = 0;
							rb.velocity=new Vector2(0,rb.velocity.y);
							anim.SetFloat("Speed",horizontalSpeed);
						}
						//if running, do a raycast of the gapin front of the enemy and check if he will make it
						else
						{
							Jump(1);
						}
					}
				}

				if(checker!=null)
				{
					if(rb.velocity.y>0&&!ignoreSemiSolid||rb.velocity.y>0&&!ignoreSemiSolid&&!checker.insideSemiSolid)
					{
						ignoreSemiSolid = true;
						Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),true);
						whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
						////Debug.Log(gameObject.name+" Ignoring collision");
					}
				else if(rb.velocity.y<=0&&ignoreSemiSolid&&checker.insideSemiSolid||rb.velocity.y<=0&&!transform.GetChild(0).gameObject.activeInHierarchy)
					{
						ignoreSemiSolid = false;
						Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
						whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
						////Debug.Log(gameObject.name+" Not ignoring collision");
					}
				}
			}
		}
	}
	void findLazyWalkPoint()
	{
		//Debug.Log("pick new lazy point");
		reachedPoint = false;
		//move to the x point horizontally
		//lazy walk - find a random point behind the enemy
		targetPoint = new Vector3(transform.position.x+(Random.Range(2,4)*-1*transform.localScale.x),transform.position.y,transform.position.z);
		transform.localScale = new Vector3(-transform.localScale.x,transform.localScale.y,transform.localScale.z);
		horizontalSpeed = 1f;
		Debug.DrawLine(transform.position, targetPoint,Color.blue);
		anim.SetFloat("Speed",horizontalSpeed);
	}
	void lookforProjectiles()
	{
		float checklength = 2f;
		if(horizontalSpeed>3f) checklength = 2.5f;
		Vector3 StartPoint1 = new Vector3(transform.position.x,transform.position.y+transform.up.y*1.0f,transform.position.z);
		RaycastHit2D ray = Physics2D.BoxCast(StartPoint1,col.size,0,transform.right,checklength,projectileMask);
		//RaycastHit2D ray2 = Physics2D.BoxCast(StartPoint2,col.size,0,transform.right,checklength,projectileMask);
		RaycastHit2D ray3 = Physics2D.BoxCast(StartPoint1,col.size,0,-transform.right,checklength,projectileMask);
		//RaycastHit2D ray4 = Physics2D.BoxCast(StartPoint2,col.size,0,-transform.right,checklength,projectileMask);
		//dodge
		if(ray.collider!=null||ray3.collider!=null)
		{
			firing = false;
			anim.ResetTrigger("Shoot");
			updateTimer = 60;
			reachedPoint = true;
			rb.velocity = new Vector2(0,rb.velocity.y);
			horizontalSpeed = 0f;
			anim.SetFloat("Speed",horizontalSpeed);

			//Debug.Log("Projectile detected");
			Jump(0);
		}
	}
	void playerSightLine()
	{
		Vector3 StartPoint = new Vector3(transform.position.x,transform.position.y+transform.up.y*0.5f,transform.position.z);
		RaycastHit2D ray1 = Physics2D.Raycast(StartPoint,transform.right*transform.localScale.x,13f,playerMask);
		if(ray1.collider!=null&&ray1.collider.gameObject.name=="PlayerCollider")
		{
			//Debug.Log("chase");
			ignorePlayer = true;
			reachedPoint = false;
			updateTimer = 60;
			Debug.DrawLine(StartPoint,ray1.point,Color.red);
			targetPoint = new Vector3(ray1.point.x,ray1.point.y,transform.position.z);
			horizontalSpeed = 4f;
			anim.SetFloat("Speed",horizontalSpeed);
		}
	}
	public void playerPeek()
	{
		if(firing)
		{
			firing = false;
			anim.ResetTrigger("Shoot");
		}
		Vector3 StartPoint = new Vector3(transform.position.x-(0.8f*transform.localScale.x),transform.position.y+transform.up.y*0.5f,transform.position.z);
		Vector3 StartPoint2 = new Vector3(transform.position.x-(0.8f*transform.localScale.x),transform.position.y+transform.up.y*2f,transform.position.z);
		RaycastHit2D ray1 = Physics2D.Raycast(StartPoint,transform.right*transform.localScale.x,6f,playerMask);
		RaycastHit2D ray2 = Physics2D.Raycast(StartPoint2,transform.right*transform.localScale.x,6f,playerMask);

		bool ray1Positive = false; bool ray2Positive = false;
		if(ray1.collider!=null&&ray1.collider.gameObject.name=="PlayerCollider") ray1Positive = true;
		if(!ray1Positive&&ray2.collider!=null&&ray2.collider.gameObject.name=="PlayerCollider") ray2Positive = true;

		if(!ray1Positive&&!ray2Positive)
		{
			anim.SetBool("CanGetUp",true);
		}
		else
		{
			if(ray1.collider!=null)
			Debug.DrawLine(StartPoint,ray1.point,Color.red,1f);
			if(ray2.collider!=null)
			Debug.DrawLine(StartPoint2,ray2.point,Color.green,1f);
			anim.SetBool("CanGetUp",false);
		}
	}
	public void Fire()
	{
		if(!dying)
		{
			data.playSound(56,transform.position);

			for(int i = 0; i< projectiles.Count; i++)
			if(!projectiles[i].activeInHierarchy)
			{
				Vector3 pos = transform.GetChild(2).position;
				projectiles[i].transform.position = new Vector3(pos.x,pos.y,transform.position.z);
				projectiles[i].transform.parent = null;
				projectiles[i].transform.localScale = transform.localScale;
				projectiles[i].SetActive(true);
				break;
			}
		}
		firing = false;
	}
	void tryFire()
	{
		Vector3 StartPoint = new Vector3(transform.position.x,transform.position.y+transform.up.y*0.5f,transform.position.z);
		RaycastHit2D ray1 = Physics2D.Raycast(StartPoint,transform.right*transform.localScale.x,13f,playerMask);
		if(ray1.collider!=null&&ray1.collider.gameObject.name=="PlayerCollider")
		{
		for(int i = 0; i< projectiles.Count; i++)
		{
			if(!projectiles[i].activeInHierarchy&&projectiles[i].GetComponent<Jumper>().summonedObject==null)
			{
				//Debug.Log("shoot");
				firing = true;
				ignorePlayer = true;
				reachedPoint = true;
				updateTimer = 80;
				horizontalSpeed = 0;
				anim.SetFloat("Speed",horizontalSpeed);
				rb.velocity = new Vector2(0,rb.velocity.y);
				if(player.position.x<=transform.position.x&&transform.localScale.x!=-1)
					transform.localScale = new Vector3(-1,1,1);
				else if(player.position.x>transform.position.x&&transform.localScale.x!=1)
					transform.localScale = Vector3.one;

				anim.SetTrigger("Shoot");
				break;
			}
		}
		if(!firing) playerSightLine();
		}
		else
		{
			playerSightLine();
		}
	}
	void Jump(int type)
	{
		if(grounded&&jumpCooldown<=0)
		{
			jumpCooldown = 5;
			horizontalSpeed = 7f;
			Edge = false;
			grounded = false;
			if(!ignoreSemiSolid)
			{
				ignoreSemiSolid = true;
				whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
			}
			data.playSound(54,transform.position);
			rb.velocity = new Vector2(rb.velocity.x,0);
			//anim.SetTrigger("Jump");
			anim.SetBool("Grounded",grounded);
			Vector2 dir;
			if(type==0)
			{
			dir = transform.up * jumpForce/jumpForceReduct;
			rb.AddForce(new Vector2(0,dir.y),ForceMode2D.Force);
			}
			else
			{
				dir = transform.up * 1000/jumpForceReduct;
				rb.AddForce(new Vector2(100*transform.localScale.x,dir.y),ForceMode2D.Force);
			}
		}
	}
	void groundDetect()
	{
		Vector3 StartPoint1 = new Vector3(transform.position.x-0.21f,transform.position.y+0.12f,transform.position.z);
		Vector3 StartPoint2 = new Vector3(transform.position.x+0.21f,transform.position.y+0.12f,transform.position.z);
		RaycastHit2D ray1 = Physics2D.Raycast(StartPoint1,-transform.up,0.13f,whatIsGround);
		RaycastHit2D ray2 = Physics2D.Raycast(StartPoint2,-transform.up,0.13f,whatIsGround);
		if(ray1.collider != null && !grounded && rb.velocity.y <= 0f && Time.timeScale!=0
		|| ray2.collider != null && !grounded && rb.velocity.y <= 0f && Time.timeScale!=0)
		{
			grounded = true;
			if(horizontalSpeed>4f)
			horizontalSpeed = 4f;
		}
		else if(ray1.collider == null && grounded && !touchingGround
		     || ray1.collider == null && rb.velocity.y > 0.02f
		     || ray2.collider == null && grounded && !touchingGround
		     || ray2.collider == null && rb.velocity.y > 0.02f)
			 {
				 grounded = false;
			 }

		anim.SetBool("Grounded",grounded);
	}
	void edgeDetect()
	{
		Vector3 point = new Vector3(transform.position.x+0.2f*transform.localScale.x,transform.position.y+0.1f,transform.position.z);
		RaycastHit2D ray = Physics2D.Raycast(point,-transform.up,0.5f,whatIsGround);

		Debug.DrawLine(point,new Vector3(point.x,point.y-0.5f,point.z),Color.green);

		if(ray.collider==null&&!Edge)
		{
			Edge = true;
			//Debug.Log(gameObject.name+" Edge detected");
		}
		if(ray.collider!=null&&Edge)
		{
			Edge = false;
		}
	}
	void wallDetect()
	{
		Vector3 point = new Vector3(transform.position.x,transform.position.y+transform.up.y*0.4f,transform.position.z);
		Vector3 point2 = new Vector3(transform.position.x,transform.position.y+transform.up.y*1.4f,transform.position.z);
		RaycastHit2D ray1 = Physics2D.Raycast(point,transform.right*transform.localScale.x*(inverted?-1:1),0.5f,whatIsGround),
		ray2 = Physics2D.Raycast(point2,transform.right*transform.localScale.x*(inverted?-1:1),0.5f,whatIsGround);

		Debug.DrawLine(point,new Vector3(point.x+(1f*transform.localScale.x),point.y,point.z),Color.blue);
		Debug.DrawLine(point2,new Vector3(point2.x+(1f*transform.localScale.x),point2.y,point2.z),Color.green);
		if(ray1.collider!=null||ray2.collider!=null)
		{
			reachedPoint = true;
			horizontalSpeed = 0;
			updateTimer = 60;
			rb.velocity=new Vector2(0,rb.velocity.y);
			anim.SetFloat("Speed",horizontalSpeed);
		}
	}
	IEnumerator dieInLava()
	{
		dying = true;
		yield return new WaitForSeconds(4f);
		Destroy(gameObject);
	}
	void OnCollisionStay2D(Collision2D other)
	{
		if(other.gameObject.tag == "Spring"||other.gameObject.tag == "NPC"||other.gameObject.tag == "Ground"||other.gameObject.tag == "Harm"||other.gameObject.tag == "semiSolid"&&!ignoreSemiSolid&&checker.insideSemiSolid)
		{
			touchingGround = true;
		}
	}
	void OnCollisionExit2D(Collision2D other)
	{
		if(other.gameObject.tag == "Spring"||other.gameObject.tag == "NPC"||other.gameObject.tag == "Ground"||other.gameObject.tag == "Harm"||other.gameObject.tag == "semiSolid")
		{
			touchingGround = false;
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name=="ObjectActivator"&&!visible)
		{
			visible = true;
			//Debug.Log("trigger");
			if(player==null)
			Start();
		}
		if(other.name=="InstantDeath")
		{
			if(GetComponent<Gravity>()!=null)
			{
			horizontalSpeed = 0;
			anim.SetFloat("Speed",horizontalSpeed);
			rb.velocity = Vector2.zero;
			Gravity grav = GetComponent<Gravity>();
			grav.pushForces = new Vector2(grav.pushForces.x,grav.pushForces.y/2);
			grav.maxVelocities = new Vector2(grav.maxVelocities.x,0.4f);
			}
			data.spawnCheeseSplatterPoint(transform.position);
			StartCoroutine(dieInLava());
			transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = other.GetComponent<Renderer>().sortingLayerName;
			transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = other.GetComponent<Renderer>().sortingOrder-1;
			horizontalSpeed = 0;
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name=="ObjectActivator"&&visible)
		{
			visible = false;
		}
	}
}
