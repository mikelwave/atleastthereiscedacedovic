using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Events;

public class Jumper : MonoBehaviour {
	#region jumper
	public bool sendContactInfo = false;
	public bool playBounceAnim = false,playBounceAnimOnImpact = false;
	public bool turnOffOffscreen = true;
	public bool trackPlayer = false;
	public AudioClip bounceSound;
	public float jump;
	public AudioClip impactSound = null;
	public float collisionDetectOffset = 0f;
	float impactAnimFrameTime = 0.025f;
	public Sprite[] impact;
	Rigidbody2D rb;
	public SimpleAnim2 anim2;
	MovementAI ai;
	public SpriteRenderer render;
	Gravity grav;
	bool inAir = false;
	public bool exploding = false;
	public float rayCastLength = 0.5f;
	public bool destroyOffscreen = false;
	public bool isEnemy = false;
	bool startIsEnemy = false;
	bool visible = false;
	bool parried = false;
	LayerMask whatIsGround;
	public Transform setParent;
	Color32 ballColor;
	public GameObject toSummonOnImpact;
	[HideInInspector]
	public GameData data;
	public bool inverted = false;
	public bool piercing = false;
	public bool parryable = true;
	Transform player;
	[HideInInspector]
	public GameObject summonedObject = null;
	public bool simpleRaycasts = true;
	public bool allCollisionTriggersImpact = false;
	public bool startWithVelo = false;
	public bool destroyOnImpact = false;
	[Header("Normal, Ceil, Floor, Round")]
	public String YSummonBehaviour = "Normal";
	public String XSummonBehaviour = "Normal";
	public Vector2 spawnOffset;
	int summonRotation;
	public int waitBetweenJumps = 0;
	int jumpWait = 0,reJumpCooldown = 0;
	bool canTurnOff = true;
	public bool alwaysOn = false;
	public bool explodeInLava = false;
	public bool burgerKills = false;
	string[] normBallCol = new string[]{"Boss","EnemyCenterPivot","EnemyCustomPivot","Enemy","Disc","knockHarm","Harm","HarmTrigger","EnemyUnstompable","Enemy_Projectile","Poison"}; 
	Collider2D col;
	IEnumerator parryCooldown()
	{
		canTurnOff = false;
		yield return new WaitForSeconds(0.1f);
		canTurnOff = true;
	}
	public IEnumerator explode()
	{
		if(sendContactInfo)
		print("Exploding");
		canTurnOff = false;
		col.enabled = false;
		exploding = true;
		if(impactSound!=null)
		data.playUnlistedSound(impactSound);
		ai.enabled = false;
		anim2.enabled = false;
		if(grav!=null)
		grav.enabled = false;
		rb.bodyType = RigidbodyType2D.Static;
		for(int i = 0; i<impact.Length;i++)
		{
		render.sprite = impact[i];
		yield return new WaitForSeconds(impactAnimFrameTime);
		}
		if(setParent!=null)
			transform.SetParent(setParent);

		if(toSummonOnImpact!=null)
		{
			float Ypos = transform.position.y,Xpos = transform.position.x;
			switch(YSummonBehaviour)
			{
				default: break;
				case "Ceil": Ypos = Mathf.Ceil(Ypos); break; 
				case "Floor": Ypos = Mathf.Floor(Ypos); break; 
				case "Round": Ypos = Mathf.Round(Ypos); break; 
			}
			switch(XSummonBehaviour)
			{
				default: break;
				case "Ceil": Xpos = Mathf.Ceil(Xpos); break; 
				case "Floor": Xpos = Mathf.Floor(Xpos); break; 
				case "Round": Xpos = Mathf.Round(Xpos); break; 
			}
			Vector3 newPos = new Vector3(Xpos+spawnOffset.x,Ypos+spawnOffset.y,transform.position.z);
			summonedObject = Instantiate(toSummonOnImpact,newPos,Quaternion.Euler(0,0,summonRotation));
		}
		ExplodeEventTriggered();
		if(!destroyOnImpact)
		gameObject.SetActive(false);
		else
		Destroy(gameObject);
	}
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D>();
		if(anim2==null)
		{
			if(transform.GetChild(0).GetComponent<SimpleAnim2>()!=null)
				anim2 = transform.GetChild(0).GetComponent<SimpleAnim2>();
		}
		ai = GetComponent<MovementAI>();
		if(trackPlayer)
		player = GameObject.Find("Player").transform;
		if(render==null)
		render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		grav = GetComponent<Gravity>();
		ballColor = render.color;
		startIsEnemy = isEnemy;
		col = GetComponent<Collider2D>();
		GetComponent<MovementAI>().fireball = this;
		if(data==null)
		data = GameObject.Find("_GM").GetComponent<GameData>();

		if(!visible)
		{
			if(!turnOffOffscreen)
			{
				transform.GetChild(0).gameObject.SetActive(false);
				if(rb.bodyType != RigidbodyType2D.Static)
				{
					rb.bodyType = RigidbodyType2D.Static;
					grav.enabled = false;
				}
			}
		}
	}
	void OnEnable()
	{
		if(ai == null)
		Start();
		ai.enabled = true;
		if(Mathf.Round(transform.eulerAngles.z)!=0)
		inverted = true;
		else inverted = false;
		if(anim2!=null)
		{
			anim2.enabled = true;
		}
		rb.bodyType = RigidbodyType2D.Dynamic;
		if(grav!=null) grav.enabled = true;
		inAir = true;
		exploding = false;
		whatIsGround = ai.whatIsGround;
		if(startWithVelo)
		rb.velocity = new Vector2(rb.velocity.x,jump);
	}
	void OnDisable()
	{
		if(parried)
		{
			parried = false;
			isEnemy = startIsEnemy;
			render.color = ballColor;

			if(isEnemy)
			{
				transform.tag = "Enemy_Projectile";
				transform.GetChild(0).tag = "Enemy_Projectile";
				gameObject.layer = 19;
				transform.GetChild(0).gameObject.layer = 12;
			}
			else
			{
				transform.tag = "Fireball";
				transform.GetChild(0).tag = "Fireball";
				gameObject.layer = 18;
				transform.GetChild(0).gameObject.layer = 29;
			}
		}
		col.enabled = true;
		canTurnOff = true;
	}
	void Update()
	{
		if(!simpleRaycasts)
		wallDetect();
		else wallDetectSimple();
		if(!piercing)ceillingDetect();
		whatIsGround = ai.whatIsGround;
		if(Time.timeScale!=0)
		{
			if(waitBetweenJumps!=0)
			{
				if(jumpWait>0)
				{
					jumpWait--;
					if(jumpWait<=0&&reJumpCooldown<=0)
					{
						rejump();
						if(ai!=null)
							ai.speed = ai.startSpeed;
					}
				}
				if(reJumpCooldown>0)
				{
					reJumpCooldown--;
				}
			}
		}
	}
	void ceillingDetect()
	{
		Vector3 point = new Vector3(transform.position.x,transform.position.y,transform.position.z);
		RaycastHit2D ray;
		ray = Physics2D.Raycast(point,transform.up*transform.localScale.y*0.55f,rayCastLength,whatIsGround);
		if(ray.collider!=null&&impact.Length!=0&&!exploding) StartCoroutine(explode());
	}
	void wallDetect()
	{
		Vector3 pos = transform.position;
		float xStart = (0.4f*transform.localScale.x);
		float raycastLengthOffset = rayCastLength+0.4f;
		Vector3 point = new Vector3(pos.x-xStart,pos.y,pos.z);
		Vector3 point2 = new Vector3(pos.x-xStart,pos.y+0.5f,pos.z);
		Vector3 point3 = new Vector3(pos.x-xStart,pos.y-0.5f,pos.z);
		Vector2 box = new Vector2(0.01f,1.0f);
		RaycastHit2D ray1;
		if(!inverted)
		{
			ray1 = Physics2D.BoxCast(point,box,0,transform.right*transform.localScale.x,raycastLengthOffset,whatIsGround);
		}
		else
		{
			ray1 = Physics2D.BoxCast(point,box,0,transform.right*transform.localScale.x*-1,raycastLengthOffset,whatIsGround);
		}

		Debug.DrawLine(point,new Vector3(point.x+(raycastLengthOffset*transform.localScale.x),point.y,point.z),Color.blue);
		if(ray1.collider!=null)
		{
				if(impact.Length!=0&&!exploding)
				{	
					//if(ray1.collider!=null)print(ray1.collider.transform.name);
					//print(ai.ignoreSemiSolid);
					StartCoroutine(explode());
				}
		}
	}
	void wallDetectSimple()
	{
		Vector3 point = new Vector3(transform.position.x-(0.4f*transform.localScale.x),transform.position.y,transform.position.z);
		RaycastHit2D ray1;
		Vector2 box = new Vector2(0.01f,0.65f);
		if(!inverted)
		{
			ray1 = Physics2D.BoxCast(point,box,0,transform.right*transform.localScale.x,rayCastLength+0.4f,whatIsGround);
		}
		else
		{
			ray1 = Physics2D.BoxCast(point,box,0,transform.right*transform.localScale.x*-1,rayCastLength+0.4f,whatIsGround);
		}

		Debug.DrawLine(point,new Vector3(point.x+((rayCastLength+0.4f)*transform.localScale.x),point.y,point.z),Color.blue);
		if(ray1.collider!=null)
		{
				if(impact.Length!=0&&!exploding)
				{	
					StartCoroutine(explode());
				}
		}
	}
	void rejump()
	{
		if(waitBetweenJumps!=0)reJumpCooldown = 5;
		if(waitBetweenJumps==0||jumpWait<=0)
		{
			if(!inverted)
			{
				if(rb.bodyType != RigidbodyType2D.Static)
					rb.velocity = new Vector2(rb.velocity.x,jump);
			}
			else
			{
				if(rb.bodyType != RigidbodyType2D.Static)
					rb.velocity = new Vector2(rb.velocity.x,-jump);
			}
			if(playBounceAnim)
				anim2.StartPlaying();
			if(bounceSound!=null)
				data.playUnlistedSoundPoint(bounceSound,transform.position);
		}
	}
	void OnCollisionStay2D(Collision2D other)
	{
		String t = other.gameObject.tag;
		if(reJumpCooldown==0)
		{
			if(burgerKills&&!exploding&&jumpWait==0&&other.gameObject.name.Contains("Burger"))
			{
				StartCoroutine(explode());
			}

			if(t == "Boss" &&!isEnemy
			||t == "Ground"
			||t == "Harm"
			||t == "semiSolid"&&!ai.ignoreSemiSolid)
			{
				if(jumpWait==0)
				{
					if(!allCollisionTriggersImpact)
					{
						if(!inverted)
						for(int i = 0; i<other.contacts.Length;i++)
						{
							if(other.contacts[i].point.y <= transform.position.y+collisionDetectOffset&&inAir)
							{
								if(sendContactInfo)
								print(transform.name+" point: "+other.contacts[i].point+" <= "+(transform.position+new Vector3(0,collisionDetectOffset,0)));
								inAir = false;
								if(waitBetweenJumps==0)
								{
									rejump();
								}
								else
								{
									if(ai!=null)
									{
										if(playBounceAnimOnImpact)
										{
											if(impactSound!=null) data.playUnlistedSoundPoint(impactSound,transform.position);
											anim2.StartPlaying();
										}
										ai.speed = 0;
										if(rb.bodyType!=RigidbodyType2D.Static)
										rb.velocity = new Vector2(0,rb.velocity.y);
									}
									jumpWait = waitBetweenJumps;
								}

								if(trackPlayer)
								getPlayerPoint();
								JumpImpactEventTriggered();
								break;
							}
						}
						else
						for(int i = 0; i<other.contacts.Length;i++)
						{
							if(other.contacts[i].point.y >= transform.position.y-collisionDetectOffset&&inAir)
							{
								if(sendContactInfo)
								print(transform.name+" point: "+other.contacts[i].point+" <= "+(transform.position-new Vector3(0,collisionDetectOffset,0)));
								inAir = false;
								
								if(waitBetweenJumps==0)
								rejump();
								else
								{
									if(ai!=null)
									{
										if(playBounceAnimOnImpact)
										{
											if(impactSound!=null) data.playUnlistedSoundPoint(impactSound,transform.position);
											anim2.StartPlaying();
										}
										ai.speed = 0;
										rb.velocity = new Vector2(0,rb.velocity.y);
									}
									jumpWait = waitBetweenJumps;
								}

								if(trackPlayer)
								getPlayerPoint();
								JumpImpactEventTriggered();
								break;
							}
						}
					}
					else
					{
						if(allCollisionTriggersImpact)
						{
							for(int i = 0; i<other.contactCount;i++)
							{
								float xPoint = other.GetContact(i).point.x;
								//print("normal: "+transform.position+", other: "+xPoint+", id: "+i);
								//print(Mathf.Abs(transform.position.x)-Mathf.Abs(xPoint));
								//print(Mathf.Abs(xPoint)-Mathf.Abs(transform.position.x));
								if(transform.position.x>xPoint && Mathf.Abs(transform.position.x)-Mathf.Abs(xPoint)>0.116f)
								{
									//print("right");
									summonRotation = 270;
									spawnOffset = new Vector2(spawnOffset.x-0.5f,spawnOffset.y+0.5f);
								}
								else if(Mathf.Abs(xPoint)-Mathf.Abs(transform.position.x)>0.116f)
								{
									//print("left");
									summonRotation = 90;
									spawnOffset = new Vector2(spawnOffset.x+0.5f,spawnOffset.y+0.5f);
								}
								else
								{
									//above
									if(transform.position.y<other.GetContact(i).point.y)
									{
										//print("above");
										summonRotation = 180;
										spawnOffset = new Vector2(spawnOffset.x,spawnOffset.y+1);
									}
								}
							}
						}
						
						if(!exploding)
						StartCoroutine(explode());
					}
				}
			}
		}
	}
	void getPlayerPoint()
	{
		if(player!=null)
		{
			if(player.position.x>=transform.position.x&&ai.direction==-1
			||player.position.x<transform.position.x&&ai.direction==1)
			{
				ai.changeDirection(-ai.direction);
			}
		}
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "ObjectActivator"&&!visible)
		{
			visible = true;
			if(turnOffOffscreen)
			transform.SetParent(null);
			else
			{
				if(rb.bodyType != RigidbodyType2D.Dynamic)
				{
					rb.bodyType = RigidbodyType2D.Dynamic;
					grav.enabled = true;
				}
			}
			transform.GetChild(0).gameObject.SetActive(true);
		}
		if(!exploding&&explodeInLava&&other.gameObject.name=="InstantDeath")
		{
			exploding = true;
			StartCoroutine(explode());
			data.spawnCheeseSplatterPoint(transform.position);
		}
		String t = other.gameObject.tag;
		if(!isEnemy&&!piercing)
		{
			if(Array.IndexOf(normBallCol,t)>=0)
			{
				if(sendContactInfo)
				print(t);
				if(impact.Length!=0)
				{
					exploding = true;
					StartCoroutine(explode());
				}

			}
			else if(parryable&&t == "enemyAxeAura"&&!parried)
			{
				StartCoroutine(parryCooldown());
				data.playSound(46,transform.position);
				isEnemy = true;
				parried = true;
				transform.tag = "Enemy_Projectile";
				transform.GetChild(0).tag = "Enemy_Projectile";
				transform.localScale = new Vector3(-transform.localScale.x,1,1);
				ai.direction = -ai.direction;
				render.color = new Color32(232,118,75,255);
				gameObject.layer = 13;
				transform.GetChild(0).gameObject.layer = 12;
			}
		}
		else
		{
			if(isEnemy&&parryable&&other.gameObject.name == "axeAura"&&!parried)
			{
				data.playSound(46,transform.position);
				isEnemy = false;
				parried = true;
				transform.tag = "Fireball";
				transform.GetChild(0).tag = "Fireball";
				transform.localScale = new Vector3(-transform.localScale.x,1,1);
				ai.direction = -ai.direction;
				render.color = new Color32(255,255,255,255);
				gameObject.layer = 18;
				transform.GetChild(0).gameObject.layer = 29;
			}
			else if(startIsEnemy&&parryable&&t == "Player" && !parried &&!piercing
			||!startIsEnemy&&parryable&&t == "Player" && parried &&!piercing
			||t == "Fireball" &&!piercing)
			{
				if(impact.Length!=0)
				{
					StartCoroutine(explode());
				}
			}
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator"&&visible&&canTurnOff&&!alwaysOn)
		{
			visible = false;
			if(setParent!=null)
				transform.SetParent(setParent);
			if(!destroyOffscreen)
			{
				if(!turnOffOffscreen)
				{
					transform.GetChild(0).gameObject.SetActive(false);
					if(rb.bodyType != RigidbodyType2D.Static)
					{
						rb.bodyType = RigidbodyType2D.Static;
						grav.enabled = false;
					}
				}
				else
				{
					gameObject.SetActive(false);
				}
			}
			else Destroy(gameObject);
		}
	}
	void OnCollisionExit2D(Collision2D other)
	{
		String t = other.gameObject.tag;
		if(t == "Ground"||t == "Harm"||t == "semiSolid"&&ai.ignoreSemiSolid)
		{
			inAir = true;
		}
	}
	#endregion
	//my event
     [Serializable]
     public class ExplodeEvent : UnityEvent { }
 
     [SerializeField]
     private ExplodeEvent explodeEvent = new ExplodeEvent();
     public ExplodeEvent onExplodeEvent { get { return explodeEvent; } set { explodeEvent = value; } }
 
     public void ExplodeEventTriggered()
     {
         onExplodeEvent.Invoke();
     }
	[Serializable]
     public class JumpImpactEvent : UnityEvent { }
 
     [SerializeField]
     private JumpImpactEvent jumpImpactEvent = new JumpImpactEvent();
     public JumpImpactEvent onJumpImpactEvent { get { return jumpImpactEvent; } set { jumpImpactEvent = value; } }
 
     public void JumpImpactEventTriggered()
     {
         onJumpImpactEvent.Invoke();
     }
}
