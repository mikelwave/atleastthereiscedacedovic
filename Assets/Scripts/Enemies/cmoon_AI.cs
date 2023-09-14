using System.Collections;
using UnityEngine;

public class cmoon_AI : MonoBehaviour
{   
    int updateTimer = 40;
	bool touchingGround = false,grounded = false,reachedPoint = true,Edge = false,dying = false,seesPlayer = false;
	public float xDetectOffset = 0.24f,yDetectOffset = 0.12f;
    float horizontalSpeed = 0f;
    Vector3 startPosition;
	public LayerMask whatIsGround;
    Vector3 targetPoint = Vector3.zero;
    Transform player;
    EnemyOffScreenDisabler eneOff;
	//EnemyCorpseSpawner eneCorpse;
    Animator anim;
    LayerMask playerMask;
    GameData data;
    Rigidbody2D rb;

    checkForSemiSolid checker;
	CompositeCollider2D semiSolid;
	public bool ignoreSemiSolid = false;
    public bool inverted = false;
    PlayerMusicBounce pBounce;
	PlayerScript pScript;
	playerSprite pSprites;
    SpriteRenderer render;
    public Sprite[] sprites;
    Gravity grav;
    Coroutine cor;
    int checkFrames = 5;
    IEnumerator flipdelay()
    {
        yield return new WaitForSeconds(0.2f);
        if(grounded)
        {
            flipGravity(false);
        }
        cor = null;
    }
    // Start is called before the first frame update
    void Start()
    {
        if(player==null)
		{
			player = GameObject.Find("Player_main").transform;
            pBounce = player.GetComponent<PlayerMusicBounce>();
			pSprites = player.GetComponent<playerSprite>();
			pScript = player.GetComponent<PlayerScript>();
            grav = GetComponent<Gravity>();
			playerMask |= (1 << LayerMask.NameToLayer("Player"));
			playerMask |= (1 << LayerMask.NameToLayer("Ground"));
			eneOff = GetComponent<EnemyOffScreenDisabler>();
            render = transform.GetChild(0).GetComponent<SpriteRenderer>();
			//eneCorpse = GetComponent<EnemyCorpseSpawner>();
			startPosition = transform.position;
			data = GameObject.Find("_GM").GetComponent<GameData>();
			rb = GetComponent<Rigidbody2D>();
			if(transform.childCount>1)
			checker = transform.GetChild(1).GetComponent<checkForSemiSolid>();
			semiSolid = GameObject.Find("SemiSolidMap").GetComponent<CompositeCollider2D>();
			anim = GetComponent<Animator>();
			//Debug.Log("Start");
			GetComponent<Crusher>().assignValues(whatIsGround,GetComponent<EnemyCorpseSpawner>(),data);
		}
    }
	void pointReached()
	{
		reachedPoint = true;
        horizontalSpeed = 0;
        rb.velocity=new Vector2(0,rb.velocity.y);
        anim.SetFloat("Speed",horizontalSpeed);
	}
    void LateUpdate()
    {
        if(pBounce.frame==0&&render.sprite==sprites[0])
        render.sprite = sprites[1];
    }
    // Update is called once per frame
    void Update()
    {
		if(eneOff.visible&&Time.timeScale!=0&&!dying)
		{
			groundDetect();
            if(grounded)
            {
				bool foundWall = wallDetect();
				if(grounded)
				edgeDetect();
                if(grounded&&SuperInput.GetKeyDown("Jump")&&cor==null&&seesPlayer&&scanForCeiling(Vector3.zero))
                    cor = StartCoroutine(flipdelay());
                if(checkFrames==0)
                {
					if(!pScript.dead)
					{
						findPlayer();

						if(seesPlayer)
						{
							if(reachedPoint&&Mathf.Round(targetPoint.y)<transform.position.y&&inverted&&scanForCeiling(Vector3.zero)
							||reachedPoint&&Mathf.Round(targetPoint.y)>transform.position.y&&!inverted&&scanForCeiling(Vector3.zero))
							{
								////print(Mathf.Abs(Mathf.Abs(transform.position.x)-Mathf.Abs(targetPoint.x)));
								if(Mathf.Abs(Mathf.Abs(transform.position.x)-Mathf.Abs(targetPoint.x))<=1f)
								flipGravity(true);
							}

							if(!foundWall)
							{
								if(Edge&&scanForCeiling(Vector3.zero)||!Edge)
								{
									////print("chasing");
									chasePlayer();
								}
							}
							else if(reachedPoint)
							{
								if(scanForCeilingAndWall(Vector3.zero))
								{
									//print("Test3");
									transform.localScale = new Vector3(-transform.localScale.x,transform.localScale.y,transform.localScale.z);
								}
							}
						}
					}

                    checkFrames = 5;
                }
                checkFrames--;
                //enemy stands when he reaches the goal point, for 60 frames, he scans for player, if he doesn't see him, he makes a lazy turn
                if(reachedPoint)
                {
                    if(updateTimer==0)
                    {
                        //Debug.Log("move now");
                        if(!seesPlayer)
                        findLazyWalkPoint();
                        updateTimer = 40;
                    }
                    updateTimer--;
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
                                //print("test");
                                pointReached();
                            }
                        }
                        else
                        {
                            if(transform.position.x<=targetPoint.x)
                            {
                                //print("test2");
                                pointReached();
                            }
                        }
                    }
                    else
                    {
                        //if lazy turn walk, just stop
                        if(horizontalSpeed<2f)
                        {
							//print("test3");
                            pointReached();
                        }
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
	void groundDetect()
	{
		Vector3 StartPoint1 = Vector3.zero;
		Vector3 StartPoint2 = Vector3.zero;
        if(!inverted)
        {
            StartPoint1 = new Vector3(transform.position.x-xDetectOffset,transform.position.y+yDetectOffset,transform.position.z);
		    StartPoint2 = new Vector3(transform.position.x+xDetectOffset,transform.position.y+yDetectOffset,transform.position.z);
        }
        else
        {
            StartPoint1 = new Vector3(transform.position.x-xDetectOffset,transform.position.y-yDetectOffset,transform.position.z);
		    StartPoint2 = new Vector3(transform.position.x+xDetectOffset,transform.position.y-yDetectOffset,transform.position.z);
        }
		RaycastHit2D ray1 = Physics2D.Raycast(StartPoint1,-transform.up,0.15f,whatIsGround);
		RaycastHit2D ray2 = Physics2D.Raycast(StartPoint2,-transform.up,0.15f,whatIsGround);
		
		Debug.DrawLine(StartPoint1,StartPoint1-transform.up*0.15f,Color.blue);
		Debug.DrawLine(StartPoint2,StartPoint2-transform.up*0.15f,Color.red);

		if(ray1.collider != null && !grounded && Time.timeScale!=0
		|| ray2.collider != null && !grounded && Time.timeScale!=0)
		{
			if(!inverted && rb.velocity.y <= 0f
			||  inverted && rb.velocity.y <= 0f)
			{
				if(ray1.collider != null)
				Debug.DrawLine(StartPoint1,ray1.point,Color.red,0.1f);
				if(ray2.collider != null)
				Debug.DrawLine(StartPoint2,ray2.point,Color.yellow,0.1f);
				////print("grounded");
				grounded = true;
				if(horizontalSpeed>4f)
				horizontalSpeed = 4f;

				if(seesPlayer&&!Edge)
				{
					horizontalSpeed = 3f;
					anim.speed = 1.5f;
					anim.SetFloat("Speed",horizontalSpeed);
				}
			}
		}
		else if(ray2.collider == null && ray1.collider == null && grounded && !touchingGround
		     || ray2.collider == null && ray1.collider == null && rb.velocity.y > 0.02f)
			 {
				////print("ungrounded");
				grounded = false;
			 }

		anim.SetBool("Grounded",grounded);
	}
	void edgeDetect()
	{
		Vector3 point = Vector3.zero;
        if(!inverted)
        point = new Vector3(transform.position.x+xDetectOffset*transform.localScale.x,transform.position.y+yDetectOffset,transform.position.z);
        else point = new Vector3(transform.position.x+xDetectOffset*transform.localScale.x,transform.position.y-yDetectOffset,transform.position.z);
		RaycastHit2D ray = Physics2D.Raycast(point,-transform.up,4f,whatIsGround);

		Debug.DrawLine(point,new Vector3(point.x,point.y-0.5f,point.z),Color.green);

		if(ray.collider==null&&!Edge)
		{
			if(!seesPlayer)
			{
				Edge = true;
				//print("Test2");
				pointReached();
			}
			//Debug.Log(gameObject.name+" Edge detected");
			else
			{
				if(!scanForCeiling(Vector3.zero)||!scanForCeiling(-transform.right))
				{
					Edge = true;
					//print("Test2");
					pointReached();
				}
				else
				{
					//print("edge flip");
					flipGravity(true);
				}
			}
		}
		if(ray.collider!=null&&Edge)
		{
			Edge = false;
		}
	}
	bool wallDetect()
	{
		Vector3 point = new Vector3(transform.position.x,transform.position.y+transform.up.y*0.4f,transform.position.z);
		RaycastHit2D ray1;
		if(!seesPlayer)
		{
			if(!inverted)
			ray1 = Physics2D.Raycast(point,transform.right*transform.localScale.x,0.5f,whatIsGround);
			else ray1 = Physics2D.Raycast(point,transform.right*transform.localScale.x*-1,0.5f,whatIsGround);
		}
		else
		{
			if(!inverted)
			ray1 = Physics2D.Raycast(point,transform.right*transform.localScale.x,1f,whatIsGround);
			else ray1 = Physics2D.Raycast(point,transform.right*transform.localScale.x*-1,1f,whatIsGround);
		}

		Debug.DrawLine(point,new Vector3(point.x+(1f*transform.localScale.x),point.y,point.z),Color.blue);
		if(ray1.collider!=null)
		{
			////print(ray1.point);
			if(seesPlayer&&!scanForCeilingAndWall(Vector3.zero)||!seesPlayer)
			{
				//if(seesPlayer)
				////print("Test");
				//print("test5");
				pointReached();
				return true;
			}
			else
			{
				////print("wall flip");
				if(grounded)
				{
					flipGravity(true);
					if(seesPlayer)
					chasePlayer();
				}
				return false;
			}
		}
        else return false;
	}
	bool scanForCeiling(Vector3 offset)
	{
		RaycastHit2D hit = Physics2D.Raycast(new Vector3(transform.position.x+offset.x,(transform.position.y+transform.up.y*0.4f)+offset.y,transform.position.z),transform.up,9f,whatIsGround);
		if(hit.collider!=null)Debug.DrawLine(new Vector3(transform.position.x+offset.x,(transform.position.y+transform.up.y*0.4f)+offset.y,transform.position.z),hit.point,Color.cyan,1f);
		if(hit.collider==null)
		{ 
			////print("no ceiling");
			return false;
		}
		else
		{
			////print("found ceiling");
			return true;
		}
	}
	bool scanForCeilingAndWall(Vector3 offset)
	{
		RaycastHit2D hit = Physics2D.Raycast(new Vector3(transform.position.x+offset.x,(transform.position.y+transform.up.y*0.4f)+offset.y,transform.position.z),transform.up,9f,whatIsGround);
		RaycastHit2D hit2 = Physics2D.Raycast(new Vector3(transform.position.x+offset.x-transform.right.x,(transform.position.y+transform.up.y*1.4f)+offset.y,transform.position.z),transform.up,9f,whatIsGround);
		if(hit.collider!=null)Debug.DrawLine(new Vector3(transform.position.x+offset.x,(transform.position.y+transform.up.y*0.4f)+offset.y,transform.position.z),hit.point,Color.magenta,1f);
		if(hit2.collider!=null)Debug.DrawLine(new Vector3(transform.position.x+offset.x-transform.right.x,(transform.position.y+transform.up.y*1.4f)+offset.y,transform.position.z),hit2.point,Color.green,1f);
		if(hit.collider==null)
		{ 
			////print("no ceiling");

			return false;
		}
		else if(hit.collider!=null&&hit2.collider!=null)
		{

			if(!inverted&&Mathf.Round(hit.point.y)>Mathf.Round(hit2.point.y)
			||  inverted&&Mathf.Round(hit.point.y)<Mathf.Round(hit2.point.y))
			{
				//print(Mathf.Round(hit.point.y)+" > "+Mathf.Round(hit2.point.y));
				//print("found wall, not flipping");
				return false;
			}
			else
			{
				return true;
			}
		}
		else
		{
			////print("found ceiling");
			return true;
		}
	}
    void findPlayer()
    {
		bool lostPlayer = false;
		Vector3 playerpos;
		if(pSprites.state==0||pScript.crouching)
		{
			playerpos = player.position;
		}
		else
		{
			playerpos = player.position+(player.up*0.75f);
		}
		if(seesPlayer&&horizontalSpeed==0)
		{
			if(transform.localScale.x==-1&&playerpos.x>transform.position.x
			||transform.localScale.x == 1&&playerpos.x<transform.position.x)
			transform.localScale=new Vector3(-transform.localScale.x,transform.localScale.y,transform.localScale.z);
		}
        Vector3 point = new Vector3(transform.position.x,transform.position.y+transform.up.y*1.5f,transform.position.z);
        RaycastHit2D ray;
        if(Mathf.Round(player.eulerAngles.z)==0)
        ray = Physics2D.Raycast(point,playerpos-point+(Vector3.up*0.5f),20f,playerMask);
        else ray = Physics2D.Raycast(point,playerpos-point-(Vector3.up*0.5f),20f,playerMask);
        if(ray.collider!=null)
        {
            //Debug.DrawLine(point,ray.point,Color.blue,1f/60*5);
            if(ray.collider.name=="PlayerCollider"&&!seesPlayer)
            {
				//print(ray.collider.name);
                Debug.DrawLine(point,ray.point,Color.blue,1f/60*5);
                seesPlayer = true;
                data.playSoundOverWrite(77,transform.position);
                if(!wallDetect())chasePlayer();
            }
            else if(ray.collider.name!="PlayerCollider"&&seesPlayer)
            {
                seesPlayer = false;
				lostPlayer = true;
            }
        }
        else
        {
            if(seesPlayer)seesPlayer = false;
        }

		if(lostPlayer&&!reachedPoint)
		{
			//print("test6");
			pointReached();
		}
    }
	IEnumerator dieInLava()
	{
		dying = true;
		yield return new WaitForSeconds(4f);
		Destroy(gameObject);
	}
    void findLazyWalkPoint()
	{
		//Debug.Log("pick new lazy point");
		reachedPoint = false;
		//move to the x point horizontally
		//lazy walk - find a random point behind the enemy
		targetPoint = new Vector3(transform.position.x+(Random.Range(2,4)*-1*transform.localScale.x),transform.position.y,transform.position.z);
		transform.localScale = new Vector3(-transform.localScale.x,transform.localScale.y,transform.localScale.z);
		horizontalSpeed = 2f;
        anim.speed = 1f;
		Debug.DrawLine(transform.position, targetPoint,Color.blue);
		anim.SetFloat("Speed",horizontalSpeed);
	}
    void chasePlayer()
	{
		reachedPoint = false;
		targetPoint = new Vector3(Mathf.Floor(player.position.x)+0.5f,player.position.y,player.position.z);
        if(targetPoint.x<transform.position.x)
        transform.localScale = new Vector3(-1,transform.localScale.y,transform.localScale.z);
        else transform.localScale = new Vector3(1,transform.localScale.y,transform.localScale.z);
		if(grounded&&!Edge)
		{
			horizontalSpeed = 3f;
			anim.speed = 1.5f;
			anim.SetFloat("Speed",horizontalSpeed);
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
        if(other.name=="deathZone")
		{
			if(GetComponent<shellScript>()!=null)
			{
				GetComponent<shellScript>().moving = false;
				StartCoroutine(GetComponent<shellScript>().returnToEnemy());
			}
			if(eneOff.visible)
			eneOff.toggle(false);
			
			eneOff.enableFlag = false;
			transform.position = startPosition;
			eneOff.testOffscreenSpawn();
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
    void flipGravity(bool environmental)
	{
		if(!environmental
		||environmental&&!inverted&&targetPoint.y>=transform.position.y
		||environmental&&inverted&&targetPoint.y<=transform.position.y)
		{
			data.playSoundOverWrite(78,transform.position);
			grounded = false;
			anim.SetBool("Grounded",grounded);
			float startRot = transform.eulerAngles.z;
			float targetRot = startRot+180;
			horizontalSpeed = 0;
			anim.SetFloat("Speed",horizontalSpeed);
			rb.velocity = Vector2.zero;

			if(!inverted) transform.position += Vector3.up*2;
			else transform.position -= Vector3.up*2;
			transform.eulerAngles=new Vector3(0,0,Mathf.Clamp(Mathf.Round(transform.eulerAngles.z)+targetRot,0,360));
			render.flipX = inverted;
			Vector2 newGrav = grav.pushForces;
			grav.pushForces = new Vector2(newGrav.x,-newGrav.y);
			inverted = !inverted;
			checkFrames = 0;
		}
	}
}
