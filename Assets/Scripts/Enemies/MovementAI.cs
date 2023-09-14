using System.Collections;
using UnityEngine;

public class MovementAI : MonoBehaviour {
	public bool rotating = false;
	public bool faceDirection = true;
	public int direction = 1;
	public bool walkOffEdges = true;
	public float speed = 1f,altSpeed = -1f;
	public float startSpeed = 0;
	public SpriteRenderer render;
	Rigidbody2D rb;
	bool leftEdge = false;
	bool rightEdge = false;
	public LayerMask whatIsGround;
	LayerMask whatIsSolidGround;
	public float rayCastLength = 0.3f;
	public bool facePlayerOnSpawn = false;
	public bool changeDirTowardsPlayer = false;
	public bool checkWallCollision = true;
	public float blockImpactJump = 12f;
	public bool sendContactInfo = false;
	public float collisionDetectOffset = 0.05f,crusherMaxHeight = 0,crusherDetectionOffset = 0,crusherDownLength = 0.3f;
	public float edgeDetectOffset = 0f;
	public bool destroyInLava = true;
	checkForSemiSolid checker;
	CompositeCollider2D semiSolid;
	public bool ignoreSemiSolid = false;
	public Vector3 startPosition;
	public bool returnToStartPointOnDeathZone = true;
	public bool canGetCrushed = true;
	public bool neverSleep = false;
	public bool inverted = false;
	public float directionInverter = 1;
	Transform player;
	[HideInInspector]
	public Jumper fireball;
	EnemyCorpseSpawner eneCorpse;
	Collider2D col1;
	Collider2D col2;
	public bool turnOffSemiSolidOnDisable = false;
	public bool constantCheckDir = false;
	bool inLava = false;
	GameData data;
	
	// Use this for initialization
	void Start () {
		if(rb==null)
		{
			startPosition = transform.position;
			rb = GetComponent<Rigidbody2D>();
			data = GameObject.Find("_GM").GetComponent<GameData>();
			if(render==null)
			render = transform.GetChild(0).GetComponent<SpriteRenderer>();
			if(transform.childCount>1)
			checker = transform.GetChild(1).GetComponent<checkForSemiSolid>();
			semiSolid = GameObject.Find("SemiSolidMap").GetComponent<CompositeCollider2D>();
			eneCorpse = GetComponent<EnemyCorpseSpawner>();
			col1 = GetComponent<Collider2D>();
			col2 = transform.GetChild(0).GetComponent<Collider2D>();
			if(col1!=null)
			{
			Physics2D.IgnoreCollision(col1,col1,true);
			Physics2D.IgnoreCollision(col1,col2,true);
			}
			player = GameObject.Find("Player").transform;
			if(crusherDetectionOffset==0)crusherDetectionOffset = collisionDetectOffset;
			if(altSpeed!=-1&&GameObject.Find("DataShare").GetComponent<dataShare>().mode==1)
			{
				speed = altSpeed;
			}
			startSpeed = speed;
			whatIsSolidGround = whatIsGround;
			whatIsSolidGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
			if(inverted)
			flipBoxes(inverted);

			//changeDirection(direction);
		}
	}
	public void setParentNull()
	{
		transform.parent = null;
	}
	public void flipBoxes(bool flip)
	{
		if(flip)
		{
			col1.offset = new Vector2(Mathf.Abs(col1.offset.x),col1.offset.y);
			if(col2!=null)
			col2.offset = new Vector2(Mathf.Abs(col2.offset.x),col2.offset.y);
		}
		else
		{
			col1.offset = new Vector2(-Mathf.Abs(col1.offset.x),col1.offset.y);
			if(col2!=null)
			col2.offset = new Vector2(-Mathf.Abs(col2.offset.x),col2.offset.y);
		}
	}
	void OnEnable()
	{
		if(rb==null)
		Start();
		if(Mathf.Round(transform.eulerAngles.z)!=0&&!inverted)
		{
			inverted=true;
		}
		else if(Mathf.Round(transform.eulerAngles.z)==0&&inverted)
		{
			inverted=false;
		}
		if(faceDirection)
		{
			render.flipX=inverted;
		}
		if(turnOffSemiSolidOnDisable)
		{
			ignoreSemiSolid = true;
			if(semiSolid!=null&&col1!=null)
			Physics2D.IgnoreCollision(semiSolid, col1,true);
			whatIsGround = whatIsSolidGround;
		}
		if(GetComponent<Gravity>()!=null)
		{
			Gravity grav = GetComponent<Gravity>();
			float pushForceY = Mathf.Abs(grav.pushForces.y);
			if(!inverted)pushForceY = -pushForceY;
			grav.pushForces = new Vector2(grav.pushForces.x,pushForceY);
		}
		if(facePlayerOnSpawn)
		{
			float xPos = player.position.x;
			if(xPos < transform.position.x)
			direction = -1;
			else direction = 1;

			if(faceDirection)
			{
				transform.localScale = new Vector3(direction*directionInverter,transform.localScale.y,transform.localScale.z);
				//print(transform.name+": "+transform.localScale.x);
			}
		}
		changeDirection((int)transform.localScale.x);
	}
	void OnDisable()
	{
		if(Application.isPlaying)
		{
			if(!turnOffSemiSolidOnDisable&&ignoreSemiSolid)
			{
				ignoreSemiSolid = false;
				if(semiSolid!=null&&col1!=null)
				Physics2D.IgnoreCollision(semiSolid, col1,false);
				whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
				//Debug.Log(gameObject.name+" Not ignoring collision");
			}
			else
			{
				if(turnOffSemiSolidOnDisable)
				{
					ignoreSemiSolid = true;
					if(semiSolid!=null&&col1!=null)
					Physics2D.IgnoreCollision(semiSolid, col1,true);
					whatIsGround = whatIsSolidGround;
				}
			}
		}
	}
	void crusher()
	{
		RaycastHit2D rayLeft = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5,0),-transform.right,0.3f,whatIsGround);
		RaycastHit2D rayRight = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5,0),transform.right,0.3f,whatIsGround);
		RaycastHit2D rayDown = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5,0),-transform.up,crusherDownLength,whatIsGround);
		RaycastHit2D rayUp = Physics2D.Raycast(transform.position+new Vector3(0,collisionDetectOffset*5+(crusherMaxHeight),0),transform.up,0.5f,whatIsSolidGround);

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
			else if(fireball!=null&&!fireball.exploding)
			{
				fireball.StartCoroutine(fireball.explode());
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
		if(constantCheckDir&&faceDirection&&transform.localScale.x!=direction*directionInverter)
		{
			transform.localScale = new Vector3(direction*directionInverter,transform.localScale.y,transform.localScale.z);
		}
		if(transform.GetChild(0).gameObject.activeInHierarchy&&canGetCrushed
		||neverSleep&&canGetCrushed)
		{
			crusher();
		}
	if(checker!=null)
	{
		if(!inverted)
		{
			if(rb.velocity.y>0&&!ignoreSemiSolid
			||rb.velocity.y<0&&!ignoreSemiSolid&&!checker.insideSemiSolid)
			{
				ignoreSemiSolid = true;
				Physics2D.IgnoreCollision(semiSolid, col1,true);
				whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
				//Debug.Log(gameObject.name+" Ignoring collision "+"Checker inside: "+checker.insideSemiSolid+" RbY: "+rb.velocity.y);
			}
			else if(rb.velocity.y<=0&&ignoreSemiSolid&&checker.insideSemiSolid
			||rb.velocity.y<=0&&!transform.GetChild(0).gameObject.activeInHierarchy)
			{
				ignoreSemiSolid = false;
				Physics2D.IgnoreCollision(semiSolid, col1,false);
				whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
				//Debug.Log(gameObject.name+" Not ignoring collision");
			}
		}
		else
		{
			if(rb.velocity.y<0&&!ignoreSemiSolid
			||rb.velocity.y>0&&!ignoreSemiSolid&&!checker.insideSemiSolid)
			{
				ignoreSemiSolid = true;
				Physics2D.IgnoreCollision(semiSolid, col1,true);
				whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
				//Debug.Log(gameObject.name+" Ignoring collision");
			}
			else if(rb.velocity.y>=0&&ignoreSemiSolid&&checker.insideSemiSolid
			||rb.velocity.y>=0&&!transform.GetChild(0).gameObject.activeInHierarchy)
			{
				ignoreSemiSolid = false;
				Physics2D.IgnoreCollision(semiSolid, col1,false);
				whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
				//Debug.Log(gameObject.name+" Not ignoring collision");
			}
		}
		if(changeDirTowardsPlayer&&((walkOffEdges)||!walkOffEdges&&leftEdge&&rightEdge))
		{
			if(player.position.x>=transform.position.x&&direction==-1
			||player.position.x<transform.position.x&&direction==1)
			{
				changeDirection(-direction);
			}
			if(transform.localScale.x!=direction*directionInverter)
			{
				transform.localScale = new Vector3(direction*directionInverter,1,1);
			}
		}
	}


		if(rotating&&Time.timeScale!=0 &&speed!=0)
		{
			if(!inverted)
			transform.GetChild(0).Rotate((Vector3.forward * speed * 2 * (-direction)));
			else transform.GetChild(0).Rotate((Vector3.forward * speed * 2 * direction));
		}
		if(speed!=0&&rb.bodyType!=RigidbodyType2D.Static)
		rb.velocity = new Vector2(speed*direction,rb.velocity.y);

		if(!walkOffEdges)
		edgeDetect();
		else if(leftEdge ||rightEdge)
		{
			leftEdge = false;
			rightEdge = false;
		}
	}
	public void changeDirection(int dir)
	{
		direction = dir;
			if(faceDirection)
				transform.localScale = new Vector3(direction*directionInverter,transform.localScale.y,transform.localScale.z);
	}
	void edgeDetect()
	{
		Vector3 point,point2;
		float yOffset = 0.05f+edgeDetectOffset;if(inverted)yOffset=-yOffset;
		point = new Vector3(transform.position.x-0.2f,transform.position.y+yOffset,transform.position.z);
		point2 = new Vector3(transform.position.x+0.2f,transform.position.y+yOffset,transform.position.z);
		RaycastHit2D ray1 = Physics2D.Raycast(point,-transform.up,0.5f,whatIsGround);
		RaycastHit2D ray2 = Physics2D.Raycast(point2, -transform.up,0.5f,whatIsGround);
		if(!inverted)
		{
			Debug.DrawLine(point,new Vector3(point.x,point.y-0.5f,point.z),Color.green);
			Debug.DrawLine(point2,new Vector3(point2.x,point2.y-0.5f,point2.z),Color.yellow);
		}
		else
		{
			Debug.DrawLine(point,new Vector3(point.x,point.y+0.5f,point.z),Color.green);
			Debug.DrawLine(point2,new Vector3(point2.x,point2.y+0.5f,point2.z),Color.yellow);
		}

		if(ray1.collider==null&&!leftEdge)
		{
			leftEdge = true;
			if(!rightEdge)
			{
			//Debug.Log(transform.name+ " detected edge on the left");

			//turn around logic
			changeDirection(1);
			}
		}
		if(ray2.collider==null&&!rightEdge)
		{
			rightEdge = true;
			if(!leftEdge)
			{
			//Debug.Log(transform.name+ " detected edge on the right");

			//turn around logic
			changeDirection(-1);
			}
			
		}
		if(ray1.collider!=null&&leftEdge)
		{
			leftEdge = false;
		}
		if(ray2.collider!=null&&rightEdge)
		{
			rightEdge = false;
		}
	}
	void wallDetect(Vector2 contact)
	{
		RaycastHit2D ray1,ray2;
		Vector3 point = new Vector3(transform.position.x,contact.y,transform.position.z);
		if(!inverted)
		{
			ray1 = Physics2D.Raycast(point,-transform.right,rayCastLength,whatIsGround);
			ray2 = Physics2D.Raycast(point, transform.right,rayCastLength,whatIsGround);
		}
		else
		{
			ray1 = Physics2D.Raycast(point, transform.right,rayCastLength,whatIsGround);
			ray2 = Physics2D.Raycast(point,-transform.right,rayCastLength,whatIsGround);
		}
		if(!inverted)
		{
		Debug.DrawLine(point,new Vector3(point.x-rayCastLength,point.y,point.z),Color.red);
		Debug.DrawLine(point,new Vector3(point.x+rayCastLength,point.y,point.z),Color.blue);
		}
		else
		{
		Debug.DrawLine(point,new Vector3(point.x+rayCastLength,point.y,point.z),Color.red);
		Debug.DrawLine(point,new Vector3(point.x-rayCastLength,point.y,point.z),Color.blue);
		}

		if(ray1.collider!=null&&direction != 1f)
		{
			//if(ray1.collider.gameObject == gameObject)
			//	Debug.Log("Collided with itself");
			if(sendContactInfo)
			Debug.Log(transform.name+ " detected" + ray1.collider.name);

			//turn around logic
			changeDirection(1);
		}
		if(ray2.collider!=null&&direction != -1f)
		{
			//if(ray2.collider.gameObject == gameObject)
			//	Debug.LogError("Collided with itself");
			if(sendContactInfo)
			Debug.Log(transform.name+ " detected" + ray2.collider.name);

			//turn around logic
			changeDirection(-1);
			
		}
	}
	IEnumerator dieInLava()
	{
		yield return new WaitForSeconds(4f);
		Destroy(gameObject);
	}
	public void jump(bool invertJump)
	{
		if(rb.bodyType!=RigidbodyType2D.Static)
		rb.velocity = new Vector2(rb.velocity.x,blockImpactJump*(invertJump?-1:1));
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "BlockParent(Clone)"&&blockImpactJump !=0f)
		{
			float otherY = other.transform.position.y+(inverted?-1:1),pos = transform.position.y;
			float offset = otherY-pos;
			//print("Offset: "+offset);
			if(!inverted&&offset<=0.55f||inverted&&offset>=-0.55f)
			{
				jump(inverted);
			}
		}
		if(other.name=="InstantDeath"&&destroyInLava)
		{
			Jumper jump = GetComponent<Jumper>();
			if(jump!=null)
			{
				jump.StopAllCoroutines();
				jump.enabled = false;
			}
			if(GetComponent<Gravity>()!=null)
			{
			rb.velocity = Vector2.zero;
			Gravity grav = GetComponent<Gravity>();
			grav.pushForces = new Vector2(grav.pushForces.x,grav.pushForces.y/2);
			grav.maxVelocities = new Vector2(grav.maxVelocities.x,0.4f);
			}
			if(inLava)
			{
				inLava = true;
				data.spawnCheeseSplatterPoint(transform.position);
			}
			StartCoroutine(dieInLava());
			render.sortingLayerName = other.GetComponent<Renderer>().sortingLayerName;
			render.sortingOrder = other.GetComponent<Renderer>().sortingOrder-1;
			speed = 0;
		}
		if(other.name=="deathZone")
		{
			if(returnToStartPointOnDeathZone)
			{
				shellScript sc = GetComponent<shellScript>();
			//print("returns");
			if(sc!=null)
			{
				sc.moving = false;
				StartCoroutine(sc.returnToEnemy());
			}
			EnemyOffScreenDisabler eneDis = GetComponent<EnemyOffScreenDisabler>();
			if(eneDis.visible)
			eneDis.toggle(false);
			
			eneDis.enableFlag = false;
			transform.position = startPosition;
			eneDis.testOffscreenSpawn();
			}
			else if(destroyInLava)Destroy(gameObject);
		}
	}
	void OnCollisionStay2D(Collision2D other)
	{
		if(this.enabled&&checkWallCollision)
		{
		if(other.gameObject.tag == "Ground"
		||other.gameObject.tag == "semiSolid"&&!ignoreSemiSolid
		||other.gameObject.tag == "Harm"
		||other.gameObject.tag == "Spring"
		||other.gameObject.tag == "BigBlock")
		{
			for(int i = 0; i<other.contacts.Length;i++)
			{
					if(sendContactInfo)
						{
							Debug.Log(other.gameObject.tag);
							Debug.Log(other.contacts[i].point.y+" "+transform.position.y+collisionDetectOffset+" "+other.contacts.Length);
							Debug.DrawLine(transform.position,other.contacts[i].point,Color.white,0.3f);
						}
				if(other.contacts[i].point.y > transform.position.y+collisionDetectOffset&&!inverted
				||other.contacts[i].point.y < transform.position.y-collisionDetectOffset&&inverted)
				{
					if(sendContactInfo)
						{
							Debug.Log(Mathf.Round(other.contacts[i].point.y)+" "+Mathf.Round(transform.position.y));
							Debug.DrawLine(transform.position,other.contacts[i].point,Color.red,0.3f);
						}
					wallDetect(other.contacts[i].point);
					break;
				}
			}
		}
		}
	}
}
