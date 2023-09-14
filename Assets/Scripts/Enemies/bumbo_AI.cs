using System.Collections;
using UnityEngine;

public class bumbo_AI : MonoBehaviour {
	Rigidbody2D rb;
	Transform player;
	public float SpeedAdditive = 1;
	public float maxSpeed = 0;
	float currentSpeed = 0;
	int dir = -1;
	public LayerMask whatIsGround;
	public bool inverted = false;
	EnemyOffScreenDisabler eneOff;
	checkForSemiSolid checker;
	CompositeCollider2D semiSolid;
	public bool ignoreSemiSolid = false;
	bool grounded = false;
	bool touchingGround = false;
	bool canJump = true;
	bool canMove = false;
	bool followPlayer = true;
	int coinCounter = 0;
	GameData data;
	Animator anim;
	ParticleSystem dust;
	bool inLava = false;
	// Use this for initialization
	void Start ()
	{
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		player = GameObject.Find("Player_main").transform;
		data = GameObject.Find("_GM").GetComponent<GameData>();
		eneOff = GetComponent<EnemyOffScreenDisabler>();
		checker = transform.GetChild(1).GetComponent<checkForSemiSolid>();
		semiSolid = GameObject.Find("SemiSolidMap").GetComponent<CompositeCollider2D>();
		dust = transform.GetChild(3).GetComponent<ParticleSystem>();
		data.playSound(63,transform.position);

		Jump(1000);
	}
	IEnumerator jumpCoolDown()
	{
		canJump = false;
		yield return new WaitUntil(()=>grounded);
		yield return new WaitForSeconds(0.05f);
		yield return 0;
		canJump = true;
		if(!canMove)
		{
			canMove = true;
			GetComponent<EnemyCorpseSpawner>().blockHitKills=true;
		}
	}
	IEnumerator dieInLava()
	{
		inLava = true;
		yield return new WaitForSeconds(4f);
		Destroy(gameObject);
	}
	// Update is called once per frame
	void Update ()
	{
		if(Time.timeScale!=0&&eneOff.visible&&!inLava)
		{
			wallDetect();
			groundDetect();
			Movement();
			if(grounded&&canJump)
			edgeDetect();

			//jump if player jumps
			//if(grounded&&SuperInput.GetKeyDown("Jump"))
			//Jump(1800);
			anim.SetFloat("HVelocity",Mathf.Abs(rb.velocity.x));
			anim.SetFloat("YVelocity",rb.velocity.y);

			if(checker!=null)
			{
				//Debug.Log(rb.velocity.y+" + Ignore Semi Solid: "+ignoreSemiSolid+" Checker Inside: "+checker.insideSemiSolid);
				if(!inverted)
				{
					//print(transform.name+": "+rb.velocity.y);
					if(rb.velocity.y>0&&!ignoreSemiSolid&&!grounded
					||rb.velocity.y<0&&!ignoreSemiSolid&&!checker.insideSemiSolid&&!grounded)
					{
						ignoreSemiSolid = true;
						Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),true);
						whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
						//Debug.Log(gameObject.name+" Ignoring collision "+"Checker inside: "+checker.insideSemiSolid+" RbY: "+rb.velocity.y);
					}
					else if(rb.velocity.y<=0&&ignoreSemiSolid&&checker.insideSemiSolid
					||rb.velocity.y<=0&&!transform.GetChild(0).gameObject.activeInHierarchy)
					{
					ignoreSemiSolid = false;
					Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
					whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
					//Debug.Log(gameObject.name+" Not ignoring collision");
					}
				}
				else
				{
					if(rb.velocity.y<0&&!ignoreSemiSolid&&!grounded
					||rb.velocity.y>0&&!ignoreSemiSolid&&!checker.insideSemiSolid&&!grounded)
					{
						ignoreSemiSolid = true;
						Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),true);
						whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
						//Debug.Log(gameObject.name+" Ignoring collision");
					}
					else if(rb.velocity.y>=0&&ignoreSemiSolid&&checker.insideSemiSolid
					||rb.velocity.y>=0&&!transform.GetChild(0).gameObject.activeInHierarchy)
					{
						ignoreSemiSolid = false;
						Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
						whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
						//Debug.Log(gameObject.name+" Not ignoring collision");
					}
				}
			}
		}
		if(Time.timeScale!=0&&!eneOff.visible&&rb.velocity.x!=0)
		{
			if(rb.velocity.x<0)
			rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x+(SpeedAdditive*dir)*Time.deltaTime,-maxSpeed,0),rb.velocity.y);
			else rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x+(SpeedAdditive*dir)*Time.deltaTime,0,maxSpeed),rb.velocity.y);
		}
		if(Mathf.Abs(rb.velocity.x)>1&&grounded&&anim.GetBool("Slide")&&!dust.isPlaying)
		{
			//dust.transform.eulerAngles = transform.eulerAngles;
			if(!inverted)
			{
				dust.transform.localPosition=new Vector3(0.25f,0,0);
				dust.transform.localScale = transform.localScale;
			}
			else
			{
				dust.transform.localPosition=new Vector3(-0.25f,0,0);
				dust.transform.localScale = new Vector3(-transform.localScale.x,transform.localScale.y,transform.localScale.z);
			}
			dust.Play();
		}
		else if(!anim.GetBool("Slide")&&dust.isPlaying
		||!grounded
		||Mathf.Abs(rb.velocity.x)<=1)
		{
			dust.Stop(false,ParticleSystemStopBehavior.StopEmitting);
		}
	}
	void Jump(float force)
	{
		if(canJump)
		{
		anim.SetBool("Slide",false);
		grounded = false;
		data.playSound(64,transform.position);
		if(!ignoreSemiSolid)
		{
			ignoreSemiSolid = true;
			Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),true);
			whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
			//Debug.Log(gameObject.name+" Ignoring collision");
		}
		rb.velocity = new Vector2(rb.velocity.x,0);
		Vector2 dir;
		dir = transform.up * force;
		rb.AddForce(new Vector2(0,dir.y),ForceMode2D.Force);
		StartCoroutine(jumpCoolDown());
		}
	}
	void wallDetect()
	{
		float inv = (inverted?-1:1);
		Vector3 point = transform.position;
		RaycastHit2D ray1,rayShortWall;
			ray1 = Physics2D.Raycast(point,transform.right*transform.localScale.x,0.5f*inv,whatIsGround);
			if(transform.localScale.x==1&&player.position.x>transform.position.x||transform.localScale.x==-1&&player.position.x<transform.position.x)
			{
				rayShortWall = Physics2D.Raycast(point,transform.right*transform.localScale.x*inv,2.5f,whatIsGround);

				if(rayShortWall.collider!=null&&grounded&&canJump)
				{
					Collider2D checkWallHeight = Physics2D.OverlapPoint(rayShortWall.point+new Vector2((transform.right.x*transform.localScale.x)/2,transform.up.y*2),whatIsGround);
					Debug.DrawLine(transform.position,rayShortWall.point+new Vector2((transform.right.x*transform.localScale.x)/2,transform.up.y*2),Color.red,1f);
					if(checkWallHeight==null)
					{
						Debug.DrawLine(transform.position,rayShortWall.point+new Vector2((transform.right.x*transform.localScale.x)/2,transform.up.y*2),Color.cyan,1f);
						Jump(1400);
						if(Mathf.Abs(rb.velocity.x)<6)
						{
							if(rb.velocity.x>=0)
							rb.velocity = new Vector3(6,rb.velocity.y);
							else rb.velocity = new Vector3(-6,rb.velocity.y);
						}
					}
				}
			}

		Debug.DrawLine(point,new Vector3(point.x+(1f*transform.localScale.x),point.y,point.z),Color.blue);
		if(ray1.collider!=null)
		{
			rb.velocity=new Vector2(0,rb.velocity.y);
		}
	}
	void groundDetect()
	{
		Vector3 StartPoint1 = new Vector3(transform.position.x-0.21f,transform.position.y,transform.position.z);
		Vector3 StartPoint2 = new Vector3(transform.position.x+0.21f,transform.position.y,transform.position.z);
		RaycastHit2D ray1 = Physics2D.Raycast(StartPoint1,-transform.up,0.63f,whatIsGround);
		RaycastHit2D ray2 = Physics2D.Raycast(StartPoint2,-transform.up,0.63f,whatIsGround);
		if(ray1.collider != null && !grounded && rb.velocity.y <= 0f && Time.timeScale!=0
		|| ray2.collider != null && !grounded && rb.velocity.y <= 0f && Time.timeScale!=0)
		{
			grounded = true;
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
		Vector3 point = new Vector3(transform.position.x+0.2f*transform.localScale.x,transform.position.y,transform.position.z);
		RaycastHit2D ray = Physics2D.Raycast(point,-transform.up,0.6f,whatIsGround);


		Debug.DrawLine(point,new Vector3(point.x,point.y-0.6f,point.z),Color.green);

		if(ray.collider==null&&grounded)
		{

			Collider2D checkPitDepth = Physics2D.OverlapPoint(new Vector2(transform.position.x,transform.position.y)+new Vector2((transform.right.x*transform.localScale.x)/2,(-transform.up.y*4)),whatIsGround);
			if(checkPitDepth==null)
			{
				Debug.DrawLine(transform.position,new Vector2(transform.position.x,transform.position.y)+new Vector2((transform.right.x*transform.localScale.x)/2,(-transform.up.y*4)),Color.cyan,1f);
				edgeJump();
			}
		}
	}
	void edgeJump()
	{
		Jump(1500);
		if(Mathf.Abs(rb.velocity.x)<6)
		{
			if(rb.velocity.x>=0)
			rb.velocity = new Vector3(6,rb.velocity.y);
			else rb.velocity = new Vector3(-6,rb.velocity.y);
		}
	}
	IEnumerator playerImpact()
	{
		Transform tr = transform.GetChild(2);
		tr.SetParent(player);
		tr.localPosition = Vector3.zero;
		if(dust.isPlaying)
		dust.Stop(false,ParticleSystemStopBehavior.StopEmitting);
		int coinsPre = data.coins;
		yield return 0;
		yield return new WaitUntil(()=>Time.timeScale!=0);


		data.addCoin(-20,false);
		int coinsPost = data.coins;
		int coinsStolen = coinsPre-coinsPost;
		coinCounter+= coinsStolen;
		//print(coinsStolen);
		ParticleSystem part = tr.GetComponent<ParticleSystem>();
		tr.GetComponent<ParticleTarget>().Target = transform;
		StartCoroutine(playCoinSounds(coinsStolen));
		var main = part.main;
        main.maxParticles = coinsStolen;
		tr.gameObject.SetActive(true);
		if(part.isPlaying)
		part.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);

		part.Play();

		followPlayer = false;
		canMove = false;
		eneOff.neverSleep = true;
		yield return new WaitForSeconds(0.6f);
		UnityEngine.Rendering.SortingGroup gr = GetComponent<UnityEngine.Rendering.SortingGroup>();
		gr.sortingLayerName = "Player";
		gr.sortingOrder = 10;
		Jump(1000);
		yield return 0;
		GetComponent<Collider2D>().enabled = false;
		yield return new WaitForSeconds(5f);
		gameObject.SetActive(false);
	}
	IEnumerator playCoinSounds(int l)
	{
		int amount = Mathf.Clamp(l/2,1,10);
		for(int i = 0; i<amount;i++)
		{
			data.playSound(62,transform.position);
			yield return new WaitForSeconds(0.05f);
		}
		yield return new WaitForSeconds(0.7f);
		if(amount>0)
		{
			amount = Mathf.Clamp(amount/3,1,3);
			//print(amount);
			for(int i = 0; i<amount;i++)
			{
				data.playSound(61,transform.position);
				//print("sound");
				yield return new WaitForSeconds(0.05f);
			}
		}
	}
	void Movement()
	{
			if(grounded&&canMove||!grounded&& transform.localScale.x==-1 && rb.velocity.x>-6&&canMove||!grounded&& transform.localScale.x==1 && rb.velocity.x<6&&canMove)
			if(Mathf.Abs(currentSpeed)<maxSpeed)
			{
				rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x+(SpeedAdditive*dir)*Time.deltaTime,-maxSpeed,maxSpeed),rb.velocity.y);
			}
			if(player.position.x>transform.position.x&&followPlayer)
			{
				if(rb.velocity.x>=-0.1f&&transform.localScale.x!=1)
				{
					transform.localScale = Vector3.one;
					anim.SetBool("Slide",false);
				}

				if(dir!=1)
				dir = 1;

				if(rb.velocity.x<-0.1f&&!anim.GetBool("Slide")&&grounded&&transform.localScale.x!=1)
				{
					anim.SetBool("Slide",true);
				}
			}
			else if(player.position.x<transform.position.x&&followPlayer)
			{
				if(rb.velocity.x<=0.1f&&transform.localScale.x!=-1)
				{
					transform.localScale = new Vector3(-1,1,1);
					anim.SetBool("Slide",false);
				}

				if(dir!=-1)
				dir = -1;

				if(rb.velocity.x>0.1f&&!anim.GetBool("Slide")&&grounded&&transform.localScale.x!=-1)
				{
					anim.SetBool("Slide",true);
				}
			}
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
		if(other.name == "Coin" && !other.gameObject.GetComponent<coinScript>().collected)
			{
				Animator an = other.transform.GetChild(0).GetComponent<Animator>();
				if(an.enabled)
				an.SetTrigger("collected");
				other.gameObject.GetComponent<coinScript>().killCoin();
				data.playSound(61,transform.position);
				coinCounter++;
			}
		if(other.name=="InstantDeath"&&!inLava)
		{
			if(GetComponent<Gravity>()!=null)
			{
			rb.velocity = Vector2.zero;
			Gravity grav = GetComponent<Gravity>();
			grav.pushForces = new Vector2(grav.pushForces.x,grav.pushForces.y/2);
			grav.maxVelocities = new Vector2(grav.maxVelocities.x,0.4f);
			}
			maxSpeed = 0;
			data.spawnCheeseSplatterPoint(transform.position);
			StartCoroutine(dieInLava());
			transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = other.GetComponent<Renderer>().sortingLayerName;
			transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = other.GetComponent<Renderer>().sortingOrder-1;
		}
		if(other.name=="deathZone")
		{
			Destroy(gameObject);
		}
	}
	public void playerSteal()
	{
		StartCoroutine(playerImpact());
	}
	void OnDisable()
	{
		if(Application.isPlaying)
		{
		if(!followPlayer)
		Destroy(gameObject);
		if(ignoreSemiSolid)
		{
			ignoreSemiSolid = false;
			Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
			whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
			//Debug.Log(gameObject.name+" Not ignoring collision");
		}
		anim.SetBool("Slide",false);
		}
	}
}
