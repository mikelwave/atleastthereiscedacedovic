using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fugo_AI : MonoBehaviour
{
	public LayerMask whatIsGround,whatIsSolidGround;
	bool Edge = false,Wall = false;
	Transform player;
	EnemyOffScreenDisabler eneOff;
	EnemyCorpseSpawner enemyCorpse;
	checkForSemiSolid checker;
	CompositeCollider2D semiSolid;
	public bool ignoreSemiSolid = false;
	public Vector3 startPosition;
	Rigidbody2D rb;
	Animator anim;
	public float outOfBoundsDistance = 12;
	public float backAwayDistance = 5;
	int behave = 0; //behaviour int 0 - idle, 1 - pursue, 2 - back away
	bool canSwitchBehaviour = true;
	Coroutine cooldown;
	Coroutine shoot;
	public float horizontalSpeed = 3f;
	bool grounded = false;
	bool shooting = false;
	bool touchingGround = false;
	public GameObject projectile;
	List<GameObject> projectiles;
	public int pooledProjectiles = 5;
	public int fireDelay;
	GameData data;
	bool visible = false;
	bool dying = false;
	public AudioClip parryDeath;
	float subGroundDetect = 0;
	
	// Use this for initialization
	void Start ()
	{
		if(player==null)
		{
		player = GameObject.Find("Player_main").transform;
		eneOff = GetComponent<EnemyOffScreenDisabler>();
		enemyCorpse = GetComponent<EnemyCorpseSpawner>();
		startPosition = transform.position;
		data = GameObject.Find("_GM").GetComponent<GameData>();
		rb = GetComponent<Rigidbody2D>();
		if(transform.childCount>1)
		checker = transform.GetChild(1).GetComponent<checkForSemiSolid>();
		semiSolid = GameObject.Find("SemiSolidMap").GetComponent<CompositeCollider2D>();
		anim = GetComponent<Animator>();
		projectiles = new List<GameObject>();
		for(int i = 0; i< pooledProjectiles;i++)
		{
			GameObject obj = Instantiate(projectile,transform.position,Quaternion.identity);
			obj.SetActive(false);
			obj.transform.SetParent(transform);
			obj.GetComponent<Jumper>().data = data;
			obj.GetComponent<Jumper>().setParent = transform;
			projectiles.Add(obj);
		}
		//Debug.Log("Start");
		if(transform.eulerAngles.z>=150)subGroundDetect = 0.24f;
		GetComponent<Crusher>().assignValues(whatIsGround,GetComponent<EnemyCorpseSpawner>(),data);
		}
	}
	IEnumerator backOff()
	{
		while(behave==2)
		{
			canSwitchBehaviour = false;
			yield return new WaitForSeconds(1f);
			canSwitchBehaviour = true;
			yield return 0;
		}
	}
	IEnumerator shootCor()
	{
		//Debug.Log("coroutine");
		//Debug.Log("visible");
		yield return new WaitForSeconds(0.5f);
		while(visible&&!dying)
		{
			//Debug.Log("shooting");
			Fire();
			yield return new WaitForSeconds(fireDelay);
		}
	}
	IEnumerator dieInLava()
	{
		dying = true;
		yield return new WaitForSeconds(4f);
		Destroy(gameObject);
	}
	void Fire()
	{
		for(int i = 0; i< projectiles.Count; i++)
			if(!projectiles[i].activeInHierarchy)
			{
				anim.SetTrigger("shoot");
				data.playSound(7,transform.position);
				Gravity grav = projectiles[i].GetComponent<Gravity>();
				if(Mathf.Round(transform.eulerAngles.z)==0)
				{
					grav.pushForces = new Vector2(grav.pushForces.x,-Mathf.Abs(grav.pushForces.y));
					projectiles[i].transform.position = new Vector3(transform.position.x,transform.position.y+0.5f,transform.position.z);
				}
				else
				{
					projectiles[i].transform.eulerAngles = new Vector3(0,0,180);
					grav.pushForces = new Vector2(grav.pushForces.x,Mathf.Abs(grav.pushForces.y));
					projectiles[i].transform.position = new Vector3(transform.position.x,transform.position.y-0.5f,transform.position.z);
				}
				projectiles[i].transform.parent = null;
				projectiles[i].transform.localScale = transform.localScale;
				projectiles[i].SetActive(true);
				break;
			}
	}
	void OnDisable()
	{
		if(shoot!=null)
		StopCoroutine(shoot);
	}
	void OnEnable()
	{
		if(shoot!=null)
		StopCoroutine(shoot);
		if(player!=null&&!dying)
		shoot = StartCoroutine(shootCor());
	}
	// Update is called once per frame
	void Update ()
	{
		if(eneOff.visible)
		{
			edgeDetect();
			wallDetect();
			groundDetect();
			facePlayer();
			if(transform.localScale.x==-1)
			{
				if(transform.position.x>player.position.x+outOfBoundsDistance&&behave!=0)
				{
					behave = 0;
					//Debug.Log("Player too far");
				}
				else if(transform.position.x<=player.position.x+outOfBoundsDistance&&behave!=1&&canSwitchBehaviour
				&&transform.position.x>=player.position.x+backAwayDistance)
				{
					behave = 1;
					//Debug.Log("Player in bounds");
				}
				else if(!Wall&&transform.position.x<=player.position.x+backAwayDistance&&behave!=2&&canSwitchBehaviour)
				{
					behave = 2;
					canSwitchBehaviour = false;
					if(cooldown!=null)
					StopCoroutine(cooldown);
					cooldown = StartCoroutine(backOff());
					//Debug.Log("Player too close");
				}
			}
			else if(transform.localScale.x==1)
			{
				if(transform.position.x<player.position.x-outOfBoundsDistance&&behave!=0)
				{
					behave = 0;
					//Debug.Log("Player too far");
				}
				else if(transform.position.x>=player.position.x-outOfBoundsDistance&&behave!=1&&canSwitchBehaviour
				&&transform.position.x<=player.position.x-backAwayDistance)
				{
					behave = 1;
					//Debug.Log("Player in bounds");
				}
				else if(!Wall&&transform.position.x>=player.position.x-backAwayDistance&&behave!=2&&canSwitchBehaviour)
				{
					behave = 2;
					canSwitchBehaviour = false;
					if(cooldown!=null)
					StopCoroutine(cooldown);
					cooldown = StartCoroutine(backOff());
					//Debug.Log("Player too close");
				}
			}
			if(behave==1)
			{
				if(!Edge)
				{
				rb.velocity=new Vector2(transform.localScale.x*horizontalSpeed,rb.velocity.y);
				}
				else	rb.velocity=new Vector2(0,rb.velocity.y);
			}
			if(behave==2)
			{
				if(!Wall)
				{
				rb.velocity=new Vector2(transform.localScale.x*horizontalSpeed*-1,rb.velocity.y);
				}
				else	rb.velocity=new Vector2(0,rb.velocity.y);
			}
			if(rb.velocity.x!=0&&grounded&&!shooting)
			anim.speed = Mathf.Abs(rb.velocity.x/2);
			else anim.speed = 1;
			anim.SetFloat("HorSpeed",Mathf.Abs(rb.velocity.x));
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
	void edgeDetect()
	{
		Vector3 point = new Vector3(transform.position.x+0.2f*transform.localScale.x,transform.position.y,transform.position.z);
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
		Vector3 point = transform.position+(transform.up*0.5f);
		RaycastHit2D ray;
		if(subGroundDetect==0)ray = Physics2D.Raycast(point,-transform.right*transform.localScale.x,0.6f,whatIsSolidGround);
		else ray = Physics2D.Raycast(point,transform.right*transform.localScale.x,0.6f,whatIsSolidGround);
		if(ray.collider!=null)
		Debug.DrawLine(point,ray.point,Color.yellow);
		//Debug.DrawLine(point,point-(transform.right*0.6f*transform.localScale.x),Color.cyan);

		if(ray.collider!=null&&!Wall)
		{
			Wall = true;
			if(cooldown!=null)
				StopCoroutine(cooldown);
			anim.SetFloat("HorSpeed",0);
			//Debug.Log(gameObject.name+" Wall detected");
		}
		if(ray.collider==null&&Wall)
		{
			Wall = false;
		}
	}
	void facePlayer()
	{
		if(player.position.x<=transform.position.x&&transform.localScale.x!=-1)
		{
		transform.localScale = new Vector3(-1,1,1);
		Wall = false;
		}
		else if(player.position.x>transform.position.x&&transform.localScale.x!=1)
		{
		transform.localScale = Vector3.one;
		Wall = false;
		}
	}
	void groundDetect()
	{
		Vector3 StartPoint1 = new Vector3(transform.position.x-0.21f,transform.position.y+(0.12f-subGroundDetect),transform.position.z);
		Vector3 StartPoint2 = new Vector3(transform.position.x+0.21f,transform.position.y+(0.12f-subGroundDetect),transform.position.z);
		RaycastHit2D ray1 = Physics2D.Raycast(StartPoint1,-transform.up,0.2f,whatIsGround);
		RaycastHit2D ray2 = Physics2D.Raycast(StartPoint2,-transform.up,0.2f,whatIsGround);
		if(ray1.collider!=null)
		{
			Debug.DrawLine(transform.position,ray1.point,Color.red);
		}
		if(ray2.collider!=null)
		{
			Debug.DrawLine(transform.position,ray2.point,Color.blue);
		}
		if((ray1.collider!=null||ray2.collider!=null)&&!grounded&&Time.timeScale!=0&&((subGroundDetect==0&&rb.velocity.y<=0f)||(subGroundDetect!=0&&rb.velocity.y>=0f)))
		{
			grounded = true;
		}
		else if(
			(ray1.collider!=null||ray2.collider!=null) &&
			(
				(grounded && !touchingGround)||((subGroundDetect==0&& rb.velocity.y > 0.02f)||(subGroundDetect!=0&& rb.velocity.y < -0.02f))
			)
		)
		{
			grounded = false;
		}

		anim.SetBool("Grounded",grounded);
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
	public void compareFireball(GameObject obj)
	{
		for(int i = 0;i<projectiles.Count;i++)
		{
			if(obj == projectiles[i])
			{
				enemyCorpse.playDeathSound = false;
				data.playUnlistedSoundPoint(parryDeath,transform.position);
				break;
			}
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
			if(shoot!=null)
			StopCoroutine(shoot);
			shoot = StartCoroutine(shootCor());
		}
		if(other.name=="InstantDeath")
		{
			if(GetComponent<Gravity>()!=null)
			{
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
		if(other.name=="deathZone")
		{
			Destroy(gameObject);
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
